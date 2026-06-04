using UnityEngine;

namespace Genese.Nucleo
{
    // ============================================================
    //  PALETA DE CULTURA (10 culturas do Claude Design)
    // ============================================================
    public struct BuildingPal
    {
        public Color wall, roof, wood, stone, accent, flame, dark;

        static Color H(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }

        public static BuildingPal ForCulture(int idx) => idx switch
        {
            1 => new BuildingPal { wall=H("#C9A86A"),roof=H("#A8843E"),wood=H("#8A6A3A"),stone=H("#B89A60"),accent=H("#C0563A"),flame=H("#E08A3A"),dark=H("#4a3826") }, // Árido
            2 => new BuildingPal { wall=H("#C7BBA2"),roof=H("#7E4A3E"),wood=H("#6E5638"),stone=H("#A9A294"),accent=H("#B7402E"),flame=H("#E08A3A"),dark=H("#34302a") }, // Medieval
            3 => new BuildingPal { wall=H("#5E4E86"),roof=H("#3E2E66"),wood=H("#4A3A6A"),stone=H("#6A5C94"),accent=H("#C9A0FF"),flame=H("#9C6BE0"),dark=H("#241c38") }, // Arcana
            4 => new BuildingPal { wall=H("#E0D6C0"),roof=H("#A8443A"),wood=H("#9A7A52"),stone=H("#D2C8AE"),accent=H("#C9A04A"),flame=H("#E08A3A"),dark=H("#4a4234") }, // Imperial
            5 => new BuildingPal { wall=H("#7A8088"),roof=H("#4A5258"),wood=H("#5A4E42"),stone=H("#8A9098"),accent=H("#E0A030"),flame=H("#E0A030"),dark=H("#2a2e32") }, // Tecnológica
            6 => new BuildingPal { wall=H("#5E8C8C"),roof=H("#2E5A66"),wood=H("#3E5A52"),stone=H("#6E9298"),accent=H("#7FD8C8"),flame=H("#5FE0C2"),dark=H("#22343a") }, // Aquática
            7 => new BuildingPal { wall=H("#C26A4A"),roof=H("#8A3E2E"),wood=H("#7A5638"),stone=H("#B89A60"),accent=H("#E8C24A"),flame=H("#E08A3A"),dark=H("#3a261c") }, // Nômade
            8 => new BuildingPal { wall=H("#5A5466"),roof=H("#3E3A4A"),wood=H("#4A4236"),stone=H("#6A6276"),accent=H("#7DF0C8"),flame=H("#7DF0C8"),dark=H("#1f1c28") }, // Subterrânea
            9 => new BuildingPal { wall=H("#C8BBA0"),roof=H("#9A5A4A"),wood=H("#8A6A4A"),stone=H("#B6AC96"),accent=H("#8C5BAA"),flame=H("#8C5BAA"),dark=H("#3a342c") }, // Ordem
            _ => new BuildingPal { wall=H("#7C6A48"),roof=H("#3C6E4A"),wood=H("#5A4630"),stone=H("#7E8478"),accent=H("#C9A04A"),flame=H("#5FE0C2"),dark=H("#2c241c") }  // Floresta
        };
    }

    // ============================================================
    //  BUILDER — cada método cria a construção em 3D com muito
    //  detalhe arquitetônico fiel ao Claude Design (buildings.js +
    //  buildings-extra*.js). Base-center = pé da construção.
    // ============================================================
    public static class BuildingBuilder
    {
        static Color H(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }

        // ---- helpers ----
        static GameObject Cube(Transform p, Material m, Vector3 sz, Vector3 pos, Vector3 rot=default)
        { var g=Prim.Cube(p,m); g.transform.localScale=sz; g.transform.localPosition=pos; if(rot!=default) g.transform.localEulerAngles=rot; return g; }
        static GameObject Cyl(Transform p, Material m, float r, float h, Vector3 pos)
        { var g=Prim.Cylinder(p,m); g.transform.localScale=new Vector3(r*2,h,r*2); g.transform.localPosition=pos; return g; }
        static GameObject Cone(Transform p, Material m, float r, float h, Vector3 pos, Quaternion rot=default, int seg=4)
        { var g=Prim.Cone(p,m,r,h,seg); g.transform.localPosition=pos; if(rot!=default) g.transform.localRotation=rot; return g; }
        static GameObject Sph(Transform p, Material m, float r, Vector3 pos)
        { var g=Prim.Sphere(p,m); g.transform.localScale=Vector3.one*r*2; g.transform.localPosition=pos; return g; }
        static Material Mat(Color c)           => Prim.Mat(c, Prim.Finish.Matte);
        static Material Sat(Color c)           => Prim.Mat(c, Prim.Finish.Satin);
        static Material Met(Color c)           => Prim.Mat(c, Prim.Finish.Metallic);
        static Material Glow(Color c,float i=1.3f) => Prim.Mat(c,Prim.Finish.Satin,c,i);

        // ================================================================
        //  BANDO
        // ================================================================

        /// <summary>Fogueira: anel de 8 pedras, lenhas cruzadas, chama dupla, brasas.</summary>
        public static void Fogueira(Transform p, BuildingPal pal, float s = 1f)
        {
            var stone=Mat(pal.stone); var wood=Mat(pal.wood);
            // pedras em anel
            for(int i=0;i<8;i++){float a=i/8f*Mathf.PI*2; var r=Prim.Sphere(p,stone); r.transform.localScale=new Vector3(0.24f,0.14f,0.24f)*s; r.transform.localPosition=new Vector3(Mathf.Cos(a)*0.5f,0.07f,Mathf.Sin(a)*0.5f)*s;}
            // lenhas cruzadas (3 pares)
            for(int i=0;i<3;i++){ float a=i*60f; var log=Prim.Cylinder(p,wood); log.transform.localScale=new Vector3(0.09f,0.56f,0.09f)*s; log.transform.localPosition=new Vector3(0,0.12f,0)*s; log.transform.localEulerAngles=new Vector3(82f,a,0);}
            // chama principal
            Cone(p, Glow(pal.flame,1.4f), 0.2f*s, 0.75f*s, new Vector3(0,0.12f,0)*s, default, 7);
            Cone(p, Glow(pal.accent,1.7f), 0.1f*s, 0.52f*s, new Vector3(0,0.48f,0)*s, default, 5);
            // brasas ao redor
            for(int i=0;i<5;i++){float a=i/5f*Mathf.PI*2; Sph(p,Glow(pal.flame,0.9f),0.06f*s,new Vector3(Mathf.Cos(a)*0.25f*s,0.08f*s,Mathf.Sin(a)*0.25f*s));}
        }

