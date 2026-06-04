using System;
using System.Collections.Generic;
using System.IO;

namespace Genese.Core
{
    public enum EventType : byte
    {
        Seca,            // balanço hídrico crítico em habitat
        Fome,            // energia média crítica
        ColapsoPop,      // população abaixo do limiar mínimo
        GuerraDeclarada, // resentimento acima do limiar de guerra
        Transcendencia,  // organização religiosa máxima
        Expansao,        // população próxima do cap
        Fusao            // duas civs fundem-se (trust+compat alto)
    }

    public struct GameEvent
    {
        public int       CivId;
        public EventType Type;
        public ulong     Tick;       // tick de ativação
        public bool      Resolved;
        public string    Resolution; // texto rastreável para a Crônica (M13)
    }

    /// <summary>
    /// Sistema de eventos causais (M14). Situações latentes ativam por LIMIAR de estado —
    /// nunca por probabilidade plana por turno. A resolução é o argmax de compatibilidade
    /// entre estado/cultura e as pré-condições de cada caminho de saída.
    /// </summary>
    public sealed class EventSystem
    {
        public readonly List<GameEvent> Log    = new(); // Crônica rastreável
        public readonly List<GameEvent> Active = new(); // eventos em andamento

        public int TotalEvents => Log.Count;

        public void Step(List<Civilization> civs, Environment env, ulong tick, Rng rng)
        {
            // Verifica limiares de cada civ
            foreach (var civ in civs)
            {
                CheckDrought    (civ, env, tick);
                CheckFamine     (civ, tick);
                CheckCollapse   (civ, tick);
                CheckTranscend  (civ, tick);
                CheckExpansion  (civ, tick);
            }
            // Verifica eventos inter-civ
            for (int i = 0; i < civs.Count; i++)
                for (int j = i+1; j < civs.Count; j++)
                    CheckWar(civs[i], civs[j], tick);

            // Resolve eventos activos (resolve-e-loga ou mantém ativo)
            for (int i = Active.Count - 1; i >= 0; i--)
            {
                var ev  = Active[i];
                var civ = civs.Find(c => c.Id == ev.CivId);
                if (civ == null) { Active.RemoveAt(i); continue; }
                Resolve(ref ev, civ, civs, env, rng);
                if (ev.Resolved) { Log.Add(ev); Active.RemoveAt(i); }
                else Active[i] = ev;
            }
        }

        void Activate(GameEvent ev)
        {
            // Nunca dois eventos do mesmo tipo para a mesma civ simultaneamente
            foreach (var a in Active) if (a.CivId == ev.CivId && a.Type == ev.Type) return;
            Active.Add(ev);
        }

        // ---- Limiares de ativação ---- por estado do mundo, nunca por tick/chance ----

        void CheckDrought(Civilization civ, Environment env, ulong tick)
        {
            int drought = 0, n = 0;
            foreach (var c in civ.Pop.Creatures)
            {
                if (!c.Alive) continue; n++;
                int x = Math.Clamp((int)c.X,0,env.W-1), y = Math.Clamp((int)c.Y,0,env.H-1);
                if (env.BalancoAgua[env.Idx(x,y)] < Environment.DroughtThreshold) drought++;
            }
            if (n > 0 && (float)drought / n > 0.45f)
                Activate(new GameEvent { CivId=civ.Id, Type=EventType.Seca, Tick=tick });
        }

        void CheckFamine(Civilization civ, ulong tick)
        {
            float avgE = 0f; int n = 0;
            foreach (var c in civ.Pop.Creatures) if (c.Alive) { avgE += c.Energy; n++; }
            if (n > 0 && avgE / n < 0.18f)
                Activate(new GameEvent { CivId=civ.Id, Type=EventType.Fome, Tick=tick });
        }

        void CheckCollapse(Civilization civ, ulong tick)
        {
            if (civ.Pop.Count < 3)
                Activate(new GameEvent { CivId=civ.Id, Type=EventType.ColapsoPop, Tick=tick });
        }

        void CheckTranscend(Civilization civ, ulong tick)
        {
            if (civ.Pop.Belief.Stage == BeliefStage.Transcendente && civ.Pop.Belief.Organization >= 0.55f)
                Activate(new GameEvent { CivId=civ.Id, Type=EventType.Transcendencia, Tick=tick });
        }

        void CheckExpansion(Civilization civ, ulong tick)
        {
            if (civ.Pop.Count >= (int)(civ.Pop.Cap * 0.88f))
                Activate(new GameEvent { CivId=civ.Id, Type=EventType.Expansao, Tick=tick });
        }

        void CheckWar(Civilization a, Civilization b, ulong tick)
        {
            var rel = a.GetOrDefault(b.Id);
            if (rel.Stance == CivStance.Guerra)
            {
                Activate(new GameEvent { CivId=a.Id, Type=EventType.GuerraDeclarada, Tick=tick });
                Activate(new GameEvent { CivId=b.Id, Type=EventType.GuerraDeclarada, Tick=tick });
            }
        }

        // ---- Resoluções causais: argmax de compatibilidade estado/cultura ----

        void Resolve(ref GameEvent ev, Civilization civ, List<Civilization> civs, Environment env, Rng rng)
        {
            switch (ev.Type)
            {
                case EventType.Seca:            ResolveDrought    (ref ev, civ, civs, rng); break;
                case EventType.Fome:            ResolveFamine     (ref ev, civ); break;
                case EventType.ColapsoPop:      ResolveCollapse   (ref ev, civ); break;
                case EventType.Transcendencia:  ResolveTranscend  (ref ev, civ); break;
                case EventType.Expansao:        ResolveExpansion  (ref ev, civ); break;
                case EventType.GuerraDeclarada: ResolveWar        (ref ev, civ, civs); break;
                default: ev.Resolved = true; break;
            }
        }

