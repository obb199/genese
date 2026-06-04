using UnityEngine;

namespace Genese
{
    /// <summary>Pedaço de efeito: escala + desloca (vetor) + some por transparência.</summary>
    public class FxBit : MonoBehaviour
    {
        Material mat; Vector3 start, from, to, move; float life, age, startA;

        public void Init(Material m, Vector3 startPos, float life, Vector3 from, Vector3 to, float startA, Vector3 move)
        {
            mat = m; start = startPos; this.life = life; this.from = from; this.to = to; this.startA = startA; this.move = move;
            transform.position = startPos; transform.localScale = from;
        }
        void Update()
        {
            age += Time.deltaTime;
            float p = Mathf.Clamp01(age / life), e = 1f - (1f - p) * (1f - p); // ease-out
            transform.localScale = Vector3.Lerp(from, to, e);
            transform.position = start + move * e;
            if (mat) { var c = mat.color; c.a = startA * (1f - p); mat.color = c; }
            if (p >= 1f) Destroy(gameObject);
        }
    }

    /// <summary>Luz que pisca e some (impacto do nudge).</summary>
    public class FlashLight : MonoBehaviour
    {
        Light l; float life, age, start;
        public void Init(float life) { l = GetComponent<Light>(); start = l.intensity; this.life = life; }
        void Update() { age += Time.deltaTime; if (l) l.intensity = start * (1f - age / life); if (age >= life) Destroy(gameObject); }
    }

    public static class Fx
    {
        static readonly Color Gold = Palette.Hex("#EAD37A");
        static readonly Color GoldB = Palette.Hex("#E0C46A");

        static Shader _sh;
        static Shader Sh => _sh != null ? _sh : (_sh = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard"));

        static GameObject Glow(PrimitiveType t, Vector3 pos, Color col, float alpha)
        {
            var go = GameObject.CreatePrimitive(t);
            var c = go.GetComponent<Collider>(); if (c) Object.Destroy(c);
            var m = new Material(Sh); col.a = alpha; m.color = col; m.SetColor("_Color", col);
            m.SetColor("_EmissionColor", col); m.EnableKeyword("_EMISSION");
            go.GetComponent<MeshRenderer>().sharedMaterial = m; go.transform.position = pos;
            return go;
        }
        static void Bit(GameObject go, float life, Vector3 from, Vector3 to, float startA, Vector3 move)
            => go.AddComponent<FxBit>().Init(go.GetComponent<MeshRenderer>().sharedMaterial, go.transform.position, life, from, to, startA, move);

        static void Flash(Vector3 p, Color col, float intensity, float life)
        {
            var go = new GameObject("Flash"); go.transform.position = p;
            var l = go.AddComponent<Light>(); l.type = LightType.Point; l.color = col; l.range = 7f; l.intensity = intensity;
            go.AddComponent<FlashLight>().Init(life);
        }

        // motes: pequenas esferas de luz, suaves
        static void Motes(Vector3 center, Color col, int n, float spread, Vector3 move, float life, float alpha)
        {
            for (int i = 0; i < n; i++)
            {
                float a = i / (float)n * 6.28f + Random.Range(-0.3f, 0.3f), rr = Random.Range(0.2f, spread);
                var pos = center + new Vector3(Mathf.Cos(a) * rr, Random.Range(0f, 0.4f), Mathf.Sin(a) * rr);
                var mo = Glow(PrimitiveType.Sphere, pos, col, alpha);
                float sz = Random.Range(0.05f, 0.11f);
                Bit(mo, life * Random.Range(0.85f, 1.2f), Vector3.one * sz, Vector3.one * 0.01f, alpha, move + new Vector3(Mathf.Cos(a) * rr * 0.35f, 0, Mathf.Sin(a) * rr * 0.35f));
            }
        }

        // Sinal — coluna de luz suave + anel roxo + motes descendo (gentil)
        public static void Signal(Vector3 p)
        {
            var beam = Glow(PrimitiveType.Cylinder, p + Vector3.up * 6f, Gold, 0.22f);
            Bit(beam, 1.5f, new Vector3(0.45f, 6f, 0.45f), new Vector3(0.1f, 6f, 0.1f), 0.22f, Vector3.zero);
            var ring = Glow(PrimitiveType.Cylinder, p + Vector3.up * 0.05f, Palette.Influencia, 0.4f);
            Bit(ring, 1.4f, new Vector3(0.4f, 0.015f, 0.4f), new Vector3(4.6f, 0.015f, 4.6f), 0.4f, Vector3.zero);
            Motes(p + Vector3.up * 3.5f, Gold, 7, 1.0f, Vector3.down * 3f, 1.5f, 0.6f);
            Flash(p + Vector3.up * 1.2f, Gold, 2.2f, 0.7f);
        }

        // Faísca — clarão curto + anel + estilhaços discretos
        public static void Faisca(Vector3 p)
        {
            var burst = Glow(PrimitiveType.Sphere, p + Vector3.up * 0.5f, Palette.Tensao, 0.55f);
            Bit(burst, 0.55f, Vector3.one * 0.18f, Vector3.one * 1.4f, 0.55f, Vector3.zero);
            var ring = Glow(PrimitiveType.Cylinder, p + Vector3.up * 0.05f, Palette.Hex("#E8A24A"), 0.4f);
            Bit(ring, 0.8f, new Vector3(0.3f, 0.015f, 0.3f), new Vector3(2.4f, 0.015f, 2.4f), 0.4f, Vector3.zero);
            Motes(p + Vector3.up * 0.5f, Palette.Hex("#E8A24A"), 6, 1.1f, Vector3.up * 0.7f, 0.7f, 0.65f);
            Flash(p + Vector3.up * 0.6f, Palette.Tensao, 2f, 0.5f);
        }

        // Inspiração — halo suave que sobe + motes em espiral
        public static void Inspiracao(Vector3 p)
        {
            var halo = Glow(PrimitiveType.Cylinder, p + Vector3.up * 0.4f, GoldB, 0.42f);
            Bit(halo, 1.7f, new Vector3(0.6f, 0.015f, 0.6f), new Vector3(2.6f, 0.015f, 2.6f), 0.42f, Vector3.up * 2.4f);
            var core = Glow(PrimitiveType.Sphere, p + Vector3.up * 0.6f, Gold, 0.5f);
            Bit(core, 1.4f, Vector3.one * 0.2f, Vector3.one * 0.7f, 0.5f, Vector3.up * 1.5f);
            Motes(p + Vector3.up * 0.4f, Gold, 9, 0.8f, Vector3.up * 2.6f, 1.5f, 0.6f);
            Flash(p + Vector3.up * 1f, GoldB, 1.8f, 0.9f);
        }
    }
}
