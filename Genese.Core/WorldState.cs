namespace Genese.Core
{
    /// <summary>
    /// Metadados serializáveis do mundo (tempo + semente). O grosso do estado vive
    /// nos subsistemas (Environment em E03; agentes em E04), serializados no snapshot.
    /// </summary>
    public sealed class WorldState
    {
        public ulong Tick;
        public ulong Seed;
    }
}
