using System.Collections.Generic;
using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    /// <summary>
    /// Detecta civs em risco de colapso e emite sinais visuais no mundo 3D sem texto:
    ///  • Anel vermelho pulsante e partículas circulares ao redor do grupo
    ///  • Luz vermelha no centro do grupo (intensidade proporcional ao perigo)
    ///  • Partículas de fumaça quando energia média está crítica
    /// Limiar: pop &lt; 6 OU energia média &lt; 0.22 OU fome ativa
    /// </summary>
    public class CoreCollapseWarning : MonoBehaviour
    {
        public CoreSim       sim;
        public CoreWorldView world;

        const float POP_THRESHOLD    = 5f;
        const float ENERGY_THRESHOLD = 0.20f;
        const int   RING_SEGMENTS    = 14;
        const ulong MIN_TICK         = 80UL; // não aciona nos primeiros ticks

        class Warning
        {
            public GameObject root;
            public Light      light;
            public GameObject[] rings;  // dois anéis em ângulos diferentes
            public GameObject[] sparks; // esferas pequenas que circulam
        }

        readonly Dictionary<int, Warning> _warnings = new Dictionary<int, Warning>();
        int _worldVer = -1;

        void Update()
        {
            if (sim?.Sim == null || world == null || !world.Ready) return;

            if (sim.WorldVersion != _worldVer)
            {
                foreach (var w in _warnings.Values) if (w.root) Destroy(w.root);
                _warnings.Clear();
                _worldVer = sim.WorldVersion;
                return;
            }

            // Aguarda os primeiros ticks para a população se estabelecer
            if (sim.Sim.Tick < MIN_TICK) return;

            var seen = new HashSet<int>();

            foreach (var civ in sim.Sim.Civs)
            {
                int  pop   = civ.Pop.Count;
                bool alive = pop > 0;
                if (!alive) { RemoveWarning(civ.Id); continue; }

                // Calcula energia média
                float totalE = 0f; int ne = 0;
                foreach (var c in civ.Pop.Creatures)
                    if (c.Alive) { totalE += c.Energy; ne++; }
                float avgE = ne > 0 ? totalE / ne : 0f;

                // Verifica se há evento de fome ou colapso ativo
                bool hasCollapse = false;
                foreach (var ev in sim.Sim.Events.Active)
                    if (ev.CivId == civ.Id &&
                        (ev.Type == CG.EventType.Fome || ev.Type == CG.EventType.ColapsoPop))
                    { hasCollapse = true; break; }

                float danger = 0f;
                if (pop < POP_THRESHOLD)    danger = Mathf.Max(danger, 1f - pop / POP_THRESHOLD);
                if (avgE < ENERGY_THRESHOLD) danger = Mathf.Max(danger, 1f - avgE / ENERGY_THRESHOLD);
                if (hasCollapse)             danger = Mathf.Max(danger, 0.7f);

                if (danger < 0.15f) { RemoveWarning(civ.Id); continue; }

                seen.Add(civ.Id);
                Vector3 center = CivCenter(civ);
                UpdateWarning(civ.Id, center, danger);
            }

            var dead = new List<int>();
            foreach (var id in _warnings.Keys) if (!seen.Contains(id)) dead.Add(id);
            foreach (var id in dead) RemoveWarning(id);
        }

        void UpdateWarning(int civId, Vector3 center, float danger)
        {
            if (!_warnings.TryGetValue(civId, out var w))
                w = CreateWarning(civId);

            w.root.transform.position = center;

            // Pulso: acelera com o perigo
            float pulseRate = 1.5f + danger * 4f;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseRate * Mathf.PI);

            // Luz vermelha central
            if (w.light)
            {
                w.light.intensity = Mathf.Lerp(0.6f, 3.5f, danger) * pulse;
                w.light.range     = Mathf.Lerp(8f, 20f, danger);
                w.light.color     = Color.Lerp(new Color(1f,0.5f,0.1f), new Color(0.95f,0.05f,0.05f), danger);
            }

            // Anéis rotativos (discos finos no plano XZ)
            for (int r = 0; r < w.rings.Length; r++)
            {
                if (w.rings[r] == null) continue;
                float rotSpeed = (50f + danger * 90f) * (r == 0 ? 1f : -1.3f);
                w.rings[r].transform.Rotate(0, rotSpeed * Time.deltaTime, 0, Space.World);

                // Disco fino: diâmetro cresce com o perigo, espessura fixa e pequena
                float diam = Mathf.Lerp(3.5f, 6f, danger) * (0.92f + 0.08f * pulse);
                w.rings[r].transform.localScale = new Vector3(diam, 0.06f, diam);

                // Cor dos anéis
                var mr = w.rings[r].GetComponent<MeshRenderer>();
                if (mr)
                {
                    Color rc = Color.Lerp(new Color(1f,0.65f,0.05f), new Color(0.95f,0.05f,0.05f), danger);
                    rc.a = 0.5f + 0.4f * pulse;
                    mr.material.color = rc;
                    mr.material.SetColor("_EmissionColor", rc * (0.5f + pulse * 0.8f));
                }
            }

            // Faíscas/esferas circulando
            float t = Time.time * (0.8f + danger * 1.2f);
            float radius = Mathf.Lerp(4f, 7f, danger);
            for (int s = 0; s < w.sparks.Length; s++)
            {
                if (w.sparks[s] == null) continue;
                float a = t + s / (float)w.sparks.Length * Mathf.PI * 2f;
                float h = center.y + 0.8f + Mathf.Sin(t * 1.3f + s) * 1.2f;
                w.sparks[s].transform.position = new Vector3(
                    center.x + Mathf.Cos(a) * radius,
                    h,
                    center.z + Mathf.Sin(a) * radius);
                float ss = (0.12f + danger * 0.18f) * (0.8f + 0.2f * pulse);
                w.sparks[s].transform.localScale = Vector3.one * ss;

                var mr = w.sparks[s].GetComponent<MeshRenderer>();
                if (mr)
                {
                    Color sc = Color.Lerp(new Color(1f,0.8f,0.1f), new Color(1f,0.1f,0.05f), danger);
                    mr.material.color = sc;
                    mr.material.SetColor("_EmissionColor", sc * 1.2f);
                }
            }
        }

        Warning CreateWarning(int civId)
        {
            var root = new GameObject($"CollapseWarning_{civId}");
            root.transform.SetParent(transform, false);

            // Luz vermelha
            var lgo = new GameObject("light");
            lgo.transform.SetParent(root.transform, false);
            var lt = lgo.AddComponent<Light>();
            lt.type = LightType.Point; lt.color = Color.red;

            // Dois discos finos rotativos (cilindro com altura mínima)
            var rings = new GameObject[2];
            for (int r = 0; r < 2; r++)
            {
                var rgo = Prim.Cylinder(root.transform,
                    MakeEmissiveMat(new Color(1f, 0.3f, 0.05f)));
                rgo.name = $"ring{r}";
                Destroy(rgo.GetComponent<Collider>());
                // Disco fino: escala Y muito pequena para não virar cilindro alto
                rgo.transform.localScale = new Vector3(4f, 0.06f, 4f);
                rgo.transform.localPosition = new Vector3(0, r * 1.2f + 0.5f, 0);
                rings[r] = rgo;
            }

            // Faíscas circulantes
            int nSparks = 6;
            var sparks = new GameObject[nSparks];
            for (int s = 0; s < nSparks; s++)
            {
                var sgo = Prim.Sphere(root.transform, MakeEmissiveMat(new Color(1f,0.5f,0.05f)));
                sgo.name = $"spark{s}";
                Destroy(sgo.GetComponent<Collider>());
                sparks[s] = sgo;
            }

            var w = new Warning { root = root, light = lt, rings = rings, sparks = sparks };
            _warnings[civId] = w;
            return w;
        }

        static Material MakeEmissiveMat(Color c)
        {
            var m = new Material(Shader.Find("Standard"));
            m.color = c;
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", c * 0.8f);
            m.SetFloat("_Glossiness", 0.6f);
            return m;
        }

        void RemoveWarning(int id)
        {
            if (!_warnings.TryGetValue(id, out var w)) return;
            if (w.root) Destroy(w.root);
            _warnings.Remove(id);
        }

        Vector3 CivCenter(CG.Civilization civ)
        {
            float sx = 0, sy = 0; int n = 0;
            foreach (var c in civ.Pop.Creatures)
                if (c.Alive) { sx += c.X; sy += c.Y; n++; }
            if (n == 0) { sx = civ.SpawnX; sy = civ.SpawnY; }
            else { sx /= n; sy /= n; }
            return world != null ? world.WorldPos(sx, sy) : Vector3.zero;
        }
    }
}