        /// <summary>Tenda: pirâmide com padrão de cor, postes, cordas e entrada escura.</summary>
        public static void Tenda(Transform p, BuildingPal pal, float s = 1f)
        {
            // corpo da tenda
            Cone(p, Mat(pal.wall), 1.2f*s, 2.0f*s, Vector3.zero, Quaternion.Euler(0,45,0), 4);
            // faixas decorativas (anéis planos)
            for(int i=0;i<2;i++){var band=Prim.Cylinder(p,Mat(pal.accent)); band.transform.localScale=new Vector3((1.2f-i*0.45f)*2*s,0.05f*s,(1.2f-i*0.45f)*2*s); band.transform.localPosition=new Vector3(0,(0.5f+i*0.65f)*s,0);}
            // entrada
            Cube(p, Mat(pal.dark), new Vector3(0.45f,0.6f,0.1f)*s, new Vector3(0,0.3f,1.18f)*s);
            // postes nos cantos
            for(int i=0;i<4;i++){float a=i*90f*Mathf.Deg2Rad; Cyl(p,Mat(pal.wood),0.04f*s,0.3f*s,new Vector3(Mathf.Cos(a)*1.1f*s,0.15f*s,Mathf.Sin(a)*1.1f*s));}
            // postes do ápice
            for(int i=0;i<2;i++) Cyl(p,Mat(pal.wood),0.028f*s,0.28f*s,new Vector3((i==0?-1:1)*0.07f*s,2.06f*s,0));
            // ornamento no topo
            Sph(p, Sat(pal.accent), 0.1f*s, new Vector3(0,2.22f*s,0));
        }

        // ================================================================
        //  TRIBAL
        // ================================================================

        /// <summary>Choça: cúpula grande com sapê, listras de palha, porta arqueada, poste central.</summary>
        public static void Choca(Transform p, BuildingPal pal, float s = 1f)
        {
            // cúpula principal
            var body=Prim.Sphere(p, Mat(pal.wall));
            body.transform.localScale=new Vector3(2.3f,1.2f,2.3f)*s;
            body.transform.localPosition=new Vector3(0,0.6f,0)*s;
            // anéis de sapê (6 camadas)
            for(int i=0;i<5;i++){float y=0.18f+i*0.22f, r=1.1f-i*0.18f;
                var ring=Prim.Cylinder(p,Mat(pal.roof)); ring.transform.localScale=new Vector3(r*2*s,0.04f*s,r*2*s); ring.transform.localPosition=new Vector3(0,y*s,0);}
            // base de pedra
            Cyl(p, Mat(pal.stone), 1.2f*s, 0.12f*s, new Vector3(0,0.06f,0)*s);
            // porta arqueada
            Cone(p, Mat(pal.dark), 0.24f*s, 0.32f*s, new Vector3(0,0.32f,1.13f)*s, default, 10);
            Cube(p, Mat(pal.dark), new Vector3(0.42f,0.42f,0.1f)*s, new Vector3(0,0.21f,1.13f)*s);
            // poste decorativo no topo
            Cyl(p, Mat(pal.wood), 0.045f*s, 0.32f*s, new Vector3(0,1.18f,0)*s);
            Cone(p, Sat(pal.accent), 0.1f*s, 0.16f*s, new Vector3(0,1.5f,0)*s, default, 4);
        }

        /// <summary>Totem: poste alto com 4 faces esculpidas, cocar, base decorada.</summary>
        public static void Totem(Transform p, BuildingPal pal, float s = 1f)
        {
            // base pedestal
            Cube(p, Mat(pal.stone), new Vector3(0.7f,0.16f,0.7f)*s, new Vector3(0,0.08f,0)*s);
            // poste principal
            Cyl(p, Mat(pal.wood), 0.2f*s, 1.3f*s, new Vector3(0,1.3f,0)*s);
            // 4 discos de face (cor alternada)
            float[] yy={0.4f,0.85f,1.35f,1.82f}; Color[] cc={pal.accent,pal.wall,pal.flame,pal.accent};
            for(int i=0;i<4;i++){
                var disc=Prim.Cylinder(p,Sat(cc[i])); disc.transform.localScale=new Vector3(0.5f,0.19f,0.5f)*s; disc.transform.localPosition=new Vector3(0,yy[i],0)*s;
                // olhos
                Sph(p,Mat(H("#23201c")),0.06f*s,new Vector3(-0.12f*s,yy[i]*s+0.04f*s,0.24f*s));
                Sph(p,Mat(H("#23201c")),0.06f*s,new Vector3(0.12f*s,yy[i]*s+0.04f*s,0.24f*s));
            }
            // cocar no topo
            Cone(p, Sat(pal.accent), 0.3f*s, 0.4f*s, new Vector3(0,2.36f,0)*s, Quaternion.Euler(0,45,0));
            for(int i=0;i<5;i++){float a=i/5f*Mathf.PI*2; Sph(p,Sat(pal.flame),0.07f*s,new Vector3(Mathf.Cos(a)*0.28f*s,2.5f*s,Mathf.Sin(a)*0.28f*s));}
        }

        /// <summary>Cerca: palissada com entalhes, vigas duplas, portal.</summary>
        public static void Cerca(Transform p, BuildingPal pal, float s = 1f)
        {
            for(int i=0;i<7;i++){
                float x=(i-3)*0.56f*s;
                Cyl(p, Mat(pal.wood), 0.065f*s, 0.52f*s, new Vector3(x,0.52f,0)*s);
                Cone(p, Mat(pal.wood), 0.075f*s, 0.18f*s, new Vector3(x,1.04f,0)*s, default, 4);
                // entalhe decorativo
                if(i%2==0) Cube(p, Mat(pal.accent*0.7f), new Vector3(0.05f*s,0.1f*s,0.12f*s), new Vector3(x,0.6f*s,0));
            }
            // vigas duplas
            for(int i=0;i<2;i++){var rail=Prim.Cylinder(p,Mat(pal.wood)); rail.transform.localScale=new Vector3(0.065f,1.85f,0.065f)*s; rail.transform.localPosition=new Vector3(0,(0.36f+i*0.35f),0)*s; rail.transform.localEulerAngles=new Vector3(0,0,90);}
        }

        // ================================================================
        //  ALDEIA
        // ================================================================

