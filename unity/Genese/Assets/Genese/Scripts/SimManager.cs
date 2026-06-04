using System.Collections.Generic;
using UnityEngine;

namespace Genese
{
    /// <summary>
    /// Estado-vitrine + agentes + nudges + ciclo dia/noite + regeneração por bioma.
    /// HUD em IMGUI (dev). NÃO é o motor real (Etapa 1) — só dá vida ao termômetro.
    /// </summary>
    public class SimManager : MonoBehaviour
    {
        [Header("Refs (ligadas pelo Bootstrap)")]
        public WorldBuilder world;
        public Light sun, hemiFill, influence, fire;
        public Camera cam;

        readonly List<Agent> agents = new();
        readonly List<string> biomeKeys = new(WorldData.Biomes.Keys);
        readonly List<string> cultureKeys = new(WorldData.Cultures.Keys);
        int biomeIdx = 0, cultureIdx = 0;

        float attention = 62f, cycle = 0f, infl = 0f;
        float dayT = 0.85f, dayTarget = 0.85f;   // 1 = dia, 0 = noite
        bool playing = true;
        string mode = null;
        Vector3 downPos; bool downValid;
        Genome species;        // genoma-base da espécie atual (por bioma)
        CameraOrbit orbit;

        // cores de "dia" do bioma atual (p/ lerp dia↔noite)
        Color skyDay, fogDay, ambSkyDay, ambEquDay, ambGndDay;
        static readonly Color NightSky = new Color(0.10f, 0.09f, 0.15f);
        static readonly Color NightFog = new Color(0.08f, 0.07f, 0.12f);
        static readonly Color NightAmb = new Color(0.05f, 0.05f, 0.09f);

        void Start()
        {
            orbit = cam ? cam.GetComponent<CameraOrbit>() : null;
            ApplyBiome();            // gera o mundo + define a espécie do bioma inicial
            Spawn(10);
        }

        public void Spawn(int n)
        {
            for (int i = 0; i < n; i++) AddCreature(RandomPos());
        }

        Vector3 RandomPos()
        {
            for (int i = 0; i < 16; i++)
            {
                float a = Random.value * 6.28f, r = Random.Range(2f, world.Radius * 0.6f);
                var p = new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r);
                if (world.GroundHeight(p.x, p.z) > world.WaterLevel + 0.35f) return p; // nasce em terra firme
            }
            return new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
        }

        Agent AddCreature(Vector3 pos)
        {
            var g = species != null ? Genome.Vary(species) : Genome.RandomGenome();
            var go = CreatureBuilder.Build(g);
            pos.y = world.GroundHeight(pos.x, pos.z);
            go.transform.position = pos;
            var ag = go.AddComponent<Agent>();
            ag.radius = world.Radius - 3f;
            agents.Add(ag);
            return ag;
        }

        void NewGeneration()
        {
            foreach (var ag in agents) if (ag) Destroy(ag.gameObject);
            agents.Clear(); Spawn(10);
        }

        void ApplyBiome()
        {
            world.Generate(biomeKeys[biomeIdx], cultureKeys[cultureIdx]);
            var key = biomeKeys[biomeIdx];
            var b = WorldData.Biomes[key];
            skyDay = b.SkyHorizon; fogDay = b.Fog;
            ambSkyDay = b.SkyTop; ambEquDay = b.Ground; ambGndDay = b.Ground * 0.5f;
            species = Genome.Species(key);     // nova espécie, combinando com o bioma
            if (fire) fire.color = WorldData.Cultures[cultureKeys[cultureIdx]].Flame;
        }

        void ChangeBiome(int d)   // troca de bioma → novo mundo + nova espécie (respawn)
        {
            biomeIdx = (biomeIdx + d + biomeKeys.Count) % biomeKeys.Count;
            ApplyBiome(); NewGeneration();
        }
        void ChangeCulture(int d) // troca de cultura → recolore a aldeia (mantém criaturas)
        {
            cultureIdx = (cultureIdx + d + cultureKeys.Count) % cultureKeys.Count;
            world.Generate(biomeKeys[biomeIdx], cultureKeys[cultureIdx]);
            if (fire) fire.color = WorldData.Cultures[cultureKeys[cultureIdx]].Flame;
        }

