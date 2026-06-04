using System.IO;

namespace Genese.Core
{
    /// <summary>
    /// Núcleo determinístico. Step() avança 1 tick: ambiente (E03) e agentes (E04) são
    /// atualizados de forma causal. Cada subsistema tem um sub-stream de RNG PERSISTENTE
    /// (criado uma vez, evolui ao longo do tempo, é serializado). Snapshot/Restore
    /// reproduzem o estado bit a bit (saves, replays, GIF de evolução).
    /// </summary>
    public sealed class Simulation : ISimulation
    {
        public const uint SnapshotVersion = 5; // E06: _nextLinId + Spec + LOD

        private readonly WorldState _state;
        private Rng _root, _decision, _mut;

        public Environment Env { get; private set; }
        public Population Pop { get; private set; }
        public ulong Tick => _state.Tick;
        public WorldState State => _state;
        public Rng Root => _root;

        /// <summary>Nível de detalhe de simulação (E06/M03 §2.3). Pleno por padrão.</summary>
        public SimLOD LOD { get; private set; } = SimLOD.Pleno;
        public void SetLOD(SimLOD lod) { LOD = lod; }

        public Simulation(ulong seed, int width = 64, int height = 48, int initialCreatures = 40, float tempBias = 0f, float umidBias = 0f)
        {
            _state = new WorldState { Seed = seed, Tick = 0 };
            _root = new Rng(seed);
            _decision = _root.Fork(Streams.Decision);
            _mut = _root.Fork(Streams.Mutation);

            Env = new Environment(width, height);
            Env.Generate(_root.Fork(Streams.Environment), tempBias, umidBias);

            Pop = new Population();
            Pop.Seed(Env, _root.Fork(Streams.Spawn), initialCreatures);
        }

        public void Step()
        {
            // LOD (E06 §2.3): Dormente pula ticks; Agregado pula Social/Speciation
            if (LOD == SimLOD.Dormente && _state.Tick % 10 != 0) { _state.Tick++; return; }

            Env.Step(_state.Tick);
            Pop.Step(Env, _state.Tick, _decision, _mut);

            // Agregado: Social e detecção de espécies correm mais espaçados
            if (LOD == SimLOD.Agregado && _state.Tick % 5 != 0) { _state.Tick++; return; }

            _state.Tick++;
        }

        public byte[] Snapshot()
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            w.Write(SnapshotVersion);
            w.Write(_state.Seed);
            w.Write(_state.Tick);
            Env.Write(w);
            _decision.WriteState(w);
            _mut.WriteState(w);
            Pop.Write(w);
            return ms.ToArray();
        }

        public void Restore(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var r = new BinaryReader(ms);
            uint version = r.ReadUInt32();
            if (version != SnapshotVersion)
                throw new InvalidDataException($"Snapshot versão {version} incompatível com {SnapshotVersion}.");
            _state.Seed = r.ReadUInt64();
            _state.Tick = r.ReadUInt64();
            _root = new Rng(_state.Seed);
            Env = new Environment(1, 1); Env.ReadInto(r);
            _decision = Rng.ReadState(r);
            _mut = Rng.ReadState(r);
            Pop = new Population(); Pop.ReadInto(r);
        }
    }
}