        /// <summary>Casa de madeira: pedra na base, 2 andares, janelas com taipal, chaminé fumegante.</summary>
        public static void CasaMadeira(Transform p, BuildingPal pal, float s = 1f)
        {
            float w=2.0f,d=1.8f,h1=0.9f,h2=1.1f;
            // fundação de pedra
            Cube(p, Mat(pal.stone), new Vector3(w+0.3f,0.2f,d+0.3f)*s, new Vector3(0,0.1f,0)*s);
            // andar térreo
            Cube(p, Mat(pal.wall), new Vector3(w,h1,d)*s, new Vector3(0,h1/2+0.2f,0)*s);
            // vigas de madeira na fachada (detalhes estruturais)
            for(int i=0;i<3;i++){float x=(i-1)*0.72f*s; Cube(p,Mat(pal.wood),new Vector3(0.07f,h1,0.08f)*s,new Vector3(x,(h1/2+0.2f),d*0.5f)*s);}
            Cube(p, Mat(pal.wood), new Vector3(w+0.05f,0.06f,0.08f)*s, new Vector3(0,h1+0.2f,d*0.5f)*s); // viga horizontal
            // 2º andar
            Cube(p, Mat(pal.roof*0.85f), new Vector3(w,h2,d)*s, new Vector3(0,h1+h2/2+0.2f,0)*s);
            // telhado 4-águas
            Cone(p, Mat(pal.roof), Mathf.Max(w,d)*0.68f*s, 0.95f*s, new Vector3(0,h1+h2+0.2f,0)*s, Quaternion.Euler(0,45,0));
            // janelas do 1º andar
            Cube(p, Sat(pal.accent), new Vector3(0.3f,0.3f,0.08f)*s, new Vector3(-0.62f,h1+0.2f,d/2+0.02f)*s);
            Cube(p, Sat(pal.accent), new Vector3(0.3f,0.3f,0.08f)*s, new Vector3(0.62f,h1+0.2f,d/2+0.02f)*s);
            // janelas do 2º andar
            Cube(p, Sat(pal.accent), new Vector3(0.3f,0.28f,0.08f)*s, new Vector3(-0.55f,h1+h2*0.55f+0.2f,d/2+0.02f)*s);
            Cube(p, Sat(pal.accent), new Vector3(0.3f,0.28f,0.08f)*s, new Vector3(0.55f,h1+h2*0.55f+0.2f,d/2+0.02f)*s);
            // porta
            Cube(p, Mat(pal.dark), new Vector3(0.42f,0.72f,0.1f)*s, new Vector3(0,0.36f+0.2f,d/2+0.02f)*s);
            Cone(p, Mat(pal.wood), 0.24f*s,0.18f*s, new Vector3(0,0.72f+0.2f,d/2+0.02f)*s, default, 10);
            // chaminé
            Cyl(p, Mat(pal.stone), 0.13f*s, 0.35f*s, new Vector3(0.62f,h1+h2+0.75f,0)*s);
            Cyl(p, Mat(pal.stone*0.7f), 0.16f*s, 0.06f*s, new Vector3(0.62f,h1+h2+0.95f,0)*s); // capa da chaminé
            // escada na frente
            for(int i=0;i<3;i++) Cube(p,Mat(pal.stone),new Vector3(0.6f,0.07f*s,0.15f)*s,new Vector3(0,(0.07f+i*0.07f)*s,d/2+0.12f+(i*0.15f))*s);
        }

        /// <summary>Celeiro: corpo oval amplo, telhado em arco, portão duplo, trapeira.</summary>
        public static void Celeiro(Transform p, BuildingPal pal, float s = 1f)
        {
            float w=2.4f,d=1.8f,h=1.5f;
            // fundação
            Cube(p, Mat(pal.stone), new Vector3(w+0.2f,0.15f,d+0.2f)*s, new Vector3(0,0.075f,0)*s);
            // corpo
            Cube(p, Mat(pal.wall), new Vector3(w,h,d)*s, new Vector3(0,h/2+0.15f,0)*s);
            // vigas verticais
            float[] vx={-w/2f, w/2f, 0f};
            foreach(var vxv in vx) Cube(p,Mat(pal.wood),new Vector3(0.08f,h,0.1f)*s,new Vector3(vxv,h/2+0.15f,d/2)*s);
            // telhado de duas águas (simples e limpo)
            Cube(p, Mat(pal.roof), new Vector3(w+0.15f, 0.16f, d+0.15f)*s, new Vector3(0, h+0.22f, 0)*s);
            Cone(p, Mat(pal.roof), (w*0.6f)*s, 0.75f*s, new Vector3(0, h+0.3f, 0)*s, Quaternion.Euler(0,45,0), 4);
            // portão duplo
            Cube(p, Mat(pal.dark), new Vector3(0.55f,0.9f,0.1f)*s, new Vector3(-0.28f,0.45f+0.15f,d/2+0.02f)*s);
            Cube(p, Mat(pal.dark), new Vector3(0.55f,0.9f,0.1f)*s, new Vector3(0.28f,0.45f+0.15f,d/2+0.02f)*s);
            // trapeira
            Cube(p, Mat(pal.wall), new Vector3(0.5f,0.4f,0.5f)*s, new Vector3(0,h+1.2f,0)*s);
            Cone(p, Mat(pal.roof), 0.36f*s, 0.3f*s, new Vector3(0,h+1.6f,0)*s, Quaternion.Euler(0,45,0));
            // fardos de feno ao lado
            for(int i=0;i<2;i++){var hay=Prim.Cylinder(p,Mat(pal.accent*0.7f)); hay.transform.localScale=new Vector3(0.4f,0.3f,0.4f)*s; hay.transform.localPosition=new Vector3((i==0?w/2+0.3f:-w/2-0.3f)*s,0.15f*s,0.4f*s); hay.transform.localEulerAngles=new Vector3(90,0,0);}
        }

        /// <summary>Poço: base de pedra lavrada, arco de madeira, roldana, telhado.</summary>
        public static void Poco(Transform p, BuildingPal pal, float s = 1f)
        {
            // base circular de pedra (cilindro + detalhe)
            Cyl(p, Mat(pal.stone), 0.52f*s, 0.42f*s, new Vector3(0,0.42f,0)*s);
            Cyl(p, Mat(pal.stone*0.85f), 0.56f*s, 0.06f*s, new Vector3(0,0.78f,0)*s);
            Sph(p, Mat(pal.dark), 0.38f*s, new Vector3(0,0.44f,0)*s); // água escura
            // blocos de pedra na borda
            for(int i=0;i<6;i++){float a=i/6f*Mathf.PI*2; Cube(p,Mat(pal.stone),new Vector3(0.18f,0.08f,0.14f)*s,new Vector3(Mathf.Cos(a)*0.5f*s,0.82f*s,Mathf.Sin(a)*0.5f*s),new Vector3(0,a*Mathf.Rad2Deg,0));}
            // arco de madeira
            for(int i=0;i<2;i++) Cyl(p,Mat(pal.wood),0.055f*s,0.68f*s,new Vector3((i==0?-.42f:.42f)*s,1.18f*s,0));
            var bar=Prim.Cylinder(p,Mat(pal.wood)); bar.transform.localScale=new Vector3(0.055f,0.52f,0.055f)*s; bar.transform.localPosition=new Vector3(0,1.58f,0)*s; bar.transform.localEulerAngles=new Vector3(0,0,90);
            // roldana
            Cyl(p, Met(pal.accent*0.8f), 0.1f*s, 0.04f*s, new Vector3(0,1.58f,0)*s);
            // balde
            Cyl(p, Mat(pal.wood), 0.1f*s, 0.12f*s, new Vector3(0.42f,1.32f,0)*s);
            // telhado
            Cone(p, Mat(pal.roof), 0.58f*s, 0.42f*s, new Vector3(0,1.68f,0)*s, Quaternion.Euler(0,45,0));
            // pedras ao redor
            for(int i=0;i<4;i++){float a=i*90f*Mathf.Deg2Rad+22f; var st=Prim.Rock(p,Mat(pal.stone*0.9f),0.2f); st.transform.localScale=Vector3.one*0.22f*s; st.transform.localPosition=new Vector3(Mathf.Cos(a)*0.82f*s,0.12f*s,Mathf.Sin(a)*0.82f*s);}
        }

