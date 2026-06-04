using System.Collections.Generic;
using System.IO;

namespace Genese.Core
{
    public struct ChronicleEntry
    {
        public ulong  Tick;
        public int    CivId;
        public string Era;   // período narrativo
        public string Text;  // narração usando nomes do idioma emergente (M08)
    }

    /// <summary>
    /// Crônica narrativa (M13 §4.3). Narra apenas eventos REALMENTE registrados no
    /// EventSystem.Log — nenhuma palavra fabricada. Usa o léxico emergente (M08) da
    /// civilização para nomear o povo quando disponível. Oferece resumo por era para
    /// partidas longas (M13 §7: civilização grande → Crônica resumida, legível).
    /// </summary>
    public sealed class Chronicle
    {
        public readonly List<ChronicleEntry> Entries = new();
        private int _syncedLogCount;
        private const int EraLength = 500;

        public int Count => Entries.Count;

        /// <summary>
        /// Sincroniza com o log de eventos. Idempotente — não duplica entradas.
        /// Só narra o que de fato aconteceu (M13 causalidade bloqueadora).
        /// </summary>
        public void Sync(List<GameEvent> log, List<Civilization> civs)
        {
            for (int i = _syncedLogCount; i < log.Count; i++)
            {
                var ev  = log[i];
                var civ = civs.Find(c => c.Id == ev.CivId);
                Entries.Add(new ChronicleEntry
                {
                    Tick  = ev.Tick,
                    CivId = ev.CivId,
                    Era   = EraName(ev.Tick),
                    Text  = Narrate(ev, PeopleNameOf(civ))
                });
            }
            _syncedLogCount = log.Count;
        }

        static string EraName(ulong tick)
        {
            long era = (long)(tick / EraLength);
            return era switch
            {
                0 => "Era Primordial",
                1 => "Idade da Dispersão",
                2 => "Era dos Grupos",
                3 => "Idade das Figuras",
                4 => "Era das Línguas",
                5 => "Idade dos Ritos",
                6 => "Era dos Conflitos",
                7 => "Idade da Expansão",
                _ => $"Era {era}"
            };
        }

        /// <summary>
        /// Nome do povo a partir do léxico emergente (M08). Quando há escrita,
        /// os nomes próprios do idioma aparecem na Crônica.
        /// </summary>
        public static string PeopleNameOf(Civilization civ)
        {
            if (civ == null) return "Desconhecidos";
            var lex = civ.Pop.Language.Lexicon;
            if (lex.TryGetValue("grupo", out var g)) return $"Povo «{g}»";
            if (lex.TryGetValue("terra", out var t)) return $"Povo de «{t}»";
            if (lex.TryGetValue("lider", out var l)) return $"Povo de «{l}»";
            return $"Civilização {civ.Id}";
        }

        static string Narrate(GameEvent ev, string civName)
        {
            string res = ResolutionSuffix(ev.Resolution);
            return ev.Type switch
            {
                EventType.Seca            => $"[tick {ev.Tick}] {civName} foi assombrada pela seca.{res}",
                EventType.Fome            => $"[tick {ev.Tick}] A fome atingiu {civName}.{res}",
                EventType.ColapsoPop      => $"[tick {ev.Tick}] {civName} entrou em colapso demográfico.",
                EventType.GuerraDeclarada => $"[tick {ev.Tick}] {civName} entrou em estado de guerra.{res}",
                EventType.Transcendencia  => $"[tick {ev.Tick}] {civName} atingiu transcendência espiritual.",
                EventType.Expansao        => $"[tick {ev.Tick}] {civName} expandiu além das suas fronteiras.",
                EventType.Fusao           => $"[tick {ev.Tick}] {civName} fundiu-se com outro povo.",
                _ => $"[tick {ev.Tick}] {civName}: {ev.Type}"
            };
        }

        static string ResolutionSuffix(string res)
        {
            if (string.IsNullOrEmpty(res)) return "";
            int arrow = res.IndexOf('→');
            string suffix = arrow >= 0 ? res.Substring(arrow + 1).Trim() : res;
            return $" {suffix}";
        }

        /// <summary>Resumo por era — para partidas longas (M13 §7).</summary>
        public List<string> SummaryByEra()
        {
            var counts = new Dictionary<string, int>();
            foreach (var e in Entries)
            {
                counts.TryGetValue(e.Era, out int n);
                counts[e.Era] = n + 1;
            }
            var result = new List<string>();
            foreach (var kv in counts) result.Add($"{kv.Key}: {kv.Value} evento(s)");
            return result;
        }

        // ----- snapshot -----
        public void Write(BinaryWriter w)
        {
            w.Write(_syncedLogCount);
            w.Write(Entries.Count);
            foreach (var e in Entries)
            { w.Write(e.Tick); w.Write(e.CivId); w.Write(e.Era); w.Write(e.Text); }
        }
        public void ReadInto(BinaryReader r)
        {
            _syncedLogCount = r.ReadInt32();
            Entries.Clear();
            int n = r.ReadInt32();
            for (int i = 0; i < n; i++)
                Entries.Add(new ChronicleEntry
                {
                    Tick  = r.ReadUInt64(), CivId = r.ReadInt32(),
                    Era   = r.ReadString(),  Text  = r.ReadString()
                });
        }
    }
}