        void ResolveDrought(ref GameEvent ev, Civilization civ, List<Civilization> civs, Rng rng)
        {
            // Três caminhos de resolução — argmax de compatibilidade com a cultura
            float xenofobia  = ContactSystem.AvgTrait(civ.Pop, "comp.territorialidade") * 0.6f
                             + ContactSystem.AvgTrait(civ.Pop, "comp.agressividade")    * 0.4f;
            float religioso  = civ.Pop.Belief.Fervor;
            float nomadismo  = ContactSystem.AvgTrait(civ.Pop, "comp.nomadismo");

            if (xenofobia > religioso && xenofobia > nomadismo
                && civ.Pop.Belief.Image == PlayerImage.Hostil)
            {
                ev.Resolution = $"Seca (civ {civ.Id}) → conflito: tribos atacam vizinhos por água";
                foreach (var other in civs)
                    if (other.Id != civ.Id)
                    {
                        var rel = civ.GetOrDefault(other.Id);
                        rel.Resentment = Math.Min(1f, rel.Resentment + 0.28f);
                        civ.Relations[other.Id] = rel;
                    }
            }
            else if (religioso >= nomadismo)
            {
                ev.Resolution = $"Seca (civ {civ.Id}) → ritual: cerimônias de chuva; fervor sobe";
                civ.Pop.Belief.RecordNudge(0);
            }
            else
            {
                ev.Resolution = $"Seca (civ {civ.Id}) → migração: grupos dispersam em busca de água";
                foreach (var c in civ.Pop.Creatures)
                    if (c.Alive) c.Energy = Math.Min(1f, c.Energy + 0.07f);
            }
            ev.Resolved = true;
        }

        void ResolveFamine(ref GameEvent ev, Civilization civ)
        {
            float coop = ContactSystem.AvgTrait(civ.Pop, "comp.cooperacao")
                       + ContactSystem.AvgTrait(civ.Pop, "comp.altruismo");
            if (coop > 0.85f)
            {
                ev.Resolution = $"Fome (civ {civ.Id}) → cooperação: Figuras redistribuem recursos";
                foreach (var fig in civ.Pop.Creatures)
                    if (fig.IsFigure && fig.Alive && fig.Energy > 0.5f)
                    {
                        float give = 0.08f; fig.Energy -= give;
                        foreach (var weak in civ.Pop.Creatures)
                            if (weak.Alive && weak.Energy < 0.25f)
                                weak.Energy = Math.Min(0.35f, weak.Energy + give * 0.25f);
                    }
            }
            else
            {
                ev.Resolution = $"Fome (civ {civ.Id}) → estresse: mortalidade elevada por escassez";
                int killed = 0;
                foreach (var c in civ.Pop.Creatures)
                    if (c.Alive && c.Energy < 0.12f && killed < 2) { c.Energy = 0f; killed++; }
            }
            ev.Resolved = true;
        }

        void ResolveCollapse(ref GameEvent ev, Civilization civ)
        {
            ev.Resolution = civ.Pop.Count == 0
                ? $"Extinção: civ {civ.Id} desapareceu do mundo"
                : $"Colapso (civ {civ.Id}): sobrevive com {civ.Pop.Count} indivíduo(s)";
            ev.Resolved = true;
        }

        void ResolveTranscend(ref GameEvent ev, Civilization civ)
        {
            ev.Resolution = $"Transcendência (civ {civ.Id}): organização espiritual máxima atingida";
            civ.Pop.Belief.RecordNudge(+1);
            ev.Resolved = true;
        }

        void ResolveExpansion(ref GameEvent ev, Civilization civ)
        {
            ev.Resolution = $"Expansão (civ {civ.Id}): população densa expande território";
            ev.Resolved = true;
        }

        void ResolveWar(ref GameEvent ev, Civilization civ, List<Civilization> civs)
        {
            // Paz emergente se resentimento caiu abaixo do limiar
            foreach (var other in civs)
            {
                if (other.Id == civ.Id) continue;
                var rel = civ.GetOrDefault(other.Id);
                if (rel.Stance != CivStance.Guerra) continue;
                if (rel.Resentment < 0.22f)
                {
                    rel.Stance = CivStance.Cautelosa;
                    civ.Relations[other.Id] = rel;
                    ev.Resolution = $"Paz emergente: civ {civ.Id} e {other.Id} cessam conflito (resentimento caiu)";
                    ev.Resolved   = true;
                    return;
                }
            }
            ev.Resolution = $"Guerra activa: civ {civ.Id} em conflito";
            ev.Resolved   = true;  // ativação repetida se mantiver estado de guerra
        }

        // ---- snapshot ----
        public void Write(BinaryWriter w)
        {
            WriteList(w, Log);
            WriteList(w, Active);
        }
        public void ReadInto(BinaryReader r)
        {
            Log.Clear();   ReadList(r, Log);
            Active.Clear(); ReadList(r, Active);
        }
        static void WriteList(BinaryWriter w, List<GameEvent> list)
        {
            w.Write(list.Count);
            foreach (var e in list)
            { w.Write(e.CivId); w.Write((byte)e.Type); w.Write(e.Tick); w.Write(e.Resolved); w.Write(e.Resolution ?? ""); }
        }
        static void ReadList(BinaryReader r, List<GameEvent> list)
        {
            int n = r.ReadInt32();
            for (int i = 0; i < n; i++)
                list.Add(new GameEvent { CivId=r.ReadInt32(), Type=(EventType)r.ReadByte(), Tick=r.ReadUInt64(), Resolved=r.ReadBoolean(), Resolution=r.ReadString() });
        }
    }
}