        /// <summary>Forno: cúpula grande com chaminé, lenhas, panelas, calçamento.</summary>
        public static void Forno(Transform p, BuildingPal pal, float s = 1f)
        {
            // calçamento de pedra
            Cyl(p, Mat(pal.stone), 0.9f*s, 0.08f*s, new Vector3(0,0.04f,0)*s);
            // cúpula
            var dome=Prim.Sphere(p,Mat(pal.stone)); dome.transform.localScale=new Vector3(1.4f,1.1f,1.4f)*s; dome.transform.localPosition=new Vector3(0,0.55f,0)*s;
            // arco de tijolo
            Cone(p, Mat(pal.accent*0.6f), 0.26f*s, 0.2f*s, new Vector3(0,0.26f,0.7f)*s, default, 12);
            Cube(p, Mat(pal.dark), new Vector3(0.4f,0.4f,0.12f)*s, new Vector3(0,0.2f,0.7f)*s);
            // chama interior visível
            Sph(p, Glow(pal.flame,1.4f), 0.2f*s, new Vector3(0,0.24f,0.48f)*s);
            // chaminé com capelo
            Cyl(p, Mat(pal.stone), 0.12f*s, 0.5f*s, new Vector3(0.4f,1.15f,0)*s);
            Cyl(p, Mat(pal.stone*0.7f), 0.16f*s, 0.06f*s, new Vector3(0.4f,1.45f,0)*s);
            // lenhas empilhadas
            for(int i=0;i<3;i++){var log=Prim.Cylinder(p,Mat(pal.wood)); log.transform.localScale=new Vector3(0.08f,0.4f,0.08f)*s; log.transform.localPosition=new Vector3(0.9f*s,0.08f*s,(i-1)*0.14f*s); log.transform.localEulerAngles=new Vector3(90,i*25f,0);}
            // panela de barro
            var pot=Prim.Sphere(p,Mat(pal.stone*0.8f)); pot.transform.localScale=new Vector3(0.22f,0.2f,0.22f)*s; pot.transform.localPosition=new Vector3(-0.72f*s,0.08f*s,0.2f*s);
        }

        // ================================================================
        //  ESTADO
        // ================================================================

        /// <summary>Muro: parede larga com passeio, 8 ameias, arco central, torres nos cantos.</summary>
        public static void Muro(Transform p, BuildingPal pal, float s = 1f)
        {
            float ww=4.5f,h=1.2f;
            // passeio
            Cube(p, Mat(pal.stone), new Vector3(ww,h,0.82f)*s, new Vector3(0,h/2,0)*s);
            // parapeito
            Cube(p, Mat(pal.stone*0.9f), new Vector3(ww,0.22f,0.72f)*s, new Vector3(0,h+0.11f,0)*s);
            // 8 ameias
            for(int i=0;i<8;i++){float x=(i-3.5f)*0.55f*s; Cube(p,Mat(pal.stone),new Vector3(0.38f,0.32f,0.72f)*s,new Vector3(x,h+0.27f,0)*s);}
            // arco central
            Cube(p, Mat(pal.dark), new Vector3(0.76f,h*0.85f,0.85f)*s, new Vector3(0,h*0.42f,0)*s);
            Cone(p, Mat(pal.stone), 0.4f*s, 0.3f*s, new Vector3(0,h*0.85f,0)*s, default, 12);
            // grades do portão
            for(int i=0;i<3;i++){Cyl(p,Met(pal.dark),0.03f*s,h*0.85f*s,new Vector3((i-1)*0.2f*s,h*0.42f,0));}
        }

        /// <summary>Obelisco: base dupla ornamentada, fuste piramidal, ponta áurea.</summary>
        public static void Obelisco(Transform p, BuildingPal pal, float s = 1f)
        {
            // plinto duplo
            Cube(p, Mat(pal.stone), new Vector3(1.2f,0.22f,1.2f)*s, new Vector3(0,0.11f,0)*s);
            Cube(p, Mat(pal.stone), new Vector3(0.95f,0.18f,0.95f)*s, new Vector3(0,0.29f,0)*s);
            // fuste cônico estreito (efeito pirâmide)
            for(int i=0;i<4;i++){float bw=0.5f-i*0.07f, by=0.47f+i*0.78f; Cube(p,Mat(pal.wall),new Vector3(bw,0.78f,bw)*s,new Vector3(0,by,0)*s);}
            // frisos horizontais (faixas escuras)
            for(int i=0;i<3;i++) Cube(p,Mat(pal.stone*0.75f),new Vector3(0.55f-i*0.06f,0.04f,0.55f-i*0.06f)*s,new Vector3(0,(0.85f+i*0.78f)*s,0));
            // ponta dourada
            Cone(p, Sat(pal.accent), 0.3f*s, 0.62f*s, new Vector3(0,3.6f,0)*s, Quaternion.Euler(0,45,0));
        }

        /// <summary>Torre procedural: altura, raio, andares e estilo variados por mundo.</summary>
        public static void Torre(Transform p, BuildingPal pal, float s = 1f)
        {
            float r1 = Random.Range(0.58f, 0.82f);  // raio do andar principal
            float h1 = Random.Range(1.6f, 2.8f);    // altura do andar principal
            float h2 = Random.Range(0.8f, 1.4f);    // altura do andar superior
            float r2 = r1 * Random.Range(0.78f, 0.92f);
            bool squarePlan = Random.value > 0.5f;  // cilindro ou prisma quadrado
            int slits = Random.Range(2, 5);

            // Convenção: todos os valores de posição são "unidades abstratas" multiplicados por s
            // via new Vector3(x, y, z) * s  →  nenhum componente deve ser pré-multiplicado por s.
            // Cylinder: localScale.y = h, altura total = 2h, extends pos.y ± h. Fundo em 0: pos.y = h.
            float bodyH  = h1 * 2f;          // altura total do corpo (abstract units)
            float totalH = bodyH + h2 * 2f;  // altura total da torre (abstract units)

            // base
            Cube(p, Mat(pal.stone), new Vector3(r1*2.2f, 0.24f, r1*2.2f)*s, new Vector3(0, 0.12f, 0)*s);
            // corpo principal — fundo em y=0
            if(squarePlan) Cube(p, Mat(pal.stone), new Vector3(r1*2f, bodyH, r1*2f)*s, new Vector3(0, bodyH*0.5f, 0)*s);
            else           Cyl(p, Mat(pal.stone), r1*s, h1*s, new Vector3(0, h1, 0)*s);
            // andar superior — fundo em bodyH
            Cyl(p, Mat(pal.wall), r2*s, h2*s, new Vector3(0, bodyH+h2, 0)*s);
            // corbels
            Cyl(p, Mat(pal.stone), (r2+0.06f)*s, 0.13f*s, new Vector3(0, totalH-0.19f, 0)*s);
            // ameias
            int merlon = Random.Range(6,10);
            for(int i=0;i<merlon;i++){float a=i/(float)merlon*Mathf.PI*2; Cube(p,Mat(pal.stone),Vector3.one*0.19f*s,new Vector3(Mathf.Cos(a)*(r2+0.1f), totalH, Mathf.Sin(a)*(r2+0.1f))*s);}
            // telhado
            bool pointy = Random.value > 0.3f;
            float roofH = pointy ? Random.Range(0.8f,1.4f) : 0.18f;
            if(pointy) Cone(p, Mat(pal.roof), (r2+0.12f)*s, roofH*s, new Vector3(0, totalH, 0)*s, default, Random.Range(8,12));
            else        Cyl(p, Mat(pal.roof*0.8f), (r2+0.06f)*s, 0.18f*s, new Vector3(0, totalH+0.09f, 0)*s);
            // arrow slits (~35% da altura do corpo)
            for(int i=0;i<slits;i++){float a=i/(float)slits*Mathf.PI*2; Cube(p,Mat(pal.dark),new Vector3(0.09f,0.33f,0.09f)*s,new Vector3(Mathf.Cos(a)*(r1-0.02f), bodyH*0.35f, Mathf.Sin(a)*(r1-0.02f))*s);}
            // porta
            Cube(p, Mat(pal.dark), new Vector3(0.40f,0.70f,0.12f)*s, new Vector3(0, 0.35f, r1+0.02f)*s);
            Cone(p, Mat(pal.stone), 0.22f*s, 0.2f*s, new Vector3(0, 0.7f, r1+0.02f)*s, default, 10);
            // bandeira
            float flagBase = totalH + roofH;
            Cyl(p, Mat(pal.wood), 0.038f*s, 0.8f*s, new Vector3(0, flagBase+0.4f, 0)*s);
            Cube(p, Sat(pal.flame), new Vector3(0.34f,0.21f,0.02f)*s, new Vector3(0.19f, flagBase+1.0f, 0)*s);
        }

