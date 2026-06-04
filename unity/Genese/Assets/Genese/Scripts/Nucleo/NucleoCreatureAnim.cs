using UnityEngine;

namespace Genese.Nucleo
{
    /// <summary>
    /// Animação visual das criaturas do núcleo: bob (saltinho), squash &amp; stretch
    /// e tremor de impacto. Não movimenta o transform — isso é tarefa do CoreCreatureView.
    /// Extrai a lógica de animação do Agent.cs da demo e adapta para o sistema multi-civ.
    /// </summary>
    [RequireComponent(typeof(CreatureView))]
    public class NucleoCreatureAnim : MonoBehaviour
    {
        CreatureView _view;
        float   _hop;            // fase acumulada do saltinho
        float   _phase;          // offset aleatório por criatura (evita síncronismo visual)
        Vector3 _lastPos;
        float   _moveSpeed;      // velocidade estimada de movimento
        float   _impact;         // impacto no chão (squash) → desaparece rapidamente
        float   _pop;            // efeito de mutação/nascimento

        void Awake()
        {
            _view  = GetComponent<CreatureView>();
            _phase = Random.value * Mathf.PI * 2f;
            _hop   = Random.value * Mathf.PI * 2f;
        }

        void Start()
        {
            // Resolve referências caso o CreatureBuilder não as tenha ligado ainda
            if (_view.rig  == null) _view.rig  = transform.Find("Rig");
            if (_view.body == null && _view.rig != null) _view.body = _view.rig.Find("Body");
            _lastPos = transform.position;
        }

        /// <summary>Dispara efeito de surgimento/mutação.</summary>
        public void TriggerPop() => _pop = 1f;

        void Update()
        {
            if (_view == null || _view.rig == null || _view.body == null) return;

            float dt = Time.deltaTime;

            // ── Velocidade de movimento (estimada pela variação de posição) ──
            Vector3 cur = transform.position;
            float moved = (cur - _lastPos).magnitude;
            _lastPos = cur;
            float instantSpeed = moved / Mathf.Max(dt, 0.001f);
            _moveSpeed = Mathf.Lerp(_moveSpeed, instantSpeed, dt * 6f);

            // ── Hop phase — acelera com o movimento ──────────────────────────
            float hopRate = 4.5f + _moveSpeed * 2.8f;
            _hop += dt * hopRate;

            // ── Bob: rig sobe e desce junto (corpo + orelhas + antenas) ──────
            float h  = Mathf.Abs(Mathf.Sin(_hop + _phase));
            float bob = h * (0.10f + _moveSpeed * 0.05f);
            var rp = _view.rig.localPosition;
            rp.y = bob;
            _view.rig.localPosition = rp;

            // ── Impacto: squash no momento de aterrar ────────────────────────
            bool landing = h < 0.08f && _moveSpeed > 0.2f;
            if (landing) _impact = 0.22f;
            _impact = Mathf.Max(0f, _impact - dt * 4f);

            // ── Squash & stretch no body ──────────────────────────────────────
            // sq > 1 = esticado (no ar), sq < 1 = achatado (aterragem)
            float sq = 1f + Mathf.Sin(_hop * 2f + _phase) * 0.10f - _impact * 0.25f;
            sq = Mathf.Max(0.55f, sq);
            float wxz = _view.wide / Mathf.Sqrt(sq);
            _view.body.localScale = new Vector3(wxz, _view.baseTall * sq, wxz);

            // ── Pop (nascimento / mutação) ────────────────────────────────────
            if (_pop > 0f)
            {
                _pop -= dt * 2.5f;
                float popScale = _view.genome != null ? _view.genome.size : 1f;
                transform.localScale = Vector3.one * popScale * (1f + Mathf.Max(0f, _pop) * 0.45f);
            }
        }
    }
}
