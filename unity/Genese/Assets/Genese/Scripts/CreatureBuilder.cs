using UnityEngine;

namespace Genese
{
    /// <summary>
    /// Monta uma criatura 3D a partir do Genome — porta de creature.js (Claude Design).
    /// Eixos fiéis: shape, color/blend, pattern, eyes/pupil, mouth, ears, tail,
    /// legs, arms, antennae, finish, emit, ornament, signal, glow, size.
    /// Retorna a raiz; a animação usa o CreatureView anexado.
    /// </summary>
    public static class CreatureBuilder
    {
        struct Shape { public float wide, tall, bottom, lean; }

        static Shape ShapeOf(string s) => s switch
        {
            "round" => new Shape { wide = 1.06f, tall = 1.00f },
            "tall"  => new Shape { wide = 0.86f, tall = 1.42f },
            "squat" => new Shape { wide = 1.20f, tall = 0.82f },
            "pear"  => new Shape { wide = 0.92f, tall = 1.10f, bottom = 0.5f },
            "bean"  => new Shape { wide = 0.96f, tall = 1.18f, lean = 0.12f },
            "blob"  => new Shape { wide = 1.04f, tall = 0.96f, bottom = 0.15f },
            _       => new Shape { wide = 1.00f, tall = 1.15f }, // egg
        };

        static Prim.Finish FinishOf(string f) => f switch
        {
            "satin" => Prim.Finish.Satin,
            "metallic" => Prim.Finish.Metallic,
            "iridescent" => Prim.Finish.Iridescent,
            _ => Prim.Finish.Matte,
        };

        // helpers locais ------------------------------------------------------
        static GameObject Sphere(Transform p, Material m, Vector3 pos, Vector3 scl)
        {
            var go = Prim.Sphere(p, m); go.transform.localPosition = pos; go.transform.localScale = scl; return go;
        }