        // ================================================================
        //  MEDIEVAL
        // ================================================================

        /// <summary>Castelo procedural: dimensões, torres, ameias e estilo variados por mundo.</summary>
        public static void Castelo(Transform p, BuildingPal pal, float s = 1f)
        {
            // ── Parâmetros procedurais (variam por semente do mundo) ──────────
            float kW   = Random.Range(2.8f, 4.4f);   // largura do manter
            float kH   = Random.Range(2.4f, 3.8f);   // altura do manter
            float kD   = Random.Range(2.4f, 3.4f);   // profundidade do manter
            int tCount = Random.Range(0,3)==0 ? 2 : 4; // 2 ou 4 torres
            float tR   = Random.Range(0.45f, 0.68f); // raio das torres
            float tH   = Random.Range(1.6f, 2.4f);   // altura das torres
            int merlon = Random.Range(5, 10);         // nº de ameias
            bool bigGate = Random.value > 0.4f;       // torre-porta grande
            Color roofCol = Random.value > 0.5f ? pal.roof : Color.Lerp(pal.roof, pal.accent, 0.35f);

            // ── Manter central ───────────────────────────────────────────────
            Cube(p, Mat(pal.stone), new Vector3(kW, kH, kD)*s, new Vector3(0, kH*0.5f, 0)*s);
            // janelas do manter (2-3)
            int wins = Random.Range(2,4);
            for(int i=0;i<wins;i++){
                float wx = (i-(wins-1)*0.5f) * (kW/(wins+0.5f)) * s;
                Cube(p,Sat(pal.accent),new Vector3(0.25f,0.4f,0.1f)*s,new Vector3(wx,(kH*0.72f)*s,(kD*0.51f)*s));
                Cube(p,Mat(pal.dark),new Vector3(0.16f,0.28f,0.12f)*s,new Vector3(wx,(kH*0.72f)*s,(kD*0.51f)*s));
            }
            // ameias do manter
            for(int i=0;i<merlon;i++){
                float x=(i-(merlon-1)*0.5f)*(kW*s/(merlon-0.5f));
                Cube(p,Mat(pal.stone),new Vector3(0.28f,0.3f,0.65f)*s,new Vector3(x,(kH+0.1f)*s,0));
            }
            // ── Torres ───────────────────────────────────────────────────────
            // posições dos cantos segundo o número de torres
            float hx = kW*0.5f+0.1f, hz = kD*0.5f+0.1f;
            float[][] corners = tCount==2
                ? new[]{new[]{-hx,-hz}, new[]{hx,hz}}
                : new[]{new[]{-hx,-hz}, new[]{hx,-hz}, new[]{-hx,hz}, new[]{hx,hz}};
            foreach(var c2 in corners)
            {
                // c2[0]/c2[1] são unidades abstratas — usar new Vector3(...)*s para escalar
                float cx2=c2[0], cz2=c2[1];
                // torre: fundo em y=0 (pos.y = tH = localScale.y do Cyl)
                Cyl(p, Mat(pal.stone), tR*s, tH*s, new Vector3(cx2, tH, cz2)*s);
                // ameias da torre no topo (tH*2)
                float tTop = tH*2f;
                for(int k=0;k<6;k++){float a=k/6f*Mathf.PI*2; Cube(p,Mat(pal.stone),Vector3.one*0.17f*s,new Vector3(cx2+Mathf.Cos(a)*(tR+0.08f), tTop, cz2+Mathf.Sin(a)*(tR+0.08f))*s);}
                Cone(p,Mat(roofCol),(tR+0.14f)*s,0.85f*s,new Vector3(cx2, tTop, cz2)*s,default,8);
                // bandeira
                Cyl(p,Mat(pal.wood),0.035f*s,0.72f*s,new Vector3(cx2, tTop+0.85f, cz2)*s);
                Cube(p,Sat(pal.flame),new Vector3(0.30f,0.19f,0.02f)*s,new Vector3(cx2+0.16f, tTop+1.32f, cz2)*s);
            }
            // ── Portão ───────────────────────────────────────────────────────
            float gH = bigGate ? kH*0.72f : kH*0.55f;
            float gW = bigGate ? 1.3f : 1.0f;
            Cube(p, Mat(pal.stone), new Vector3(gW, gH, 0.85f)*s, new Vector3(0, gH*0.5f, (kD*0.5f+0.4f))*s);
            Cube(p, Mat(pal.dark), new Vector3(gW*0.58f, gH*0.78f, 0.9f)*s, new Vector3(0, gH*0.4f, (kD*0.5f+0.4f))*s);
            Cone(p, Mat(pal.stone), gW*0.56f*s, 0.48f*s, new Vector3(0, gH*s, (kD*0.5f+0.4f)*s), default, 10);
            for(int i=0;i<3;i++) Cyl(p,Met(pal.dark),0.03f*s,gH*0.78f*s,new Vector3((i-1)*0.22f*s,gH*0.4f*s,(kD*0.5f+0.4f)*s));
        }

        /// <summary>Muralha defensiva: longa com torres e arco.</summary>
        public static void Muralha(Transform p, BuildingPal pal, float s = 1f)
        {
            Cube(p, Mat(pal.stone), new Vector3(5.6f,1.85f,0.82f)*s, new Vector3(0,0.92f,0)*s);
            for(int i=0;i<10;i++){float x=(i-4.5f)*0.55f*s; Cube(p,Mat(pal.stone),new Vector3(0.34f,0.3f,0.82f)*s,new Vector3(x,1.85f+0.15f,0)*s);}
            for(int i=0;i<2;i++){float x=(i==0?-2.9f:2.9f)*s; Cyl(p,Mat(pal.stone),0.58f*s,1.4f*s,new Vector3(x,1.4f*s,0)); Cone(p,Mat(pal.roof),0.68f*s,0.7f*s,new Vector3(x,2.8f*s,0),default,8);}
            Cube(p, Mat(pal.dark), new Vector3(0.8f,1.38f,0.85f)*s, new Vector3(0,0.69f,0)*s);
            Cone(p, Mat(pal.stone), 0.45f*s, 0.35f*s, new Vector3(0,1.42f,0)*s, default, 12);
        }

