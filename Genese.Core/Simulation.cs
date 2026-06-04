using System;
using System.Collections.Generic;
using System.IO;

namespace Genese.Core
{
    /// <summary>
    /// Núcleo determinístico (E08). Gerencia MÚLTIPLAS civilizações rodando as mesmas
    /// regras (M01-M10) + ContactSystem inter-civ (M11) + EventSystem causal (M14).
    ///
    /// Cada civ tem seu próprio par de RNGs (decision/mutation), derivados da semente
    /// com offsets de stream distintos — independência total entre civs.
    /// Pop (compat. com código anterior) é atalho para Civs[0].Pop.
    /// </summary>
    public sealed class Simulation : ISimulation
    {
        public const uint SnapshotVersion = 8; // E09: InfluenceSystem + Chronicle + PopStats

        private WorldState _state;
        private Rng _root;
        private Rng _contact;   // ContactSystem entre civs
        private Rng _evRng;     // EventSystem

        // RNGs por civ (paralelos à lista Civs)
        private readonly List<Rng> _civDec = new();
        private readonly List<Rng> _civMut = new();

        public Environment   Env    { get; private set; }
        public List<Civilization> Civs      { get; private set; } = new();
        public EventSystem        Events    { get; private set; } = new();
        public InfluenceSystem    Influence { get; private set; } = new();
        public Chronicle          Chronicle { get; private set; } = new();

        /// <summary>Backward-compat: civilização do jogador (Civs[0].Pop).</summary>
        public Population Pop => Civs.Count > 0 ? Civs[0].Pop : null;

        /// <summary>Destino atual da simulação (M14 §5), avaliado em cada Step.</summary>
        public DestinyType Destiny { get; private set; } = DestinyType.Continuidade;

        public ulong     Tick  => _state.Tick;
        public WorldState State => _state;
        public Rng       Root  => _root;

        public SimLOD LOD { get; private set; } = SimLOD.Pleno;
        public void SetLOD(SimLOD lod) { LOD = lod; }

        /// <param name="numCivs">Número de civilizações (padrão 2: cada uma em metade do mapa).</param>
        public Simulation(ulong seed, int width = 64, int height = 48, int initialCreatures = 40,
                          float tempBias = 0f, float umidBias = 0f,
                          float tempBias2 = float.NaN, float umidBias2 = float.NaN,
                          int numCivs = 2)
        {
            _state = new WorldState { Seed = seed, Tick = 0 };
            _root  = new Rng(seed);

            Env = new Environment(width, height);
            Env.Generate(_root.Fork(Streams.Environment), tempBias, umidBias, tempBias2, umidBias2);

            _contact = _root.Fork(Streams.Contact);
            _evRng   = _root.Fork(Streams.Events);
            Influence.Init(width, height);

            // Se initialCreatures == 0, respeita o pedido (testes de ambiente sem população)
            int perCiv = initialCreatures == 0 ? 0
                       : Math.Max(4, initialCreatures / Math.Max(1, numCivs));
            for (int c = 0; c < numCivs; c++)
            {
                // Streams da civ c: offsets de 0x100*c para isolamento total
                ulong off = (ulong)c * 0x100UL;
                var dec = _root.Fork(Streams.Decision + off);
                var mut = _root.Fork(Streams.Mutation + off);
                var sym = _root.Fork(Streams.Symbol   + off);
                var spw = _root.Fork(Streams.Spawn    + off);

                // Faixa de spawn horizontal dividida por número de civs
                int xMin = c * (width / numCivs);
                int xMax = (c == numCivs - 1) ? width : (c + 1) * (width / numCivs);

                var pop = new Population { Cap = Math.Max(30, initialCreatures) };
                pop.Seed(Env, spw, perCiv, sym, xMin, xMax);

                _civDec.Add(dec);
                _civMut.Add(mut);

                int sx = (xMin + xMax) / 2, sy = height / 2;
                Civs.Add(new Civilization(c, pop, sx, sy));
            }

            Events = new EventSystem();
        }

        public void Step()
        {
            if (LOD == SimLOD.Dormente && _state.Tick % 10 != 0) { _state.Tick++; return; }

            Env.Step(_state.Tick);

            // Cada civ avança com seus próprios RNGs (determinístico, isolado)
            for (int i = 0; i < Civs.Count; i++)
                Civs[i].Pop.Step(Env, _state.Tick, _civDec[i], _civMut[i]);

            if (LOD != SimLOD.Agregado || _state.Tick % 5 == 0)
            {
                // Contato inter-civ (a cada 4 ticks — mais frequente)
                if (_state.Tick % 4 == 0 && Civs.Count > 1)
                    for (int i = 0; i < Civs.Count; i++)
                        for (int j = i+1; j < Civs.Count; j++)
                            ContactSystem.CheckAndInteract(Civs[i], Civs[j], Env, _state.Tick, _contact);

                // Eventos causais + Crônica + Destino (a cada 20 ticks)
                if (_state.Tick % 20 == 0)
                {
                    Events.Step(Civs, Env, _state.Tick, _evRng);
                    Chronicle.Sync(Events.Log, Civs);
                    Destiny = EventSystem.EvaluateDestiny(Civs, Env);
                }
            }

            // Influência: regeneração de Atenção por tick (fervor da civ do jogador)
            Influence.Step(Pop != null ? Pop.Belief.Fervor : 0f);

            _state.Tick++;
        }

        public byte[] Snapshot()
        {
            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms);
            w.Write(SnapshotVersion);
            w.Write(_state.Seed); w.Write(_state.Tick);
            w.Write((byte)LOD);
            Env.Write(w);
            _contact.WriteState(w); _evRng.WriteState(w);
            w.Write(Civs.Count);
            for (int i = 0; i < Civs.Count; i++)
            {
                _civDec[i].WriteState(w);
                _civMut[i].WriteState(w);
                Civs[i].Write(w);
            }
            Events.Write(w);
            Influence.Write(w);
            Chronicle.Write(w);
            w.Write((byte)Destiny);
            return ms.ToArray();
        }

        public void Restore(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var r  = new BinaryReader(ms);
            uint version = r.ReadUInt32();
            if (version != SnapshotVersion)
                throw new InvalidDataException($"Snapshot versão {version} incompatível com {SnapshotVersion}.");
            _state = new WorldState { Seed = r.ReadUInt64(), Tick = r.ReadUInt64() };
            LOD    = (SimLOD)r.ReadByte();
            _root  = new Rng(_state.Seed);
            Env    = new Environment(1,1); Env.ReadInto(r);
            _contact = Rng.ReadState(r); _evRng = Rng.ReadState(r);
            int n = r.ReadInt32();
            Civs.Clear(); _civDec.Clear(); _civMut.Clear();
            for (int i = 0; i < n; i++)
            {
                _civDec.Add(Rng.ReadState(r));
                _civMut.Add(Rng.ReadState(r));
                Civs.Add(Civilization.Read(r));
            }
            Events = new EventSystem(); Events.ReadInto(r);
            Influence = new InfluenceSystem(); Influence.ReadInto(r);
            Chronicle = new Chronicle(); Chronicle.ReadInto(r);
            Destiny = (DestinyType)r.ReadByte();
        }
    }
}
