namespace Genese.Nucleo
{
    /// <summary>Versão semântica do jogo (E12).</summary>
    public static class GameVersion
    {
        public const int    Major   = 0;
        public const int    Minor   = 9;
        public const int    Patch   = 0;
        public const string Label   = "alpha";
        public const string Full    = "0.9.0-alpha";

        // Versão do formato de save (deve coincidir com Genese.Core.Simulation.SnapshotVersion)
        public const uint SaveVersion = 8;

        public static string Display => $"Gênese v{Full}  (save v{SaveVersion})";
    }
}