        /// <summary>Igreja: nave + abside + torre sineira + rosácea + vitrais + cruz.</summary>
        public static void Igreja(Transform p, BuildingPal pal, float s = 1f)
        {
            // nave
            Cube(p, Mat(pal.wall), new Vector3(2.5f,2.1f,4.0f)*s, new Vector3(0,1.05f,0)*s);
            Cone(p, Mat(pal.roof), 1.4f*s, 1.05f*s, new Vector3(0,2.1f,0)*s, default, 4);
            // abside (ábside semicircular na traseira)
            Cyl(p, Mat(pal.wall), 0.9f*s, 1.8f*s, new Vector3(0,0.9f,-2.2f)*s);
            Cone(p, Mat(pal.roof), 1.0f*s, 0.7f*s, new Vector3(0,1.8f,-2.2f)*s, default, 6);
            // vitrais laterais
            for(int i=0;i<3;i++){float z=(i-1)*1.2f*s;
                Cube(p,Sat(H("#7BB8E0")),new Vector3(0.15f,0.45f,0.1f)*s,new Vector3(1.26f*s,1.5f*s,z)); // lado esquerdo
                Cube(p,Sat(H("#7BB8E0")),new Vector3(0.15f,0.45f,0.1f)*s,new Vector3(-1.26f*s,1.5f*s,z));
            }
            // torre sineira
            Cube(p, Mat(pal.stone), new Vector3(1.1f,3.8f,1.1f)*s, new Vector3(-0.95f,1.9f,-1.6f)*s);
            Cone(p, Mat(pal.roof), 0.65f*s, 0.65f*s, new Vector3(-0.95f,3.8f,-1.6f)*s, default, 4);
            // sineta
            Cyl(p, Met(pal.accent*0.7f), 0.2f*s, 0.25f*s, new Vector3(-0.95f,3.2f,-1.6f)*s);
            // cruz
            Cyl(p, Sat(pal.accent), 0.04f*s, 0.42f*s, new Vector3(-0.95f,4.5f,-1.6f)*s);
            var crossBar=Prim.Cylinder(p,Sat(pal.accent)); crossBar.transform.localScale=new Vector3(0.04f,0.24f,0.04f)*s; crossBar.transform.localPosition=new Vector3(-0.95f,4.65f,-1.6f)*s; crossBar.transform.localEulerAngles=new Vector3(0,0,90);
            // rosácea
            Sph(p, Sat(H("#7BB8E0")), 0.28f*s, new Vector3(0,1.8f,2.02f)*s);
            // portal
            Cube(p, Mat(pal.dark), new Vector3(0.46f,0.88f,0.12f)*s, new Vector3(0,0.44f,2.02f)*s);
            Cone(p, Mat(pal.stone), 0.26f*s, 0.28f*s, new Vector3(0,0.88f,2.02f)*s, default, 12);
            // escadaria
            for(int i=0;i<4;i++) Cube(p,Mat(pal.stone),new Vector3(0.8f,0.08f*s,0.18f)*s,new Vector3(0,(0.08f+i*0.08f)*s,2.18f+(i*0.18f))*s);
        }

        /// <summary>Moinho: torre cônica com cap, 4 velas largas, mecanismo superior visível.</summary>
        public static void Moinho(Transform p, BuildingPal pal, float s = 1f)
        {
            // base circular de pedra
            Cyl(p, Mat(pal.stone*1.1f), 0.9f*s, 0.22f*s, new Vector3(0,0.11f,0)*s);
            // torre principal cônica
            Prim.Cone(p, Mat(pal.stone), 0.86f*s, 4.0f*s, 14).transform.localPosition=Vector3.zero;
            // bandas decorativas de tijolo
            for(int i=0;i<3;i++) Cyl(p,Mat(pal.accent*0.65f),0.86f-i*0.2f<0.4f?0.4f:(0.86f-i*0.2f),0.1f,new Vector3(0,(0.8f+i*1.1f)*s,0));
            // cap (domo de madeira)
            var cap=Prim.Sphere(p,Mat(pal.roof)); cap.transform.localScale=new Vector3(1.0f,0.65f,1.0f)*s; cap.transform.localPosition=new Vector3(0,4.05f,0)*s;
            // eixo do mecanismo
            Cyl(p, Met(pal.stone), 0.08f*s, 0.55f*s, new Vector3(0,3.6f,0.92f)*s);
            // 4 velas com estrutura realista
            for(int i=0;i<4;i++){
                float a=i*Mathf.PI*0.5f;
                float cx2=Mathf.Cos(a)*1.22f,cz2=Mathf.Sin(a)*1.22f;
                // mastro da vela
                var arm=Prim.Cylinder(p,Mat(pal.wood)); arm.transform.localScale=new Vector3(0.06f,1.15f,0.06f)*s; arm.transform.localPosition=new Vector3(cx2,3.4f,cz2)*s; arm.transform.localEulerAngles=new Vector3(90,a*Mathf.Rad2Deg,0);
                // pano da vela
                Cube(p,Mat(pal.wall),new Vector3(0.7f,0.06f,0.52f)*s,new Vector3(cx2,3.4f,cz2)*s,new Vector3(0,a*Mathf.Rad2Deg,0));
            }
            // porta
            Cube(p, Mat(pal.dark), new Vector3(0.38f,0.62f,0.12f)*s, new Vector3(0,0.31f,0.88f)*s);
            // escada ao lado
            for(int i=0;i<3;i++) Cube(p,Mat(pal.stone),new Vector3(0.5f,0.07f,0.18f)*s,new Vector3(0,(0.07f+i*0.07f)*s,0.98f+(i*0.18f))*s);
        }

        // ================================================================
        //  CULTO
        // ================================================================

        /// <summary>Altar: plataforma tripla, pilares de chama, esfera sagrada, baixo relevo.</summary>
        public static void Altar(Transform p, BuildingPal pal, float s = 1f)
        {
            // 3 degraus de pedra
            for(int i=0;i<3;i++){float bw=2.2f-i*0.38f,bd=1.5f-i*0.28f; Cube(p,Mat(pal.stone),new Vector3(bw,0.24f,bd)*s,new Vector3(0,(0.12f+i*0.24f)*s,0));}
            // pilares laterais de chama
            for(int i=0;i<2;i++){float x=(i==0?-0.65f:0.65f)*s; Cyl(p,Mat(pal.stone),0.12f*s,0.7f*s,new Vector3(x,0.82f*s,0)); Cone(p,Glow(pal.flame,1.4f),0.16f*s,0.42f*s,new Vector3(x,1.52f*s,0),default,8);}
            // esfera sagrada no centro
            Sph(p, Glow(pal.flame,1.5f), 0.28f*s, new Vector3(0,0.98f*s,0));
            // placa de baixo relevo
            Cube(p, Mat(pal.stone*0.85f), new Vector3(0.9f,0.5f,0.06f)*s, new Vector3(0,0.5f*s,0.76f*s));
            // símbolo no centro da placa
            Sph(p, Sat(pal.accent), 0.08f*s, new Vector3(0,0.52f*s,0.82f*s));
        }

