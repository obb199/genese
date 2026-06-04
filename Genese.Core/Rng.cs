namespace Genese.Core
{
    /// <summary>
    /// RNG determinístico central (xoshiro256** semeado por SplitMix64).
    /// Princípio de causalidade (GDD §1.5): toda aleatoriedade do jogo passa por
    /// aqui, semeada pelo estado do mundo — nunca por DateTime/Environment.
    ///
    /// Fork(streamId) cria um sub-stream independente derivado da SEMENTE original
    /// (não do estado corrente), de modo que adicionar um novo consumo de RNG em um
    /// subsistema não altera a sequência de outro (requisito de E01).
    /// </summary>
    public sealed class Rng
    {
        private readonly ulong _seed;
        private ulong _s0, _s1, _s2, _s3;

        public Rng(ulong seed)
        {
            _seed = seed;
            Reseed(seed);
        }

        private void Reseed(ulong seed)
        {
            ulong sm = seed;
            _s0 = SplitMix(ref sm); _s1 = SplitMix(ref sm);
            _s2 = SplitMix(ref sm); _s3 = SplitMix(ref sm);
        }

        private static ulong SplitMix(ref ulong x)
        {
            x += 0x9E3779B97F4A7C15UL;
            ulong z = x;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        private static ulong Rotl(ulong x, int k) => (x << k) | (x >> (64 - k));

        /// <summary>Próximo inteiro 64-bit pseudoaleatório (xoshiro256**).</summary>
        public ulong NextULong()
        {
            ulong result = Rotl(_s1 * 5UL, 7) * 9UL;
            ulong t = _s1 << 17;
            _s2 ^= _s0; _s3 ^= _s1; _s1 ^= _s2; _s0 ^= _s3; _s2 ^= t; _s3 = Rotl(_s3, 45);
            return result;
        }

        /// <summary>Double em [0,1).</summary>
        public double NextDouble() => (NextULong() >> 11) * (1.0 / 9007199254740992.0);

        /// <summary>Inteiro em [minInclusive, maxExclusive).</summary>
        public int Range(int minInclusive, int maxExclusive)
            => maxExclusive <= minInclusive ? minInclusive
             : minInclusive + (int)(NextDouble() * (maxExclusive - minInclusive));

        /// <summary>Double em [min, max).</summary>
        public double Range(double min, double max) => min + NextDouble() * (max - min);

        /// <summary>true com probabilidade p (derivada do estado, GDD §1.5).</summary>
        public bool Chance(double p) => NextDouble() < p;

        /// <summary>Sub-stream determinístico e independente para um subsistema.</summary>
        public Rng Fork(ulong streamId)
        {
            ulong mixed = _seed ^ (streamId * 0x9E3779B97F4A7C15UL);
            return new Rng(SplitMix(ref mixed));
        }

        // --- estado para snapshot/restore (E01) ---
        public void WriteState(System.IO.BinaryWriter w)
        {
            w.Write(_seed); w.Write(_s0); w.Write(_s1); w.Write(_s2); w.Write(_s3);
        }

        public static Rng ReadState(System.IO.BinaryReader r)
        {
            ulong seed = r.ReadUInt64();
            var rng = new Rng(seed);
            rng._s0 = r.ReadUInt64(); rng._s1 = r.ReadUInt64(); rng._s2 = r.ReadUInt64(); rng._s3 = r.ReadUInt64();
            return rng;
        }
    }

    /// <summary>IDs de sub-stream — um por subsistema (evita acoplamento de consumo).</summary>
    public static class Streams
    {
        public const ulong Environment = 1;
        public const ulong Mutation = 2;
        public const ulong Decision = 3;
        public const ulong Reproduction = 4;
        public const ulong Culture = 5;
        public const ulong Events = 6;
        public const ulong Spawn = 7;
    }
}
