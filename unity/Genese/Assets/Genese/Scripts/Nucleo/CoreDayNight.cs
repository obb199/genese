using UnityEngine;

namespace Genese.Nucleo
{
    /// <summary>
    /// Ciclo completo de dia e noite com 6 fases graduais — Amanhecer, Manhã, Meio-dia,
    /// Tarde, Anoitecer, Noite — cada uma com paleta de luz, cor do céu e ângulo solar únicos.
    /// Roda automaticamente; o HUD pode avançar para a próxima fase.
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
        float _phase = 0.28f; // começa perto do amanhecer
        float _targetPhase = 0.28f;

        // Fases do dia (limites em [0,1))
        const float kNight   = 0.00f;
        const float kDawn    = 0.16f;
        const float kMorning = 0.28f;
        const float kNoon    = 0.45f;
        const float kAfter   = 0.62f;
        const float kDusk    = 0.76f;
        const float kEvening = 0.88f;

        public bool IsNight => _phase < kDawn || _phase >= kEvening;
        public string PhaseName => _phase switch
        {
            var p when p < kDawn    => "Noite",
            var p when p < kMorning => "Amanhecer",
            var p when p < kNoon    => "Manhã",
            var p when p < kAfter   => "Meio-dia",
            var p when p < kDusk    => "Tarde",
            var p when p < kEvening => "Anoitecer",
            _                       => "Noite"
        };

