using UnityEngine;

namespace Genese.Nucleo
{
    /// <summary>
    /// Ciclo completo de dia e noite com 8 keyframes definidos em fase [0,1) e
    /// interpolação suave (SmoothStep) entre eles. Elimina descontinuidades: os
    /// valores em toda fronteira de segmento são exatamente idênticos no início
    /// do próximo segmento — resultado de usar UM único valor por keyframe.
    /// </summary>
    public class CoreDayNight : MonoBehaviour
    {
        public Light sun;
        public Camera cam;

        [Header("Velocidade do ciclo")]
        [Tooltip("Duração de um dia completo em segundos (padrão = 320s ≈ 5min)")]
        public float cycleDuration = 320f;
        public bool autoCycle = true;

        // Fase normalizada [0..1) — 0 = meia-noite, 0.25 = amanhecer, 0.5 = meio-dia, 0.75 = anoitecer
        float _phase      = 0.28f;
        float _targetPhase = 0.28f;

        // ─── Nomes das fases ─────────────────────────────────────────────────────
        const float kMidnight = 0.00f;
        const float kPreDawn  = 0.16f;
        const float kSunrise  = 0.28f;
        const float kNoon     = 0.45f;
        const float kAfternoon= 0.62f;
        const float kSunset   = 0.76f;
        const float kEvening  = 0.88f;

        public bool IsNight => _phase < kPreDawn || _phase >= kEvening;
        public string PhaseName => _phase switch
        {
            var p when p < kPreDawn   => "Noite",
            var p when p < kSunrise   => "Amanhecer",
            var p when p < kNoon      => "Manhã",
            var p when p < kAfternoon => "Meio-dia",
            var p when p < kSunset    => "Tarde",
            var p when p < kEvening   => "Anoitecer",
            _                         => "Noite"
        };

        // ─── Keyframes ──────────────────────────────────────────────────────────
        struct DayKey
        {
            public Color skyTop, skyHorizon, ambientGround, sunColor;
            public float sunIntensity;
            public DayKey(string top, string hor, string amb, string sun, float inten)
            {
                skyTop = H(top); skyHorizon = H(hor); ambientGround = H(amb);
                sunColor = H(sun); sunIntensity = inten;
            }
            static Color H(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
        }

        // Um valor por instante — zero descontinuidades por construção.
        // Fase 1.00 repete a fase 0.00 para fechar o loop sem salto.
        static readonly (float phase, DayKey key)[] _keys =
        {
            (0.00f, new DayKey("#060611", "#0A0A1A", "#030308", "#99A4E5", 0.06f)), // meia-noite
            (0.16f, new DayKey("#140D2E", "#722018", "#140808", "#FF6120", 0.15f)), // pré-amanhecer
            (0.28f, new DayKey("#855890", "#EBA030", "#381E0A", "#FFAD58", 0.65f)), // nascer do sol
            (0.45f, new DayKey("#5C95D2", "#ACD0E8", "#383830", "#FFF5DC", 1.22f)), // meio-dia
            (0.62f, new DayKey("#60A0C8", "#B8D0DC", "#383830", "#FFF5E0", 1.25f)), // tarde plena
            (0.76f, new DayKey("#9E7460", "#EB9B40", "#332216", "#FFB860", 0.72f)), // pôr do sol
            (0.88f, new DayKey("#0F0822", "#241836", "#060305", "#99A0CC", 0.08f)), // anoitecer
            (1.00f, new DayKey("#060611", "#0A0A1A", "#030308", "#99A4E5", 0.06f)), // meia-noite (loop)
        };

        /// <summary>Avança para a próxima fase do ciclo.</summary>
        public void Toggle()
        {
            float next = _phase switch
            {
                var p when p < kPreDawn   => kPreDawn,
                var p when p < kSunrise   => kSunrise,
                var p when p < kNoon      => kNoon,
                var p when p < kAfternoon => kAfternoon,
                var p when p < kSunset    => kSunset,
                var p when p < kEvening   => kEvening,
                _                         => kPreDawn
            };
            _targetPhase = next;
            autoCycle = false;
        }
        public void ResumeCycle() { autoCycle = true; }

        void Update()
        {
            if (autoCycle)
            {
                _phase = (_phase + Time.deltaTime / cycleDuration) % 1f;
                _targetPhase = _phase;
            }
            else
            {
                // Desliza suavemente até a fase alvo (sem teleporte)
                float diff = Mathf.DeltaAngle(_phase * 360f, _targetPhase * 360f) / 360f;
                _phase += Mathf.Sign(diff) * Mathf.Min(Mathf.Abs(diff), Time.deltaTime * 0.55f);
                _phase = (_phase % 1f + 1f) % 1f;
                if (Mathf.Abs(diff) < 0.003f) autoCycle = true;
            }
            Apply(_phase);
        }

        void Apply(float p)
        {
            // Encontra os dois keyframes adjacentes
            int a = 0, b = 1;
            for (int i = 0; i < _keys.Length - 1; i++)
            {
                if (p >= _keys[i].phase && p < _keys[i + 1].phase) { a = i; b = i + 1; break; }
            }
            float range = _keys[b].phase - _keys[a].phase;
            float t = range > 0f ? (p - _keys[a].phase) / range : 0f;
            // SmoothStep para transições ainda mais suaves no meio do segmento
            t = t * t * (3f - 2f * t);

            var ka = _keys[a].key;
            var kb = _keys[b].key;
            Color skyTop        = Color.Lerp(ka.skyTop,        kb.skyTop,        t);
            Color skyHorizon    = Color.Lerp(ka.skyHorizon,    kb.skyHorizon,    t);
            Color ambientGround = Color.Lerp(ka.ambientGround, kb.ambientGround, t);
            Color sunColor      = Color.Lerp(ka.sunColor,      kb.sunColor,      t);
            float sunIntensity  = Mathf.Lerp(ka.sunIntensity,  kb.sunIntensity,  t);

            // Posição solar: elevação segue seno (nasce a leste, passa pelo zenith, poente a oeste)
            float sunElevation = Mathf.Sin((p - 0.25f) * Mathf.PI * 2f) * 80f;
            float sunAzimuth   = 40f + (p - 0.25f) * 180f;

            if (sun != null)
            {
                sun.intensity = sunIntensity;
                sun.color     = sunColor;
                sun.transform.rotation = Quaternion.Euler(sunElevation, sunAzimuth, 0f);
            }

            if (cam != null)
                cam.backgroundColor = Color.Lerp(skyHorizon, skyTop, 0.45f);

            RenderSettings.ambientMode         = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor     = skyTop;
            RenderSettings.ambientEquatorColor = skyHorizon;
            RenderSettings.ambientGroundColor  = ambientGround;
        }
    }
}
