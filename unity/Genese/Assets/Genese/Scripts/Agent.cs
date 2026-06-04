using UnityEngine;

namespace Genese
{
    /// <summary>
    /// Movimento autônomo de uma criatura (vaguear + saltinho com squash &amp;
    /// stretch). VITRINE: ainda sem decisão real por traços — isso é da Etapa 1.
    /// </summary>
    [RequireComponent(typeof(CreatureView))]
    public class Agent : MonoBehaviour
    {
        // registro global p/ separação (evita sobreposição). Colisão real = Etapa 1.
        static readonly System.Collections.Generic.List<Agent> All = new();

        public float radius = 12.5f;
        public float bodyRadius = 0.55f;   // círculo de colisão (cobre corpo + pernas), escala com tamanho
        float heading, speed, t, pause, hop, pop;
        CreatureView view;

        void Awake()
        {
            view = GetComponent<CreatureView>();
            // resolve refs pelo nome (robusto: independe da atribuição do builder sobreviver)
            if (view.rig == null) view.rig = transform.Find("Rig");
            if (view.body == null && view.rig != null) view.body = view.rig.Find("Body");
            bodyRadius = 0.6f * (view.genome != null ? view.genome.size : 1f);
            heading = Random.value * 6.28f;
            speed = Random.Range(0.5f, 1.1f);
            t = Random.value * 3f;
            hop = Random.value * 6.28f;
        }

        void OnEnable() => All.Add(this);
        void OnDisable() => All.Remove(this);

        // Separação rígida por círculos (soma dos raios; cada um corrige metade).
        void HardSeparate(ref Vector3 p)
        {
            foreach (var o in All)
            {
                if (o == this) continue;
                float min = bodyRadius + o.bodyRadius;
                Vector3 d = p - o.transform.position; d.y = 0;
                float m = d.magnitude;
                if (m > 0.0001f && m < min) p += d / m * (min - m) * 0.5f;
                else if (m <= 0.0001f) p += new Vector3(Random.value - 0.5f, 0, Random.value - 0.5f) * (min * 0.5f);
            }
        }

        public void Pop() => pop = 1f;                 // efeito de mutação
        public void Alert() { pause = 1.6f; }          // reação ao Sinal (tremor/pausa)

        void Update()
        {
            float dt = Time.deltaTime;
            float now = Time.time;
            t -= dt; hop += dt * 8f;
            var rig = view.rig;   // saltinho (bob) — move TODAS as peças juntas
            var body = view.body; // squash & stretch — só o corpo

            if (pause > 0)
            {
                pause -= dt;
                var rp = rig.localPosition; rp.y = Mathf.Abs(Mathf.Sin(now * 8f)) * 0.05f; rig.localPosition = rp; // tremor de alerta
            }
            else
            {
                var p = transform.position;
                p.x += Mathf.Cos(heading) * speed * dt;
                p.z += Mathf.Sin(heading) * speed * dt;
                HardSeparate(ref p);                   // não se sobrepõem
                Obstacles.Resolve(ref p, bodyRadius);  // círculo de colisão (corpo+pernas)

                var W = WorldBuilder.Instance;
                if (W != null)
                {
                    float gh = W.GroundHeight(p.x, p.z);
                    if (gh < W.WaterLevel + 0.15f) { heading += Random.Range(2.2f, 4.1f); p = transform.position; gh = W.GroundHeight(p.x, p.z); } // não entra na água: vira
                    p.y = gh;                          // segue o relevo (montanhas/vales)
                }
                transform.position = p;

                float r = Mathf.Sqrt(p.x * p.x + p.z * p.z);
                if (r > radius) heading = Mathf.Atan2(-p.z, -p.x) + Random.Range(-0.6f, 0.6f);
                if (t <= 0) { t = Random.Range(1.4f, 3.5f); if (Random.value < 0.5f) heading += Random.Range(-1.2f, 1.2f); }

                // saltinho no Rig (corpo + antenas + orelhas sobem/descem juntos)
                float h = Mathf.Abs(Mathf.Sin(hop));
                var rp = rig.localPosition; rp.y = h * 0.12f; rig.localPosition = rp;
                // squash & stretch só no corpo
                float sq = 1f + 0.12f * Mathf.Sin(hop * 2f);
                float wxz = view.wide / Mathf.Sqrt(sq);
                body.localScale = new Vector3(wxz, view.baseTall * sq, wxz);

                transform.rotation = Quaternion.Euler(0, -heading * Mathf.Rad2Deg + 90f, 0);
            }

            if (pop > 0)
            {
                pop -= dt * 2f;
                float s = (view.genome.size) * (1f + Mathf.Max(0, pop) * 0.4f);
                transform.localScale = Vector3.one * s;
            }
        }
    }
}
