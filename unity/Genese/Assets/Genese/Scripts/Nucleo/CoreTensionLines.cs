using System.Collections.Generic;
using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    /// <summary>
    /// Desenha arcos/linhas coloridos entre pares de civs conforme o nível de tensão:
    ///  • Amarelo tênue → baixo resentimento (avisar cedo)
    ///  • Laranja pulsante → tensão alta
    ///  • Vermelho intenso + partículas → GUERRA
    /// Visível no mapa sem abrir nenhum painel.
    /// </summary>
    public class CoreTensionLines : MonoBehaviour
    {
        public CoreSim       sim;
        public CoreWorldView world;

        const float MIN_TENSION = 0.18f;  // abaixo disso, linha invisível

        struct TensionLink
        {
            public LineRenderer line;
            public GameObject   orb;    // esfera central pulsante na guerra
            public Light        light;
        }

        readonly Dictionary<long, TensionLink> _links = new Dictionary<long, TensionLink>();
        readonly HashSet<long>                 _alive = new HashSet<long>();

        Material _lineMat;

        void Start()
        {
            _lineMat = new Material(Shader.Find("Particles/Standard Unlit")
                                 ?? Shader.Find("Unlit/Color")
                                 ?? Shader.Find("Standard"));
            _lineMat.color = Color.white;
        }

        void Update()
        {
            if (sim?.Sim == null || world == null || !world.Ready) return;

            var civs = sim.Sim.Civs;
            _alive.Clear();

            for (int i = 0; i < civs.Count; i++)
            for (int j = i + 1; j < civs.Count; j++)
            {
                var a = civs[i]; var b = civs[j];
                var rel = a.GetOrDefault(b.Id);
                float tension = rel.Resentment;

                if (tension < MIN_TENSION) { RemoveLink(i, j); continue; }

                long k = Key(i, j);
                _alive.Add(k);
                UpdateLink(k, a, b, rel, tension);
            }

            // Remove links não mais activos
            var dead = new List<long>();
            foreach (var k in _links.Keys) if (!_alive.Contains(k)) dead.Add(k);
            foreach (var k in dead) RemoveLink(k);
        }

        void UpdateLink(long k, CG.Civilization a, CG.Civilization b,
                        CG.CivRelation rel, float tension)
        {
            if (!_links.TryGetValue(k, out var lk))
                lk = CreateLink(k);

            Vector3 pA = CivCenter(a) + Vector3.up * (2f + tension * 4f);
            Vector3 pB = CivCenter(b) + Vector3.up * (2f + tension * 4f);
            Vector3 mid = (pA + pB) * 0.5f + Vector3.up * Mathf.Lerp(2f, 8f, tension);

            // Arco suave com 12 pontos
            int segs = 12;
            lk.line.positionCount = segs + 1;
            for (int s = 0; s <= segs; s++)
            {
                float t = s / (float)segs;
                Vector3 pt = Vector3.Lerp(Vector3.Lerp(pA, mid, t), Vector3.Lerp(mid, pB, t), t);
                lk.line.SetPosition(s, pt);
            }

            // Cor: amarelo → laranja → vermelho
            bool atWar = rel.Stance == CG.CivStance.Guerra;
            Color col = tension < 0.45f
                ? Color.Lerp(new Color(1f,0.85f,0.1f,1f), new Color(1f,0.42f,0.05f,1f), tension/0.45f)
                : Color.Lerp(new Color(1f,0.42f,0.05f,1f), new Color(0.95f,0.05f,0.05f,1f), (tension-0.45f)/0.55f);

            float pulse = atWar ? 0.7f + 0.3f*Mathf.Sin(Time.time*5f) : 1f;
            lk.line.startColor = lk.line.endColor = col * pulse;
            float w = Mathf.Lerp(0.15f, 0.65f, tension) * pulse;
            lk.line.startWidth = lk.line.endWidth = w;

            // Orb central: aparece apenas em guerra
            if (atWar)
            {
                if (lk.orb == null) { /* criado em CreateLink */ }
                if (lk.orb != null)
                {
                    lk.orb.SetActive(true);
                    lk.orb.transform.position = mid;
                    float s2 = 0.5f + 0.2f*Mathf.Sin(Time.time*4f);
                    lk.orb.transform.localScale = Vector3.one * s2;
                    var mr = lk.orb.GetComponent<MeshRenderer>();
                    if (mr) mr.material.color = col * (1f + Mathf.Sin(Time.time*6f)*0.3f);
                }
                if (lk.light != null)
                {
                    lk.light.color     = col;
                    lk.light.intensity = 1.5f + Mathf.Sin(Time.time*5f)*0.5f;
                }
            }
            else
            {
                if (lk.orb   != null) lk.orb.SetActive(false);
                if (lk.light != null) lk.light.intensity = 0f;
            }

            _links[k] = lk;
        }

        TensionLink CreateLink(long k)
        {
            var go = new GameObject($"TensionLine_{k}");
            go.transform.SetParent(transform, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.material         = _lineMat;
            lr.useWorldSpace    = true;
            lr.shadowCastingMode= UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows   = false;

            // Orb central (sempre criado, ativo só em guerra)
            var orbGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orbGO.name = "warOrb";
            Destroy(orbGO.GetComponent<Collider>());
            orbGO.transform.SetParent(go.transform, false);
            var orbMat = new Material(Shader.Find("Standard"));
            orbMat.EnableKeyword("_EMISSION");
            orbGO.GetComponent<MeshRenderer>().material = orbMat;
            orbGO.SetActive(false);

            // Luz no orb
            var lgo = new GameObject("light");
            lgo.transform.SetParent(orbGO.transform, false);
            var lt = lgo.AddComponent<Light>();
            lt.type = LightType.Point; lt.range = 16f; lt.intensity = 0f;

            var link = new TensionLink { line = lr, orb = orbGO, light = lt };
            _links[k] = link;
            return link;
        }

        void RemoveLink(int a, int b) => RemoveLink(Key(a, b));
        void RemoveLink(long k)
        {
            if (!_links.TryGetValue(k, out var lk)) return;
            if (lk.line?.gameObject) Destroy(lk.line.gameObject);
            _links.Remove(k);
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

        static long Key(int a, int b) => ((long)Mathf.Min(a,b) << 32) | (uint)Mathf.Max(a,b);
    }
}