        public static GameObject Build(Genome g)
        {
            var root = new GameObject("Criatura");
            // Rig agrupa todas as peças → o saltinho (bob) é aplicado nele (tudo se move junto);
            // o squash & stretch fica só no corpo. Assim antenas/orelhas acompanham o movimento.
            var rig = new GameObject("Rig");
            rig.transform.SetParent(root.transform, false);
            var sh = ShapeOf(g.shape);
            float wide = sh.wide, tall = sh.tall;
            var fin = FinishOf(g.finish);
            Color emitCol = g.emit == "body" ? g.signal : g.color;
            float emitI = g.emit == "body" ? 0.4f : 0f;
            var bodyMat = Prim.Mat(g.color, fin, emitCol, emitI);

            // corpo (animado)
            var body = Prim.Sphere(rig.transform, bodyMat);
            body.name = "Body";
            body.transform.localScale = new Vector3(wide, tall, wide);
            body.transform.localPosition = new Vector3(0, 0.5f * tall, 0);
            if (sh.lean > 0) body.transform.localRotation = Quaternion.Euler(0, 0, sh.lean * Mathf.Rad2Deg);

            // bojo inferior (pear/blob)
            if (sh.bottom > 0)
                Sphere(rig.transform, bodyMat, new Vector3(0, 0.34f * tall, 0),
                       new Vector3(wide * (1 + sh.bottom), tall * 0.7f, wide * (1 + sh.bottom)));

            // ventre / dorsal / mistura de cor
            Color bellyCol = (g.blend == "twotone" || g.blend == "belly2" || g.blend == "gradient")
                ? g.color2 : Color.Lerp(g.color, Color.white, 0.45f);
            if (g.pattern == "belly" || g.blend == "belly2" || g.blend == "twotone" || g.blend == "gradient")
                Sphere(rig.transform, Prim.Mat(bellyCol, fin), new Vector3(0, 0.42f * tall, 0.2f * wide),
                       new Vector3(0.64f * wide, 0.66f * tall, 0.52f * wide));
            if (g.blend == "dorsal")
                Sphere(rig.transform, Prim.Mat(g.color2, fin), new Vector3(0, 0.6f * tall, -0.18f * wide),
                       new Vector3(0.5f * wide, 0.7f * tall, 0.5f * wide));

            // padrão: pintas / listras
            if (g.pattern == "spots")
            {
                var spotMat = Prim.Mat(g.color * 0.65f, fin);
                for (int i = 0; i < 7; i++)
                {
                    float a = Random.value * 6.28f, h = Random.Range(0.3f, 0.85f) * tall;
                    float r = 0.5f * wide * Mathf.Sin((h / tall) * Mathf.PI * 0.9f);
                    float s = Random.Range(0.10f, 0.18f);
                    Sphere(rig.transform, spotMat, new Vector3(Mathf.Cos(a) * r, h, Mathf.Sin(a) * r), new Vector3(s, s, s));
                }
            }
            else if (g.pattern == "stripes")
            {
                var stripeMat = Prim.Mat(g.color * 0.62f, fin);
                for (int i = 0; i < 3; i++)
                {
                    var ring = Prim.Cylinder(rig.transform, stripeMat);
                    ring.transform.localPosition = new Vector3(0, (0.32f + i * 0.22f) * tall, 0);
                    ring.transform.localScale = new Vector3(wide * 0.95f, 0.03f, wide * 0.95f);
                }
            }

            // olhos
            var eyeMat = Prim.Mat(Color.white, Prim.Finish.Satin);
            bool eyeEmit = g.emit == "eyes";
            var pupMat = Prim.Mat(eyeEmit ? g.signal : Palette.Hex("#16121b"), Prim.Finish.Satin,
                                  eyeEmit ? g.signal : (Color?)null, eyeEmit ? 0.9f : 0f);
            float eyeR = g.eyes == "big" ? 0.17f : g.eyes == "small" ? 0.10f : 0.135f;
            Vector2[] eyes = g.eyes switch
            {
                "one" => new[] { new Vector2(0, 0.66f * tall) },
                "three" => new[] { new Vector2(-0.17f, 0.6f * tall), new Vector2(0.17f, 0.6f * tall), new Vector2(0, 0.78f * tall) },
                _ => new[] { new Vector2(-0.17f, 0.62f * tall), new Vector2(0.17f, 0.62f * tall) },
            };
            foreach (var e in eyes)
            {
                Sphere(rig.transform, eyeMat, new Vector3(e.x, e.y, 0.34f * wide), Vector3.one * (eyeR * 2));
                float pr = eyeR * 0.46f * 2;
                Sphere(rig.transform, pupMat, new Vector3(e.x, e.y, 0.34f * wide + eyeR * 0.7f),
                       new Vector3(g.pupil == "vertical" ? pr * 0.55f : pr, g.pupil == "vertical" ? pr * 1.4f : pr, pr));
            }

            // boca
            if (g.mouth == "beak")
            {
                var beak = Prim.Cone(rig.transform, Prim.Mat(Color.Lerp(g.signal, g.color, 0.3f), Prim.Finish.Satin), 0.12f, 0.24f, 8);
                beak.transform.localPosition = new Vector3(0, 0.5f * tall, 0.45f * wide);
                beak.transform.localRotation = Quaternion.Euler(90, 0, 0);
            }
            else if (g.mouth == "tusks")
            {
                var tuskMat = Prim.Mat(Palette.Hex("#efe7d0"), Prim.Finish.Satin);
                for (int sx = -1; sx <= 1; sx += 2)
                {
                    var tk = Prim.Cone(rig.transform, tuskMat, 0.04f, 0.22f, 6);
                    tk.transform.localPosition = new Vector3(0.1f * sx, 0.4f * tall, 0.4f * wide);
                    tk.transform.localRotation = Quaternion.Euler(180, 0, 0);
                }
            }

            // pernas / pés
            var footMat = Prim.Mat(g.color * 0.7f, Prim.Finish.Satin);
            if (g.legs == "stubby")
                for (int sx = -1; sx <= 1; sx += 2)
                    Sphere(rig.transform, footMat, new Vector3(0.22f * sx, 0.07f, 0.06f), new Vector3(0.34f, 0.17f, 0.42f));
            else if (g.legs == "tall")
                for (int sx = -1; sx <= 1; sx += 2)
                {
                    var l = Prim.Cylinder(rig.transform, footMat);
                    l.transform.localPosition = new Vector3(0.2f * sx, 0.2f, 0.04f);
                    l.transform.localScale = new Vector3(0.14f, 0.2f, 0.14f);
                    Sphere(rig.transform, footMat, new Vector3(0.2f * sx, 0.04f, 0.1f), new Vector3(0.24f, 0.12f, 0.3f));
                }

            // braços
            if (g.arms == "stubby")
                for (int sx = -1; sx <= 1; sx += 2)
                    Sphere(rig.transform, bodyMat, new Vector3(0.5f * wide * sx, 0.45f * tall, 0.05f), new Vector3(0.24f, 0.36f, 0.24f));
            else if (g.arms == "claws")
                for (int sx = -1; sx <= 1; sx += 2)
                {
                    var a = Prim.Cone(rig.transform, footMat, 0.08f, 0.3f, 7);
                    a.transform.localPosition = new Vector3(0.55f * wide * sx, 0.5f * tall, 0.05f);
                    a.transform.localRotation = Quaternion.Euler(0, 0, sx * -120);
                }
            else if (g.arms == "flippers")
                for (int sx = -1; sx <= 1; sx += 2)
                {
                    var a = Sphere(rig.transform, bodyMat, new Vector3(0.52f * wide * sx, 0.4f * tall, 0.05f), new Vector3(0.18f, 0.5f, 0.36f));
                    a.transform.localRotation = Quaternion.Euler(0, 0, sx * 23);
                }

            // orelhas
            if (g.ears == "pointy")
                for (int sx = -1; sx <= 1; sx += 2)
                {
                    var e = Prim.Cone(rig.transform, bodyMat, 0.12f, 0.34f, 10);
                    e.transform.localPosition = new Vector3(0.24f * sx, 0.78f * tall, 0);
                    e.transform.localRotation = Quaternion.Euler(0, 0, sx * 20);
                }
            else if (g.ears == "round")
                for (int sx = -1; sx <= 1; sx += 2)
                    Sphere(rig.transform, bodyMat, new Vector3(0.3f * sx, 0.92f * tall, 0), new Vector3(0.26f, 0.26f, 0.13f));
            else if (g.ears == "tuft")
                for (int sx = -1; sx <= 1; sx += 2)
                {
                    var e = Prim.Cone(rig.transform, Prim.Mat(g.color2, Prim.Finish.Satin), 0.08f, 0.3f, 7);
                    e.transform.localPosition = new Vector3(0.22f * sx, 0.85f * tall, 0);
                    e.transform.localRotation = Quaternion.Euler(0, 0, sx * 17);
                }
            else if (g.ears == "fan")
                for (int sx = -1; sx <= 1; sx += 2)
                    Sphere(rig.transform, Prim.Mat(g.color2, Prim.Finish.Satin), new Vector3(0.34f * sx, 0.85f * tall, 0), new Vector3(0.4f, 0.4f, 0.06f));

            // antenas / chifres / crista / crina
            var view = root.AddComponent<CreatureView>();
            if (g.antennae == "pair")
                for (int sx = -1; sx <= 1; sx += 2)
                {
                    var stalk = Prim.Cylinder(rig.transform, footMat);
                    stalk.transform.localPosition = new Vector3(0.1f * sx, 1.05f * tall, 0);
                    stalk.transform.localScale = new Vector3(0.04f, 0.2f, 0.04f);
                    Sphere(rig.transform, Prim.Mat(g.signal, Prim.Finish.Satin, g.signal, 0.7f), new Vector3(0.1f * sx, 1.26f * tall, 0), Vector3.one * 0.16f);
                }
            else if (g.antennae == "horns")
                for (int sx = -1; sx <= 1; sx += 2)
                {
                    var h = Prim.Cone(rig.transform, Prim.Mat(g.color * 0.6f, Prim.Finish.Satin), 0.08f, 0.4f, 8);
                    h.transform.localPosition = new Vector3(0.18f * sx, 1.0f * tall, 0);
                    h.transform.localRotation = Quaternion.Euler(0, 0, sx * -23);
                }
            else if (g.antennae == "crest")
                for (int i = -1; i <= 1; i++)
                {
                    var c = Prim.Cone(rig.transform, Prim.Mat(g.color2, Prim.Finish.Satin), 0.07f, i == 0 ? 0.36f : 0.22f, 6);
                    c.transform.localPosition = new Vector3(0, 0.95f * tall, -i * 0.14f);
                }
            else if (g.antennae == "mane")
                for (int i = 0; i < 8; i++)
                {
                    float a = (float)i / 8 * 6.28f;
                    Sphere(rig.transform, Prim.Mat(g.color2, Prim.Finish.Satin), new Vector3(Mathf.Cos(a) * 0.3f, 0.85f * tall, Mathf.Sin(a) * 0.3f - 0.1f), Vector3.one * 0.16f);
                }

            // cauda
            if (g.tail == "short" || g.tail == "long")
            {
                float len = g.tail == "long" ? 0.5f : 0.28f;
                var t = Prim.Cone(rig.transform, bodyMat, 0.12f, len, 8);
                t.transform.localPosition = new Vector3(0, 0.4f * tall, -0.5f * wide - len * 0.3f);
                t.transform.localRotation = Quaternion.Euler(-63, 0, 0);
            }
            else if (g.tail == "tuft")
                Sphere(rig.transform, Prim.Mat(g.color2, Prim.Finish.Satin), new Vector3(0, 0.45f * tall, -0.52f * wide), Vector3.one * 0.28f);

            // ornamento cultural
            if (g.ornament == "headband")
            {
                var band = Prim.Cylinder(rig.transform, Prim.Mat(g.signal, Prim.Finish.Satin));
                band.transform.localPosition = new Vector3(0, 0.78f * tall, 0);
                band.transform.localScale = new Vector3(0.86f * wide, 0.03f, 0.86f * wide);
            }
            else if (g.ornament == "facepaint")
                Sphere(rig.transform, Prim.Mat(g.signal, Prim.Finish.Satin), new Vector3(0, 0.52f * tall, 0.18f * wide), new Vector3(0.55f * wide, 0.28f * tall, 0.5f * wide));
            else if (g.ornament == "crest")
                for (int i = 0; i < 3; i++)
                {
                    var c = Prim.Cone(rig.transform, Prim.Mat(g.signal, Prim.Finish.Satin), 0.06f, 0.2f, 6);
                    c.transform.localPosition = new Vector3(0, (0.92f - i * 0.05f) * tall, -0.1f - i * 0.14f);
                }
            else if (g.ornament == "religious")
                Sphere(rig.transform, Prim.Mat(Palette.PedraCoracao, Prim.Finish.Satin, Palette.PedraCoracao, 0.8f), new Vector3(0, 1.25f * tall, 0), Vector3.one * 0.2f);

            // aura de sinal
            if (g.glow)
            {
                var lgo = new GameObject("Aura");
                lgo.transform.SetParent(rig.transform, false);
                lgo.transform.localPosition = new Vector3(0, 0.6f * tall, 0);
                var light = lgo.AddComponent<Light>();
                light.type = LightType.Point; light.color = g.signal; light.range = 3f; light.intensity = 0.5f;
            }

            root.transform.localScale = Vector3.one * g.size;
            view.rig = rig.transform; view.body = body.transform; view.baseTall = tall; view.wide = wide; view.genome = g;
            return root;
        }
    }
}
