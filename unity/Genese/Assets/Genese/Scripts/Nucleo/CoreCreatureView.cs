using System.Collections.Generic;
using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    /// <summary>
    /// Sincroniza as criaturas RENDERIZADAS com a população do núcleo: cria a malha
    /// (CreatureBuilder, a partir do genoma) ao nascer, move suavemente para a posição
    /// do agente e destrói ao morrer. Limita quantas desenha (performance).
    /// </summary>
    public class CoreCreatureView : MonoBehaviour
    {
        public CoreSim sim;
        public CoreWorldView world;
        public int maxRender = 160;

        readonly Dictionary<int, GameObject> _views = new();
        readonly HashSet<int> _present = new();
        int _worldVersion = -1;

        /// <summary>Força recriar a malha de uma criatura (ex.: após mutação por faísca).</summary>
        public void MarkDirty(int id)
        {
            if (_views.TryGetValue(id, out var go)) { if (go) Destroy(go); _views.Remove(id); }
        }

        void Update()
        {
            if (sim == null || sim.Sim == null || world == null || !world.Ready) return;

            // novo mundo (semente/clima/cultura) → recomeça do zero
            if (sim.WorldVersion != _worldVersion)
            {
                foreach (var kv in _views) if (kv.Value) Destroy(kv.Value);
                _views.Clear();
                _worldVersion = sim.WorldVersion;
            }

            var pop = sim.Sim.Pop;
            _present.Clear();

            int n = 0;
            foreach (var c in pop.Creatures)
            {
                if (n >= maxRender) break;
                n++;
                _present.Add(c.Id);

                if (!_views.TryGetValue(c.Id, out var go))
                {
                    go = CreatureBuilder.Build(GenomeMap.ToVisual(c.Genome)); // arte do protótipo
                    go.transform.SetParent(transform, false);
                    var col = go.AddComponent<SphereCollider>(); col.radius = 0.6f; col.center = new Vector3(0, 0.6f, 0);
                    go.AddComponent<CoreCreatureTag>().Id = c.Id;
                    go.transform.position = world.WorldPos(c.X, c.Y);
                    _views[c.Id] = go;
                }

                var target = world.WorldPos(c.X, c.Y);
                var tr = go.transform;
                tr.position = Vector3.Lerp(tr.position, target, 0.25f);
                if ((target - tr.position).sqrMagnitude > 0.0004f)
                {
                    Vector3 dir = target - tr.position; dir.y = 0;
                    if (dir.sqrMagnitude > 1e-5f) tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(dir), 0.2f);
                }
            }

            // remove criaturas que morreram (ou saíram do limite de render)
            if (_views.Count > _present.Count)
            {
                var remove = new List<int>();
                foreach (var kv in _views) if (!_present.Contains(kv.Key)) remove.Add(kv.Key);
                foreach (var id in remove) { Destroy(_views[id]); _views.Remove(id); }
            }
        }
    }
}