        /// <summary>Templo grego: base escalonada, 8 colunas, friso triglífico, frontão com acrotério.</summary>
        public static void Templo(Transform p, BuildingPal pal, float s = 1f)
        {
            // estilóbata (3 degraus)
            for(int i=0;i<3;i++){float bw=3.8f-i*0.44f,bd=2.4f-i*0.3f; Cube(p,Mat(pal.stone),new Vector3(bw,0.25f,bd)*s,new Vector3(0,(0.125f+i*0.25f)*s,0));}
            float colH=1.55f, colBase=0.82f*s;
            // colunas (5 frente + 5 atrás) com capitel
            for(int i=0;i<5;i++){float x=(i-2)*0.7f*s;
                Cyl(p,Mat(pal.wall),0.16f*s,colH*s,new Vector3(x,colBase+colH*0.5f*s,1.05f*s)); Cyl(p,Mat(pal.stone),0.2f*s,0.1f*s,new Vector3(x,colBase+colH*s+0.05f*s,1.05f*s));
                Cyl(p,Mat(pal.wall),0.16f*s,colH*s,new Vector3(x,colBase+colH*0.5f*s,-1.05f*s)); Cyl(p,Mat(pal.stone),0.2f*s,0.1f*s,new Vector3(x,colBase+colH*s+0.05f*s,-1.05f*s));}
            // entablamento (arquitrave + friso)
            Cube(p, Mat(pal.stone), new Vector3(3.6f,0.25f,2.4f)*s, new Vector3(0,colBase+colH+0.25f*s*0.5f,0)*s);
            // friso (triglífos alternados)
            for(int i=0;i<5;i++){float x=(i-2)*0.68f*s; Cube(p,Mat(pal.stone*0.8f),new Vector3(0.24f,0.2f,0.05f)*s,new Vector3(x,colBase+colH+0.3f*s,1.21f*s));}
            // frontão
            Cone(p, Mat(pal.roof), 2.0f*s, 0.88f*s, new Vector3(0,colBase+colH+0.5f,0)*s, default, 4);
            // acrotério
            for(int i=0;i<2;i++){float x=(i==0?-1.88f:1.88f)*s; Cone(p,Sat(pal.accent),0.18f*s,0.32f*s,new Vector3(x,colBase+colH+0.88f+0.16f,0)*s,default,4);}
            Cone(p, Sat(pal.accent), 0.2f*s, 0.38f*s, new Vector3(0,colBase+colH+0.88f+0.19f,0)*s, default, 4);
            // chama central
            Sph(p, Glow(pal.accent,1.2f), 0.22f*s, new Vector3(0,colBase+colH+0.2f,0)*s);
        }

        /// <summary>Menir/trílito: 3 monólitos + lintel + inscrições luminosas.</summary>
        public static void Menir(Transform p, BuildingPal pal, float s = 1f)
        {
            // monólitos verticais (2 + 1 central)
            for(int i=0;i<2;i++){float x=(i==0?-.82f:.82f)*s; var stone=Prim.Rock(p,Mat(pal.stone),0.38f); stone.transform.localScale=new Vector3(0.62f,2.6f,0.62f)*s; stone.transform.localPosition=new Vector3(x,1.3f*s,0);}
            var center2=Prim.Rock(p,Mat(pal.stone),0.3f); center2.transform.localScale=new Vector3(0.48f,2.2f,0.48f)*s; center2.transform.localPosition=new Vector3(0,1.1f*s,0.6f*s);
            // lintel
            var lintel=Prim.Rock(p,Mat(pal.stone*0.9f),0.22f); lintel.transform.localScale=new Vector3(2.0f,0.5f,0.62f)*s; lintel.transform.localPosition=new Vector3(0,2.68f*s,0);
            // inscrições brilhantes
            for(int i=0;i<3;i++){float a=i*40f*Mathf.Deg2Rad; Sph(p,Glow(pal.accent,0.9f),0.1f*s,new Vector3(Mathf.Cos(a)*0.35f*s,1.4f*s,0.32f*s));}
            // círculo externo de pedras menores
            for(int i=0;i<6;i++){float a=i/6f*Mathf.PI*2; var sm=Prim.Rock(p,Mat(pal.stone*0.85f),0.25f); sm.transform.localScale=new Vector3(0.28f,0.9f,0.28f)*s; sm.transform.localPosition=new Vector3(Mathf.Cos(a)*1.75f*s,0.45f*s,Mathf.Sin(a)*1.75f*s);}
        }

        /// <summary>Santuário: templo pequeno ornamentado com chama interior visível, jardim.</summary>
        public static void Santuario(Transform p, BuildingPal pal, float s = 1f)
        {
            // degraus
            Cube(p, Mat(pal.stone), new Vector3(2.1f,0.18f,2.1f)*s, new Vector3(0,0.09f,0)*s);
            Cube(p, Mat(pal.stone), new Vector3(1.8f,0.16f,1.8f)*s, new Vector3(0,0.26f,0)*s);
            // naos (cella)
            Cube(p, Mat(pal.wall), new Vector3(1.7f,1.7f,1.7f)*s, new Vector3(0,1.17f,0)*s);
            // 4 colunas na frente
            for(int i=0;i<4;i++){float x=(i-1.5f)*0.52f*s; Cyl(p,Mat(pal.wall),0.13f*s,1.12f*s,new Vector3(x,0.98f*s,0.9f*s));}
            Cube(p, Mat(pal.stone), new Vector3(1.8f,0.14f,1.8f)*s, new Vector3(0,2.12f,0)*s); // epistílio
            Cone(p, Mat(pal.roof), 1.05f*s, 0.65f*s, new Vector3(0,2.26f,0)*s, Quaternion.Euler(0,45,0));
            // chama interior
            Sph(p, Glow(pal.flame,1.6f), 0.22f*s, new Vector3(0,1.1f*s,0.2f*s));
            // jardim (arbustos ao redor)
            for(int i=0;i<4;i++){float a=i*90f*Mathf.Deg2Rad+45f; var bush=Prim.Sphere(p,Prim.Mat(H("#3C6E4A"))); bush.transform.localScale=Vector3.one*0.38f*s; bush.transform.localPosition=new Vector3(Mathf.Cos(a)*1.4f*s,0.28f*s,Mathf.Sin(a)*1.4f*s);}
        }

        // ================================================================
        //  COSTA
        // ================================================================

