using System.Collections.Generic;
using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    /// <summary>
    /// Sincroniza criaturas renderizadas com TODAS as civilizações do núcleo (E08).
    /// Cada civ tem uma cor de tint diferente para distinguir visualmente.
    /// </summary>
    public class CoreCreatureView : MonoBehaviour
    {
        public CoreSim sim;
        public CoreWorldView world;
        public int maxRender = 160;

        readonly Dictionary<int, (GameObject go, int civId)> _views = new();
        readonly HashSet<int> _present = new();
        int _worldVersion = -1;

        // Cores de tint por civ (0 = neutro/jogador, 1 = laranja, 2 = roxo, 3 = verde)
        static readonly Color[] CivTints =
        {
            Color.white,
            new Color(1.0f, 0.55f, 0.1f),   // laranja (civ 1)
            new Color(0.65f, 0.3f, 0.9f),    // roxo   (civ 2)
            new Color(0.2f, 0.85f, 0.4f),    // verde  (civ 3)
        };

        static Color TintFor(int civId) => CivTints[civId % CivTints.Length];

        public void MarkDirty(int id)
        {
            if (_views.TryGetValue(id, out var pair) && pair.go) { Destroy(pair.go); _views.Remove(id); }
        }

        void Update()
        {
            if (sim == null || sim.Sim == null || world == null || !world.Ready) return;

            if (sim.WorldVersion != _worldVersion)
            {
                foreach (var kv in _views) if (kv.Value.go) Destroy(kv.Value.go);
                _views.Clear();
                _worldVersion = sim.WorldVersion;
            }

            _present.Clear();
            int n = 0;

            // Itera sobre TODAS as civs (E08: mundo multi-civilização)
            for (int ci = 0; ci < sim.Sim.Civs.Count; ci++)
            {
                var civ = sim.Sim.Civs[ci];
                foreach (var c in civ.Pop.Creatures)
                {
                    if (n >= maxRender) break;
                    n++;
                    _present.Add(c.Id);

                    if (!_views.TryGetValue(c.Id, out var pair))
                    {
                        var go = CreatureBuilder.Build(GenomeMap.ToVisual(c.Genome));
                        go.transform.SetParent(transform, false);
                        var col = go.AddComponent<SphereCollider>();
                        col.radius = 0.6f; col.center = new Vector3(0, 0.6f, 0);
                        go.AddComponent<CoreCreatureTag>().Id = c.Id;
                        go.transform.position = world.WorldPos(c.X, c.Y);

                        // Tint por civ: aplica cor a todos os MeshRenderers da criatura
                        if (ci > 0)
                        {
                            var tint = TintFor(ci);
                            foreach (var mr in go.GetComponentsInChildren<MeshRenderer>())
                            {
                                if (mr.material != null)
                                {
                                    // Blended tint: mantém a cor original mas inclina para a cor da civ
                                    mr.material.color = Color.Lerp(mr.material.color, tint, 0.38f);
                                }
                            }
                        }

                        pair = (go, ci);
                        _views[c.Id] = pair;
                    }

                    var target = world.WorldPos(c.X, c.Y);
                    var tr = pair.go.transform;
                    tr.position = Vector3.Lerp(tr.position, target, 0.25f);
                    Vector3 dir = target - tr.position; dir.y = 0;
                    if (dir.sqrMagnitude > 1e-5f)
                        tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(dir), 0.2f);
                }
                if (n >= maxRender) break;
            }

            if (_views.Count > _present.Count)
            {
                var remove = new List<int>();
                foreach (var kv in _views) if (!_present.Contains(kv.Key)) remove.Add(kv.Key);
                foreach (var id in remove) { if (_views[id].go) Destroy(_views[id].go); _views.Remove(id); }
            }
        }
    }
}
