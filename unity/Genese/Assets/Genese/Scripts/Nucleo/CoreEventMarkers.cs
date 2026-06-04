using System.Collections.Generic;
using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    /// <summary>
    /// Planta bandeiras/marcos visuais no mapa quando um evento é resolvido.
    /// Cada tipo de evento tem cor e tamanho próprios; os marcadores duram
    /// <see cref="duration"/> segundos e desaparecem gradualmente.
    /// </summary>
    public class CoreEventMarkers : MonoBehaviour
    {
        public CoreSim       sim;
        public CoreWorldView world;
        [Tooltip("Segundos que cada marcador fica visível")]
        public float duration = 30f;

        class Marker
        {
            public GameObject go;
            public Light      lt;
            public float      timeLeft;
            public float      maxTime;
        }

        readonly List<Marker> _markers = new List<Marker>();
        int _lastLogCount = 0;
        int _worldVersion = -1;

        void Update()
        {
            if (sim?.Sim == null || world == null || !world.Ready) return;

            if (sim.WorldVersion != _worldVersion)
            {
                foreach (var m in _markers) if (m.go) Destroy(m.go);
                _markers.Clear();
                _spawnCooldown.Clear();
                _lastLogCount = 0;
                _worldVersion = sim.WorldVersion;
                return;
            }

            // Detecta novos eventos no log
            var log = sim.Sim.Events.Log;
            for (int i = _lastLogCount; i < log.Count; i++)
                SpawnMarker(log[i]);
            _lastLogCount = log.Count;

            // Atualiza e remove marcadores expirados
            float dt = Time.deltaTime;
            for (int i = _markers.Count - 1; i >= 0; i--)
            {
                var m = _markers[i];
                if (m.go == null) { _markers.RemoveAt(i); continue; }

                m.timeLeft -= dt;
                if (m.timeLeft <= 0f) { Destroy(m.go); _markers.RemoveAt(i); continue; }

                // Fade nos últimos 6 segundos
                float t = Mathf.Clamp01(m.timeLeft / 6f);
                if (m.timeLeft < 6f)
                {
                    foreach (var mr in m.go.GetComponentsInChildren<MeshRenderer>())
                        if (mr.material != null) { var c = mr.material.color; c.a = t; mr.material.color = c; }
                    if (m.lt) m.lt.intensity = t * 2.2f;
                }

                // Leve balanço da bandeira
                m.go.transform.rotation = Quaternion.Euler(0,
                    Mathf.Sin(Time.time * 1.8f + i) * 4f, 0);
            }
        }

        // Cooldown por (civId, tipo): evita spam de marcadores do mesmo evento
        readonly Dictionary<long, float> _spawnCooldown = new Dictionary<long, float>();
        const float SPAWN_CD = 15f;
        const int   MAX_MARKERS = 6;

        static long MKey(int civ, CG.EventType t) => ((long)civ << 32) | (uint)t;

        void SpawnMarker(CG.GameEvent ev)
        {
            var civ = sim.Sim.Civs.Find(c => c.Id == ev.CivId);
            if (civ == null) return;

            // Limita quantidade e cooldown
            if (_markers.Count >= MAX_MARKERS) return;
            long mk = MKey(ev.CivId, ev.Type);
            if (_spawnCooldown.TryGetValue(mk, out float last) && Time.time - last < SPAWN_CD) return;
            _spawnCooldown[mk] = Time.time;

            Color col; float poleH;
            switch (ev.Type)
            {
                case CG.EventType.Seca:            col=new Color(0.85f,0.55f,0.10f); poleH=2.8f; break;
                case CG.EventType.Fome:            col=new Color(0.90f,0.18f,0.10f); poleH=2.8f; break;
                case CG.EventType.ColapsoPop:      col=new Color(0.70f,0.05f,0.05f); poleH=3.5f; break;
                case CG.EventType.GuerraDeclarada: col=new Color(0.95f,0.12f,0.08f); poleH=3.8f; break;
                case CG.EventType.Transcendencia:  col=new Color(0.80f,0.60f,1.00f); poleH=4.2f; break;
                case CG.EventType.Expansao:        col=new Color(0.20f,0.88f,0.42f); poleH=3.0f; break;
                case CG.EventType.Fusao:           col=new Color(0.40f,0.72f,1.00f); poleH=3.5f; break;
                default:                           col=Color.white;                   poleH=2.5f; break;
            }

            Vector3 worldPos = CivCenter(civ);

            var root = new GameObject($"Evento_{ev.Type}_C{ev.CivId}").transform;
            root.SetParent(transform, false);
            root.position = worldPos;

            BuildFlagpole(root, col, poleH, ev.Type);

            // Luz pontual colorida
            var lgo = new GameObject("luz").transform;
            lgo.SetParent(root, false);
            lgo.localPosition = new Vector3(0, poleH * 0.75f, 0);
            var lt = lgo.gameObject.AddComponent<Light>();
            lt.type = LightType.Point; lt.color = col;
            lt.range = ev.Type == CG.EventType.GuerraDeclarada ? 18f : 12f;
            lt.intensity = 2.2f;

            _markers.Add(new Marker { go = root.gameObject, lt = lt,
                                      timeLeft = duration, maxTime = duration });
        }

        void BuildFlagpole(Transform root, Color col, float h, CG.EventType type)
        {
            // Mastro fino e curto
            var poleMat = Prim.Mat(new Color(0.55f, 0.50f, 0.40f));
            var pole = Prim.Cylinder(root, poleMat);
            pole.transform.localScale    = new Vector3(0.045f, h * 0.5f, 0.045f);
            pole.transform.localPosition = new Vector3(0, h * 0.5f, 0);

            // Bandeira pequena e elegante
            var flagMat = new Material(Shader.Find("Standard"));
            flagMat.color = col;
            flagMat.EnableKeyword("_EMISSION");
            flagMat.SetColor("_EmissionColor", col * 0.6f);
            flagMat.SetFloat("_Glossiness", 0.7f);
            var flag = Prim.Cube(root, flagMat);
            flag.transform.localScale    = new Vector3(0.7f, 0.42f, 0.04f);
            flag.transform.localPosition = new Vector3(0.36f, h - 0.18f, 0);

            // Esfera de topo pequena e brilhante
            var topMat = Prim.Mat(col, Prim.Finish.Satin, col, 1.2f);
            var top = Prim.Sphere(root, topMat);
            top.transform.localScale    = Vector3.one * 0.14f;
            top.transform.localPosition = new Vector3(0, h + 0.08f, 0);
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
