using System.IO;

namespace Genese.Core
{
    /// <summary>
    /// Estado da relação entre dois pares de civilizações (M11 §4.3).
    /// Evolui pelo histórico de interações — nunca por evento aleatório.
    /// </summary>
    public enum CivStance : byte
    {
        Desconhecida,    // sem contato ainda
        PrimeiroContato, // encontro inicial
        Cautelosa,       // poucos intercâmbios
        Comercial,       // comércio ativo, confiança crescendo
        Aliada,          // alta confiança
        Guerra,          // resentimento acima do limiar
        Vassalagem       // uma civ domina a outra
    }

    public struct CivRelation
    {
        /// <summary>Confiança acumulada [0,1]: cresce com comércio/ajuda, cai com traições.</summary>
        public float Trust;
        /// <summary>Resentimento acumulado [0,1]: cresce com agressões, decai lentamente com paz.</summary>
        public float Resentment;
        public CivStance Stance;
        public int TradeCount;
        public int WarCount;
        public int LastContactTick;

        public void Write(BinaryWriter w)
        {
            w.Write(Trust); w.Write(Resentment); w.Write((byte)Stance);
            w.Write(TradeCount); w.Write(WarCount); w.Write(LastContactTick);
        }
        public static CivRelation Read(BinaryReader r) => new CivRelation
        {
            Trust         = r.ReadSingle(),
            Resentment    = r.ReadSingle(),
            Stance        = (CivStance)r.ReadByte(),
            TradeCount    = r.ReadInt32(),
            WarCount      = r.ReadInt32(),
            LastContactTick = r.ReadInt32()
        };
    }
}
