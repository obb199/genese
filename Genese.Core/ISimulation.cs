namespace Genese.Core
{
    /// <summary>
    /// Contrato do núcleo de simulação — independente da engine (E00 §3.1).
    /// A Unity (Genese.Game) consome esta interface; nunca o contrário.
    /// </summary>
    public interface ISimulation
    {
        ulong Tick { get; }
        WorldState State { get; }
        void Step();            // avança 1 tick de forma determinística
        byte[] Snapshot();      // serializa o estado (saves, replays, GIF de evolução)
        void Restore(byte[] s); // restaura estado exato (bit a bit)
    }
}