        /// <summary>Avança para a próxima fase do ciclo.</summary>
        public void Toggle()
        {
            float next = _phase switch
            {
                var p when p < kDawn    => kDawn,
                var p when p < kMorning => kMorning,
                var p when p < kNoon    => kNoon,
                var p when p < kAfter   => kAfter,
                var p when p < kDusk    => kDusk,
                var p when p < kEvening => kEvening,
                _                       => kDawn
            };
            _targetPhase = next;
            autoCycle = false; // para ao chegar na fase escolhida
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
                // desliza suavemente até a fase alvo
                float diff = Mathf.DeltaAngle(_phase * 360f, _targetPhase * 360f) / 360f;
                _phase += Mathf.Sign(diff) * Mathf.Min(Mathf.Abs(diff), Time.deltaTime * 0.6f);
                _phase = (_phase % 1f + 1f) % 1f;
                if (Mathf.Abs(diff) < 0.005f) autoCycle = true;
            }
            Apply(_phase);
        }

        // ---- paleta de cada instante do ciclo ----
        void Apply(float p)
        {
            // Converte fase em elevação solar (−30° noite → 80° meio-dia)
            float sunElevation = Mathf.Sin((p - 0.25f) * Mathf.PI * 2f) * 80f; // +80° ao meio-dia
            float sunAzimuth   = 40f + (p - 0.25f) * 180f;                      // leste → oeste

            Color skyTop, skyHorizon, ambientGround, sunColor;
            float sunIntensity;

            if (p < kDawn)          // Noite  [0, 0.16)
            {
                float t = p / kDawn;
                skyTop        = Color.Lerp(new Color(0.02f, 0.02f, 0.07f), new Color(0.04f, 0.04f, 0.11f), t);
                skyHorizon    = Color.Lerp(new Color(0.04f, 0.04f, 0.1f),  new Color(0.06f, 0.05f, 0.14f), t);
                ambientGround = new Color(0.01f, 0.01f, 0.03f);
                sunColor      = new Color(0.6f, 0.65f, 0.9f);  // luz de lua
                sunIntensity  = Mathf.Lerp(0.06f, 0.1f, t);
            }
            else if (p < kMorning)  // Amanhecer  [0.16, 0.28)
            {
                float t = (p - kDawn) / (kMorning - kDawn);
                skyTop        = Color.Lerp(new Color(0.08f, 0.06f, 0.18f), new Color(0.52f, 0.38f, 0.62f), t);
                skyHorizon    = Color.Lerp(new Color(0.45f, 0.22f, 0.12f), new Color(0.92f, 0.60f, 0.28f), t);
                ambientGround = Color.Lerp(new Color(0.08f, 0.05f, 0.03f), new Color(0.22f, 0.14f, 0.08f), t);
                sunColor      = Color.Lerp(new Color(1f, 0.38f, 0.12f),    new Color(1f, 0.68f, 0.35f),    t);
                sunIntensity  = Mathf.Lerp(0.15f, 0.65f, t);
            }
            else if (p < kNoon)     // Manhã  [0.28, 0.45)
            {
                float t = (p - kMorning) / (kNoon - kMorning);
                skyTop        = Color.Lerp(new Color(0.52f, 0.68f, 0.88f), new Color(0.36f, 0.58f, 0.82f), t);
                skyHorizon    = Color.Lerp(new Color(0.88f, 0.72f, 0.52f), new Color(0.68f, 0.80f, 0.90f), t);
                ambientGround = Color.Lerp(new Color(0.28f, 0.22f, 0.15f), new Color(0.22f, 0.23f, 0.20f), t);
                sunColor      = Color.Lerp(new Color(1f, 0.84f, 0.58f),    new Color(1f, 0.96f, 0.86f),    t);
                sunIntensity  = Mathf.Lerp(0.65f, 1.20f, t);
            }
            else if (p < kAfter)    // Meio-dia  [0.45, 0.62)
            {
                float t = (p - kNoon) / (kAfter - kNoon);
                skyTop        = Color.Lerp(new Color(0.36f, 0.58f, 0.82f), new Color(0.38f, 0.60f, 0.78f), t);
                skyHorizon    = Color.Lerp(new Color(0.68f, 0.80f, 0.90f), new Color(0.72f, 0.82f, 0.86f), t);
                ambientGround = new Color(0.22f, 0.22f, 0.20f);
                sunColor      = new Color(1f, 0.96f, 0.88f);
                sunIntensity  = 1.25f;
            }
            else if (p < kDusk)     // Tarde  [0.62, 0.76)
            {
                float t = (p - kAfter) / (kDusk - kAfter);
                skyTop        = Color.Lerp(new Color(0.38f, 0.60f, 0.78f), new Color(0.62f, 0.46f, 0.38f), t);
                skyHorizon    = Color.Lerp(new Color(0.72f, 0.82f, 0.86f), new Color(0.92f, 0.68f, 0.38f), t);
                ambientGround = Color.Lerp(new Color(0.22f, 0.22f, 0.20f), new Color(0.20f, 0.14f, 0.08f), t);
                sunColor      = Color.Lerp(new Color(1f, 0.96f, 0.88f),    new Color(1f, 0.72f, 0.38f),    t);
                sunIntensity  = Mathf.Lerp(1.25f, 0.75f, t);
            }
            else if (p < kEvening)  // Anoitecer  [0.76, 0.88)
            {
                float t = (p - kDusk) / (kEvening - kDusk);
                skyTop        = Color.Lerp(new Color(0.52f, 0.28f, 0.38f), new Color(0.06f, 0.05f, 0.14f), t);
                skyHorizon    = Color.Lerp(new Color(0.88f, 0.42f, 0.18f), new Color(0.14f, 0.10f, 0.22f), t);
                ambientGround = Color.Lerp(new Color(0.18f, 0.10f, 0.06f), new Color(0.03f, 0.02f, 0.05f), t);
                sunColor      = Color.Lerp(new Color(1f, 0.42f, 0.15f),    new Color(0.6f, 0.55f, 0.8f),   t);
                sunIntensity  = Mathf.Lerp(0.75f, 0.08f, t);
            }
            else                    // Noite  [0.88, 1.0)
            {
                float t = (p - kEvening) / (1f - kEvening);
                skyTop        = Color.Lerp(new Color(0.06f, 0.05f, 0.14f), new Color(0.02f, 0.02f, 0.07f), t);
                skyHorizon    = Color.Lerp(new Color(0.12f, 0.09f, 0.20f), new Color(0.04f, 0.04f, 0.10f), t);
                ambientGround = new Color(0.01f, 0.01f, 0.03f);
                sunColor      = new Color(0.6f, 0.65f, 0.9f);
                sunIntensity  = Mathf.Lerp(0.08f, 0.06f, t);
            }

            // Aplica ao sol
            if (sun != null)
            {
                sun.intensity  = sunIntensity;
                sun.color      = sunColor;
                sun.transform.rotation = Quaternion.Euler(sunElevation, sunAzimuth, 0f);
            }

            // Aplica à câmera
            if (cam != null)
                cam.backgroundColor = Color.Lerp(skyHorizon, skyTop, 0.45f);

            // Aplica ao ambiente
            RenderSettings.ambientMode          = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor      = skyTop;
            RenderSettings.ambientEquatorColor  = skyHorizon;
            RenderSettings.ambientGroundColor   = ambientGround;
        }
    }
}
