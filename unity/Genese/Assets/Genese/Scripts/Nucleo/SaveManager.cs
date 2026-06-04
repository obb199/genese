using System.IO;
using UnityEngine;

namespace Genese.Nucleo
{
    /// <summary>
    /// Sistema de save/load (E10). Usa Simulation.Snapshot() / Restore() — o mesmo
    /// mecanismo determinístico dos testes — para persistir o estado completo em disco.
    /// 3 slots de save; auto-save a cada N ticks (opcional).
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public CoreSim sim;
        [Tooltip("Número de ticks entre auto-saves (0 = desactivado)")]
        public int autoSaveInterval = 0;

        const int NumSlots = 3;

        static string SlotPath(int slot)
            => Path.Combine(Application.persistentDataPath, $"genese_save_{slot}.bin");

        static string MetaKey(int slot) => $"genese_save_{slot}_meta";

        void Update()
        {
            if (autoSaveInterval > 0 && sim?.Sim != null
                && sim.Sim.Tick > 0 && sim.Sim.Tick % (ulong)autoSaveInterval == 0)
                Save(0); // slot 0 = auto-save
        }

        /// <summary>Guarda o estado completo no slot indicado (0-2).</summary>
        public bool Save(int slot)
        {
            if (sim?.Sim == null || slot < 0 || slot >= NumSlots) return false;
            try
            {
                byte[] data = sim.Sim.Snapshot();
                File.WriteAllBytes(SlotPath(slot), data);
                // Metadados legíveis
                PlayerPrefs.SetString(MetaKey(slot),
                    $"tick:{sim.Sim.Tick}|pop:{sim.Sim.Pop?.Count ?? 0}|dest:{sim.Sim.Destiny}|civs:{sim.Sim.Civs.Count}");
                PlayerPrefs.Save();
                Debug.Log($"[Gênese] Save slot {slot} — {data.Length/1024f:0.0} KB (tick {sim.Sim.Tick})");
                return true;
            }
            catch (System.Exception e) { Debug.LogError($"[Gênese] Save falhou: {e.Message}"); return false; }
        }

        /// <summary>Carrega o save do slot e reinicia a visualização.</summary>
        public bool Load(int slot)
        {
            string path = SlotPath(slot);
            if (!File.Exists(path)) { Debug.LogWarning($"[Gênese] Slot {slot} vazio"); return false; }
            try
            {
                byte[] data = File.ReadAllBytes(path);
                sim.Sim.Restore(data);
                sim.BumpVersion(); // força rebuild das views
                Debug.Log($"[Gênese] Load slot {slot} — tick {sim.Sim.Tick}");
                return true;
            }
            catch (System.Exception e) { Debug.LogError($"[Gênese] Load falhou: {e.Message}"); return false; }
        }

        public bool HasSave(int slot) => slot >= 0 && slot < NumSlots && File.Exists(SlotPath(slot));

        /// <summary>Descrição legível do save (para UI).</summary>
        public string SaveInfo(int slot)
        {
            if (!HasSave(slot)) return "(vazio)";
            return PlayerPrefs.GetString(MetaKey(slot), "save").Replace("|", "  ");
        }

        public void DeleteSave(int slot)
        {
            if (HasSave(slot)) File.Delete(SlotPath(slot));
            PlayerPrefs.DeleteKey(MetaKey(slot));
        }
    }
}
