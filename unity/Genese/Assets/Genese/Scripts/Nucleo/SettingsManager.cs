using UnityEngine;

namespace Genese.Nucleo
{
    /// <summary>
    /// Configurações de jogo persistentes (E10). Mantém velocidade, qualidade,
    /// volume e outros parâmetros sincronizados com PlayerPrefs.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public CoreSim sim;

        // Perfis de qualidade (quanto mais alto, mais objetos por frame)
        public static readonly (string name, int maxRender, float stepsPerSec)[] QualityProfiles =
        {
            ("Performance", 60,   15f),
            ("Balanceado",  120,  20f),
            ("Qualidade",   200,  30f),
        };

        int _qualityIdx = 1;

        void Start() => Load();

        public void SetQuality(int idx)
        {
            _qualityIdx = Mathf.Clamp(idx, 0, QualityProfiles.Length - 1);
            Apply();
            Save();
        }

        public int QualityIndex => _qualityIdx;
        public string QualityName => QualityProfiles[_qualityIdx].name;

        void Apply()
        {
            if (sim == null) return;
            var p = QualityProfiles[_qualityIdx];
            // Aplica velocidade default do perfil (o jogador pode ajustar no HUD)
            sim.stepsPerSecond = p.stepsPerSec;
        }

        void Load()
        {
            _qualityIdx = PlayerPrefs.GetInt("genese_quality", 1);
            Apply();
        }

        void Save()
        {
            PlayerPrefs.SetInt("genese_quality", _qualityIdx);
            PlayerPrefs.Save();
        }
    }
}
