using System.Collections.Generic;
using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    /// <summary>
    /// Renderiza criaturas de todas as civs com:
    ///  • Corpo gerado pelo CreatureBuilder (shapes, olhos, orelhas, etc.)
    ///  • Beacon esférico colorido por civ/energia acima do corpo
    ///  • Ícone de papel (coroa, seta, ovo, disco) que se atualiza quando o papel muda
    ///  • Luz pontual discreta
    /// </summary>
    public class CoreCreatureView : MonoBehaviour
    {
        public CoreSim       sim;
        public CoreWorldView world;
        public int           maxRender = 120;
        /// <summary>Índice da civ destacada (-1 = todas normais).</summary>
        public int HighlightCivIdx = -1;

        struct ViewEntry { public GameObject go; public int civId; }

        readonly Dictionary<int, ViewEntry>     _views    = new Dictionary<int, ViewEntry>();
        readonly HashSet<int>                   _present  = new HashSet<int>();
        readonly Dictionary<int, MeshRenderer>  _beacons  = new Dictionary<int, MeshRenderer>();
        readonly Dictionary<int, Light>         _lights   = new Dictionary<int, Light>();
        readonly Dictionary<int, GameObject>    _roleIcons= new Dictionary<int, GameObject>();
        readonly Dictionary<int, string>        _lastRole = new Dictionary<int, string>();

        int _worldVersion = -1;

        // Usa a mesma paleta do CoreHud para consistência visual
        static Color TintFor(int ci) => CoreHud.CivColors[ci % CoreHud.CivColors.Length];

        public void MarkDirty(int id)
        {
            if (_views.TryGetValue(id, out var e) && e.go) Destroy(e.go);
            _views.Remove(id); _beacons.Remove(id); _lights.Remove(id);
            _roleIcons.Remove(id); _lastRole.Remove(id);
        }

        // ─────────────────────────────────────────────────────────────────────
        void Update()
        {
            if (sim?.Sim == null || world == null || !world.Ready) return;

            if (sim.WorldVersion != _worldVersion)
            {
                foreach (var kv in _views) if (kv.Value.go) Destroy(kv.Value.go);
                _views.Clear(); _beacons.Clear(); _lights.Clear();
                _roleIcons.Clear(); _lastRole.Clear();
                _worldVersion = sim.WorldVersion;
            }

            _present.Clear();
            int n = 0;

            for (int ci = 0; ci < sim.Sim.Civs.Count; ci++)
            {
                Color civCol = TintFor(ci);
                foreach (var c in sim.Sim.Civs[ci].Pop.Creatures)
                {
                    if (n >= maxRender) break;
                    n++;
                    _present.Add(c.Id);

                    // ── Criação ──────────────────────────────────────────────
                    if (!_views.TryGetValue(c.Id, out var entry))
                    {
                        var go = CreatureBuilder.Build(GenomeMap.ToVisual(c.Genome));
                        go.transform.SetParent(transform, false);
                        var col = go.AddComponent<SphereCollider>();
                        col.radius = 0.6f; col.center = new Vector3(0, 0.6f, 0);
                        go.AddComponent<CoreCreatureTag>().Id = c.Id;
                        go.transform.position = world.WorldPos(c.X, c.Y);

                        // Tint de civ
                        if (ci > 0)
                            foreach (var mr in go.GetComponentsInChildren<MeshRenderer>())
                                if (mr.material != null)
                                    mr.material.color = Color.Lerp(mr.material.color, civCol, 0.35f);

                        // Beacon esférico
                        var bGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        bGO.name = "beacon";
                        Destroy(bGO.GetComponent<Collider>());
                        bGO.transform.SetParent(go.transform, false);
                        float bH = c.IsFigure ? 3.8f : 3.0f;
                        bGO.transform.localScale    = Vector3.one * (c.IsFigure ? 0.55f : 0.38f);
                        bGO.transform.localPosition = new Vector3(0, bH, 0);
                        var bMat = new Material(Shader.Find("Standard"));
                        bMat.color = civCol;
                        bMat.EnableKeyword("_EMISSION");
                        bMat.SetColor("_EmissionColor", civCol * 0.85f);
                        bMat.SetFloat("_Glossiness", 0.85f);
                        bGO.GetComponent<MeshRenderer>().material = bMat;
                        _beacons[c.Id] = bGO.GetComponent<MeshRenderer>();

                        // Luz pontual
                        var lGO = new GameObject("clight");
                        lGO.transform.SetParent(go.transform, false);
                        lGO.transform.localPosition = new Vector3(0, bH - 0.3f, 0);
                        var lt = lGO.AddComponent<Light>();
                        lt.type = LightType.Point; lt.color = civCol;
                        lt.range = c.IsFigure ? 8f : 5f;
                        lt.intensity = c.IsFigure ? 1.5f : 0.9f;
                        _lights[c.Id] = lt;

                        // Adiciona animação de bob/squash (demo "temperatura")
                        go.AddComponent<NucleoCreatureAnim>();

                        entry = new ViewEntry { go = go, civId = ci };
                        _views[c.Id] = entry;

                        // Ícone de papel (criado logo após)
                        string role = c.Role();
                        _roleIcons[c.Id] = BuildRoleIcon(go.transform, role, civCol, bH);
                        _lastRole[c.Id]  = role;
                    }

                    // ── Movimento com snap ao terreno ────────────────────────
                    var target = world.WorldPos(c.X, c.Y);
                    var tr = entry.go.transform;
                    Vector3 next = Vector3.Lerp(tr.position, target, 0.25f);
                    float ox = sim.Sim.Env.W * 0.5f, oz = sim.Sim.Env.H * 0.5f;
                    next.y = world.WorldPos(next.x / world.cellSize + ox,
                                           next.z / world.cellSize + oz).y;
                    tr.position = next;
                    Vector3 dir = target - next; dir.y = 0;
                    if (dir.sqrMagnitude > 1e-5f)
                        tr.rotation = Quaternion.Slerp(tr.rotation,
                                          Quaternion.LookRotation(dir), 0.2f);

                    // ── Beacon: cor por energia ───────────────────────────────
                    float e = c.Energy;
                    Color eCol = e > 0.6f ? civCol
                               : e > 0.3f ? Color.Lerp(new Color(1f,0.65f,0.1f), civCol, (e-0.3f)/0.3f)
                                           : Color.Lerp(new Color(0.9f,0.12f,0.1f), new Color(1f,0.65f,0.1f), e/0.3f);
                    // Destaque: dimeia criaturas de outras civs quando há seleção
                    bool dimmed = HighlightCivIdx >= 0 && ci != HighlightCivIdx;
                    float dimFactor = dimmed ? 0.18f : 1f;
                    Color visCol = dimmed ? eCol * 0.18f : eCol;

                    if (_beacons.TryGetValue(c.Id, out var bmr) && bmr)
                    {
                        bmr.material.color = visCol;
                        bmr.material.SetColor("_EmissionColor", visCol * (0.4f + e * 0.6f));
                        float scaleBase = c.IsFigure ? 0.55f : 0.38f;
                        float pulse = c.IsFigure && !dimmed ? 0.07f * Mathf.Sin(Time.time * 2.8f) : 0f;
                        bmr.transform.localScale = Vector3.one * (scaleBase + pulse);
                    }
                    if (_lights.TryGetValue(c.Id, out var lt2) && lt2)
                    {
                        lt2.color     = visCol;
                        lt2.intensity = (c.IsFigure ? 1.5f : 0.85f) * (0.4f + e * 0.6f) * dimFactor;
                    }

                    // ── Ícone de papel: recria se mudou ───────────────────────
                    string curRole = c.Role();
                    if (!_lastRole.TryGetValue(c.Id, out var prevRole) || prevRole != curRole)
                    {
                        if (_roleIcons.TryGetValue(c.Id, out var oldIcon) && oldIcon)
                            Destroy(oldIcon);
                        float bH2 = c.IsFigure ? 3.8f : 3.0f;
                        _roleIcons[c.Id] = BuildRoleIcon(entry.go.transform, curRole, TintFor(entry.civId), bH2);
                        _lastRole[c.Id]  = curRole;
                    }
                    // Ícone de líder pulsa suavemente
                    if (curRole == "Líder" && _roleIcons.TryGetValue(c.Id, out var icon) && icon)
                        icon.transform.localRotation = Quaternion.Euler(0, Time.time * 45f, 0);
                }
                if (n >= maxRender) break;
            }

            // Remove GOs de criaturas mortas
            if (_views.Count > _present.Count)
            {
                var rem = new List<int>();
                foreach (var kv in _views) if (!_present.Contains(kv.Key)) rem.Add(kv.Key);
                foreach (var id in rem)
                {
                    if (_views[id].go) Destroy(_views[id].go);
                    _views.Remove(id); _beacons.Remove(id); _lights.Remove(id);
                    _roleIcons.Remove(id); _lastRole.Remove(id);
                }
            }
        }

        // ── Ícones de papel ───────────────────────────────────────────────────
        /// <summary>Constrói o ícone 3D do papel acima do beacon.</summary>
        static GameObject BuildRoleIcon(Transform parent, string role, Color civCol, float beaconH)
        {
            var root = new GameObject("roleIcon");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0, beaconH + 0.55f, 0);

            switch (role)
            {
                case "Líder": BuildCrown(root.transform, civCol); break;
                case "Explorador": BuildArrow(root.transform, civCol); break;
                case "Parental": BuildEggs(root.transform, civCol); break;
                default: BuildDisc(root.transform, civCol); break; // Forrageiro
            }
            return root;
        }

        // Coroa: anel dourado com 5 pontas
        static void BuildCrown(Transform root, Color civCol)
        {
            var gold = new Color(1f, 0.85f, 0.25f);
            var mat  = new Material(Shader.Find("Standard"));
            mat.color = gold;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", gold * 1.8f);
            mat.SetFloat("_Glossiness", 0.9f);

            // Base circular
            var ring = Prim.Cylinder(root, mat);
            ring.transform.localScale    = new Vector3(0.55f, 0.06f, 0.55f);
            ring.transform.localPosition = Vector3.zero;

            // 5 pontas
            for (int i = 0; i < 5; i++)
            {
                float a = i / 5f * Mathf.PI * 2f;
                var tip = Prim.Cone(root, mat, 0.07f, 0.28f, 5);
                tip.transform.localPosition = new Vector3(
                    Mathf.Cos(a) * 0.22f, 0.17f, Mathf.Sin(a) * 0.22f);
            }

            // Gema central
            var gemMat = new Material(Shader.Find("Standard"));
            gemMat.color = civCol;
            gemMat.EnableKeyword("_EMISSION");
            gemMat.SetColor("_EmissionColor", civCol * 0.9f);
            var gem = Prim.Sphere(root, gemMat);
            gem.transform.localScale    = Vector3.one * 0.18f;
            gem.transform.localPosition = new Vector3(0, 0.1f, 0);
        }

        // Seta apontando para cima e para frente (Explorador)
        static void BuildArrow(Transform root, Color civCol)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = Color.Lerp(civCol, Color.white, 0.3f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", mat.color * 1.5f);
            mat.SetFloat("_Glossiness", 0.7f);

            var shaft = Prim.Cylinder(root, mat);
            shaft.transform.localScale    = new Vector3(0.08f, 0.22f, 0.08f);
            shaft.transform.localPosition = new Vector3(0, 0.1f, 0);

            var head = Prim.Cone(root, mat, 0.15f, 0.26f, 4);
            head.transform.localPosition = new Vector3(0, 0.38f, 0);
        }

        // Par de ovos/filhotes (Parental)
        static void BuildEggs(Transform root, Color civCol)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = Color.Lerp(civCol, new Color(1f, 0.95f, 0.85f), 0.45f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", mat.color * 1.4f);
            mat.SetFloat("_Glossiness", 0.8f);

            for (int i = -1; i <= 1; i += 2)
            {
                var egg = Prim.Sphere(root, mat);
                egg.transform.localScale    = new Vector3(0.18f, 0.22f, 0.18f);
                egg.transform.localPosition = new Vector3(i * 0.16f, 0.08f, 0);
            }
            // Minúscula esfera entre os ovos
            var mid = Prim.Sphere(root, mat);
            mid.transform.localScale    = Vector3.one * 0.11f;
            mid.transform.localPosition = new Vector3(0, 0.05f, 0);
        }

        // Disco achatado (Forrageiro) — emissivo para ser visível à noite
        static void BuildDisc(Transform root, Color civCol)
        {
            var mat = new Material(Shader.Find("Standard"));
            var col = Color.Lerp(civCol, Color.white, 0.2f);
            mat.color = col;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", col * 1.2f);
            mat.SetFloat("_Glossiness", 0.7f);

            var disc = Prim.Cylinder(root, mat);
            disc.transform.localScale    = new Vector3(0.40f, 0.05f, 0.40f);
            disc.transform.localPosition = new Vector3(0, 0.02f, 0);
        }
    }
}
