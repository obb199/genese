using System;
using System.Collections.Generic;
using System.IO;

namespace Genese.Core
{
    public enum MemeType : byte { Valor, Tabu, Mito, Arte, Rito }

    /// <summary>
    /// Cultura emergente (M09). Memes — unidades de informação cultural — nascem de Figuras
    /// ou eventos, propagam-se por imitação proporcional ao prestígio e competem pela atenção
    /// coletiva. A lente cultural converte eventos em significado pelo argmax de força ×
    /// prevalência — nunca por sorteio arbitrário (GDD §1.5).
    /// </summary>
    public sealed class Culture
    {
        public struct Meme
        {
            public int      Id;
            public MemeType Type;
            public float    Force;       // intensidade intrínseca do meme
            public float    Rigidity;    // resistência à mudança e ao decaimento
            public float    Prevalence;  // fração da pop que porta este meme (0-1)
            public int      OriginId;    // Id da Figura/criatura que o originou
        }

        public readonly Dictionary<int, Meme> Pool = new();
        private int _nextId;

        /// <summary>Coesão cultural: 1 = unida, 0 = cisma — medida de dispersão das prevalências.</summary>
        public float CulturalCohesion = 1f;

        public int MemeCount => Pool.Count;

        /// <summary>
        /// Cria meme com origem causal (evento ou Figura — nunca espontâneo, M09 §4.1).
        /// </summary>
        public int SpawnMeme(MemeType type, float force, int originId, Rng rng)
        {
            int id = _nextId++;
            Pool[id] = new Meme
            {
                Id = id, Type = type, Force = Clamp01(force),
                Rigidity   = 0.25f + 0.5f * (float)rng.NextDouble(),
                Prevalence = 0.05f,
                OriginId   = originId
            };
            return id;
        }

        /// <summary>
        /// Propaga memes por imitação (M09 §4.1): Figuras com alto prestígio propagam mais rápido.
        /// Memes incompatíveis competem: a soma de prevalência por tipo é limitada a 1.5.
        /// </summary>
        public void Propagate(List<Creature> cs, Social social, Rng rng)
        {
            if (Pool.Count == 0) return;

            // Calcula bônus de propagação das Figuras para seus memes de origem
            var figureMemeBonus = new Dictionary<int, float>();  // originId → bônus
            for (int i = 0; i < cs.Count; i++)
            {
                var c = cs[i];
                if (c.Alive && c.IsFigure) figureMemeBonus[c.Id] = c.Prestige * 0.008f;
            }

            var toRemove = new List<int>();
            // Ordenação explícita: determinístico após restore (Dictionary não preserva ordem interna)
            var ids = new List<int>(Pool.Keys); ids.Sort();
            foreach (int mid in ids)
            {
                var m = Pool[mid];
                float rate = m.Force * (1f - m.Rigidity * 0.5f) * 0.004f;
                if (figureMemeBonus.TryGetValue(m.OriginId, out float bonus)) rate += bonus;
                m.Prevalence = Clamp01(m.Prevalence + rate);
                // Decaimento: memes sem suporte vivo erodem devagar
                m.Prevalence *= 0.994f + m.Rigidity * 0.005f;
                if (m.Prevalence < 0.008f) { toRemove.Add(mid); continue; }
                Pool[mid] = m;
            }
            foreach (int mid in toRemove) Pool.Remove(mid);

            // Competição por tipo: soma de prevalências ≤ 1.5 (atenção coletiva é finita)
            var sumByType = new Dictionary<MemeType, float>();
            foreach (var kv in Pool)
            {
                sumByType.TryGetValue(kv.Value.Type, out float s);
                sumByType[kv.Value.Type] = s + kv.Value.Prevalence;
            }
            foreach (var kv in sumByType)
            {
                if (kv.Value <= 1.5f) continue;
                float scale = 1.5f / kv.Value;
                var typeKeys = new List<int>();
                foreach (var m2 in Pool) if (m2.Value.Type == kv.Key) typeKeys.Add(m2.Key);
                foreach (int mid in typeKeys) { var m = Pool[mid]; m.Prevalence *= scale; Pool[mid] = m; }
            }

            // Coesão: quão concentrada está a prevalência (1 = dominado por poucos memes fortes)
            float maxP = 0f, sumP = 0f;
            foreach (var kv in Pool) { sumP += kv.Value.Prevalence; if (kv.Value.Prevalence > maxP) maxP = kv.Value.Prevalence; }
            CulturalCohesion = Pool.Count == 0 ? 1f : Clamp01(maxP / Math.Max(0.01f, sumP / Pool.Count));
        }

        /// <summary>
        /// Lente interpretativa (M09 §4.2): retorna o meme dominante para um tipo de evento.
        /// Seleção por argmax de força × prevalência — nunca sorteio.
        /// </summary>
        public Meme? Interpret(MemeType eventType)
        {
            Meme? best = null; float bestScore = -1f;
            foreach (var kv in Pool)
            {
                if (kv.Value.Type != eventType) continue;
                float score = kv.Value.Force * kv.Value.Prevalence;
                if (score > bestScore) { bestScore = score; best = kv.Value; }
            }
            return best;
        }

        /// <summary>Meme mais dominante (independente de tipo).</summary>
        public Meme? Dominant()
        {
            Meme? best = null; float bestScore = -1f;
            foreach (var kv in Pool)
            {
                float score = kv.Value.Force * kv.Value.Prevalence;
                if (score > bestScore) { bestScore = score; best = kv.Value; }
            }
            return best;
        }

        static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);

        // ----- snapshot -----
        public void Write(BinaryWriter w)
        {
            w.Write(_nextId); w.Write(CulturalCohesion);
            w.Write(Pool.Count);
            // Ordem determinística por id
            var keys = new List<int>(Pool.Keys); keys.Sort();
            foreach (int k in keys)
            {
                var m = Pool[k];
                w.Write(m.Id); w.Write((byte)m.Type);
                w.Write(m.Force); w.Write(m.Rigidity);
                w.Write(m.Prevalence); w.Write(m.OriginId);
            }
        }
        public void ReadInto(BinaryReader r)
        {
            _nextId = r.ReadInt32(); CulturalCohesion = r.ReadSingle();
            int n = r.ReadInt32(); Pool.Clear();
            for (int i = 0; i < n; i++)
            {
                var m = new Meme
                {
                    Id = r.ReadInt32(), Type = (MemeType)r.ReadByte(),
                    Force = r.ReadSingle(), Rigidity = r.ReadSingle(),
                    Prevalence = r.ReadSingle(), OriginId = r.ReadInt32()
                };
                Pool[m.Id] = m;
            }
        }
    }
}