        void Update()
        {
            float dt = Time.deltaTime, now = Time.time;
            if (playing)
            {
                cycle += dt;
                if (attention < 100) attention = Mathf.Min(100, attention + dt * 1.5f);
                infl = Mathf.Max(0, infl - dt * 0.6f);
            }
            // dia/noite anda em direção ao alvo (funciona mesmo pausado)
            dayT = Mathf.MoveTowards(dayT, dayTarget, dt * 0.5f);

            // iluminação dependente de dia/noite + bioma
            float k = dayT;
            if (sun) sun.intensity = 0.2f + 1.3f * k;
            if (hemiFill) hemiFill.intensity = 0.08f + 0.4f * k;
            if (cam) cam.backgroundColor = Color.Lerp(NightSky, skyDay, k);
            RenderSettings.fogColor = Color.Lerp(NightFog, fogDay, k);
            RenderSettings.ambientSkyColor = Color.Lerp(NightAmb, ambSkyDay, k);
            RenderSettings.ambientEquatorColor = Color.Lerp(NightAmb, ambEquDay, k);
            RenderSettings.ambientGroundColor = Color.Lerp(NightAmb * 0.6f, ambGndDay, k);
            if (fire) fire.intensity = 2.0f + Mathf.Sin(now * 1.2f) * 0.5f + (1 - k) * 1.4f; // fogueira domina à noite
            if (influence) influence.intensity = infl * 3.5f;

            if (world.Flame) { var s = world.Flame.localScale; s.y = 0.7f * (1 + Mathf.Sin(now * 1.8f) * 0.18f); world.Flame.localScale = s; }
            if (world.Heart) { world.Heart.Rotate(0, dt * 46f, 0, Space.Self); var p = world.Heart.localPosition; p.y = 1.35f + Mathf.Sin(now * 0.18f) * 0.06f; world.Heart.localPosition = p; }

            HandleNudgeClick();
        }

        void HandleNudgeClick()
        {
            if (mode == null) return;
            if (Input.GetMouseButtonDown(0)) { downPos = Input.mousePosition; downValid = downPos.x < Screen.width - 264; }
            if (Input.GetMouseButtonUp(0) && downValid && Vector2.Distance(downPos, Input.mousePosition) < 12f)
            {
                var ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 500f)) ApplyNudge(hit.point);
            }
        }

        void ApplyNudge(Vector3 p)
        {
            if (attention < 14) return;
            attention = Mathf.Max(0, attention - 14);
            switch (mode)
            {
                case "sinal":
                    Fx.Signal(p); infl = 1f;
                    foreach (var ag in agents) if (ag && Vector3.Distance(ag.transform.position, p) < 3.2f) ag.Alert();
                    break;
                case "faisca":
                    Fx.Faisca(p);
                    Agent best = null; float bd = 1e9f;
                    foreach (var ag in agents) { float d = Vector3.Distance(ag.transform.position, p); if (d < bd) { bd = d; best = ag; } }
                    if (best && bd < 4f) { var pos = best.transform.position; int idx = agents.IndexOf(best); Destroy(best.gameObject); agents[idx] = AddCreatureAt(pos); }
                    break;
                case "inspiracao":
                    Fx.Inspiracao(p);
                    foreach (var ag in agents) if (ag && Vector3.Distance(ag.transform.position, p) < 4f) ag.Alert();
                    break;
            }
        }

        Agent AddCreatureAt(Vector3 pos) { var ag = AddCreature(pos); ag.Pop(); return ag; }

        // HUD de desenvolvimento (IMGUI) -----------------------------------------
        void OnGUI()
        {
            const int W = 252;
            GUILayout.BeginArea(new Rect(Screen.width - W - 12, 12, W, 380), GUI.skin.box);
            GUILayout.Label("<b>GÊNESE — Termômetro 3D (Unity)</b>");
            GUILayout.Label($"Ciclo {Mathf.FloorToInt(cycle)} · Pop. {agents.Count} · Atenção {Mathf.RoundToInt(attention)} · {(dayT > 0.5f ? "Dia" : "Noite")}");
            var bar = GUILayoutUtility.GetRect(W - 16, 10);
            GUI.Box(bar, GUIContent.none);
            GUI.Box(new Rect(bar.x, bar.y, bar.width * attention / 100f, bar.height), GUIContent.none);

            GUILayout.Space(6);
            GUILayout.Label("Nudges (selecione e clique no chão):");
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(mode == "sinal", "Sinal", GUI.skin.button)) mode = "sinal"; else if (mode == "sinal") mode = null;
            if (GUILayout.Toggle(mode == "faisca", "Faísca", GUI.skin.button)) mode = "faisca"; else if (mode == "faisca") mode = null;
            if (GUILayout.Toggle(mode == "inspiracao", "Inspiração", GUI.skin.button)) mode = "inspiracao"; else if (mode == "inspiracao") mode = null;
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Bioma: " + WorldData.Biomes[biomeKeys[biomeIdx]].Nome);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("‹ bioma")) ChangeBiome(-1);
            if (GUILayout.Button("bioma ›")) ChangeBiome(+1);
            GUILayout.EndHorizontal();

            GUILayout.Label("Cultura: " + WorldData.Cultures[cultureKeys[cultureIdx]].Nome);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("‹ cultura")) ChangeCulture(-1);
            if (GUILayout.Button("cultura ›")) ChangeCulture(+1);
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(playing ? "⏸ Pausar" : "▶ Rodar")) playing = !playing;
            if (GUILayout.Button(dayTarget > 0.5f ? "☾ Anoitecer" : "☀ Amanhecer")) dayTarget = dayTarget > 0.5f ? 0.1f : 0.9f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("⟳ Geração")) NewGeneration();
            if (orbit && GUILayout.Button("⌖ Topo")) orbit.elevation = orbit.elevation > 1.0f ? 0.6f : 1.4f;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