        /// <summary>Doca: pier longo, pilares, galpão, barco atracado, mastro.</summary>
        public static void Doca(Transform p, BuildingPal pal, float s = 1f)
        {
            // deck do pier
            Cube(p, Mat(pal.wood), new Vector3(4.2f,0.18f,1.1f)*s, Vector3.zero);
            // suportes/pilares
            for(int i=0;i<6;i++){float x=(i-2.5f)*0.82f*s; Cyl(p,Mat(pal.wood),0.08f*s,0.52f*s,new Vector3(x,-0.5f*s,0)); for(int j=0;j<2;j++) Cyl(p,Mat(pal.wood),0.04f*s,0.12f*s,new Vector3(x,-0.1f*s,(j==0?-0.4f:0.4f)*s));}
            // bollards (cabeços de amarração)
            for(int i=0;i<3;i++){float x=(i-1)*1.4f*s; Cyl(p,Mat(pal.stone),0.1f*s,0.22f*s,new Vector3(x,0.2f*s,0.46f*s));}
            // galpão no fim
            Cube(p, Mat(pal.wall), new Vector3(1.2f,1.1f,1.0f)*s, new Vector3(-1.8f*s,0.64f*s,0));
            Cone(p, Mat(pal.roof), 0.78f*s, 0.5f*s, new Vector3(-1.8f*s,1.19f*s,0), Quaternion.Euler(0,45,0));
            // barco atracado
            var hull=Prim.Sphere(p,Mat(pal.wood)); hull.transform.localScale=new Vector3(0.82f,0.35f,1.7f)*s; hull.transform.localPosition=new Vector3(1.8f*s,-0.15f*s,0);
            Cyl(p,Mat(pal.wood),0.04f*s,0.8f*s,new Vector3(1.8f*s,0.38f*s,0));
            var sail=Prim.Cone(p,Mat(pal.wall),0.55f*s,0.9f*s,4); sail.transform.localPosition=new Vector3(1.8f*s,0.0f*s,0.12f*s); sail.transform.localRotation=Quaternion.Euler(0,45,90);
        }

        /// <summary>Farol: torre alta com bandas de cor, cabine de luz emissiva, escada helicoidal.</summary>
        public static void Farol(Transform p, BuildingPal pal, float s = 1f)
        {
            // base
            Cyl(p, Mat(pal.stone), 0.72f*s, 0.28f*s, new Vector3(0,0.14f,0)*s);
            // torre cônica
            Prim.Cone(p, Mat(pal.wall), 0.62f*s, 4.5f*s, 10).transform.localPosition=Vector3.zero;
            // bandas decorativas (escuro/claro alternado)
            for(int i=0;i<4;i++){float y=0.7f+i*0.95f,r=0.62f-i*0.12f; r=Mathf.Max(r,0.26f); Cyl(p,Mat(i%2==0?pal.roof:pal.wall),r*s,0.08f*s,new Vector3(0,y*s,0));}
            // varanda antes da cabine
            Cyl(p, Mat(pal.stone), 0.72f*s, 0.1f*s, new Vector3(0,4.48f*s,0));
            // cabine da luz
            Cyl(p, Mat(pal.wall), 0.5f*s, 0.62f*s, new Vector3(0,4.83f*s,0));
            // janelas da cabine (8 faces)
            for(int i=0;i<8;i++){float a=i/8f*Mathf.PI*2; Cube(p,Glow(pal.accent,2.2f),new Vector3(0.1f,0.38f,0.1f)*s,new Vector3(Mathf.Cos(a)*0.5f*s,4.83f*s,Mathf.Sin(a)*0.5f*s));}
            // cúpula
            Cone(p, Mat(pal.roof), 0.55f*s, 0.52f*s, new Vector3(0,5.45f*s,0), default, 10);
            // ponta
            Cyl(p, Met(pal.stone), 0.04f*s, 0.35f*s, new Vector3(0,5.98f*s,0));
            // porta
            Cube(p, Mat(pal.dark), new Vector3(0.34f,0.58f,0.1f)*s, new Vector3(0,0.29f,0.62f)*s);
        }

        /// <summary>Barco: casco largo, 2 mastros, velas cheias, bandeira, âncora.</summary>
        public static void Barco(Transform p, BuildingPal pal, float s = 1f)
        {
            // casco
            var hull=Prim.Sphere(p,Mat(pal.wood)); hull.transform.localScale=new Vector3(1.7f,0.68f,3.2f)*s; hull.transform.localPosition=new Vector3(0,0.34f,0)*s;
            // convés
            Cube(p, Mat(pal.wood*1.1f), new Vector3(1.5f,0.1f,2.8f)*s, new Vector3(0,0.62f,0)*s);
            // mastro principal
            Cyl(p, Mat(pal.wood), 0.06f*s, 2.0f*s, new Vector3(0,1.72f,0)*s);
            // vela principal
            var sail1=Prim.Cone(p,Mat(pal.wall),0.82f*s,1.5f*s,4); sail1.transform.localPosition=new Vector3(0,0.82f,0.15f)*s; sail1.transform.localRotation=Quaternion.Euler(0,45,90);
            // mastro de mezena
            Cyl(p, Mat(pal.wood), 0.05f*s, 1.5f*s, new Vector3(0,1.45f,-1.1f)*s);
            // vela de mezena
            var sail2=Prim.Cone(p,Mat(pal.wall),0.6f*s,1.1f*s,4); sail2.transform.localPosition=new Vector3(0,0.8f,-1.1f)*s; sail2.transform.localRotation=Quaternion.Euler(0,45,90);
            // crow's nest
            Cyl(p, Mat(pal.wood), 0.25f*s, 0.1f*s, new Vector3(0,2.65f,0)*s);
            // bandeira
            Cube(p, Sat(pal.accent), new Vector3(0.3f,0.22f,0.02f)*s, new Vector3(0.18f,2.92f,0)*s);
            // âncora
            var anc=Prim.Cube(p,Met(pal.dark)); anc.transform.localScale=new Vector3(0.08f,0.38f,0.08f)*s; anc.transform.localPosition=new Vector3(0.65f,0.48f,1.52f)*s;
        }

        // ================================================================
        //  DISPATCH
        // ================================================================
        public static void BuildByKey(string key, Transform p, BuildingPal pal, float s = 1f)
        {
            switch (key)
            {
                case "fogueira":     Fogueira(p, pal, s);     break;
                case "tenda":        Tenda(p, pal, s);        break;
                case "choca":        Choca(p, pal, s);        break;
                case "totem":        Totem(p, pal, s);        break;
                case "cerca":        Cerca(p, pal, s);        break;
                case "casa_madeira": CasaMadeira(p, pal, s);  break;
                case "celeiro":      Celeiro(p, pal, s);      break;
                case "poco":         Poco(p, pal, s);         break;
                case "forno":        Forno(p, pal, s);        break;
                case "muro":         Muro(p, pal, s);         break;
                case "obelisco":     Obelisco(p, pal, s);     break;
                case "torre":        Torre(p, pal, s);        break;
                case "castelo":      Castelo(p, pal, s);      break;
                case "muralha":      Muralha(p, pal, s);      break;
                case "igreja":       Igreja(p, pal, s);       break;
                case "moinho":       Moinho(p, pal, s);       break;
                case "altar":        Altar(p, pal, s);        break;
                case "templo":       Templo(p, pal, s);       break;
                case "menir":        Menir(p, pal, s);        break;
                case "santuario":    Santuario(p, pal, s);    break;
                case "doca":         Doca(p, pal, s);         break;
                case "farol":        Farol(p, pal, s);        break;
                case "barco":        Barco(p, pal, s);        break;
            }
        }
    }
}
