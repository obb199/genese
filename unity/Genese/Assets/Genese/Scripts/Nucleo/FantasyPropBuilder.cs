using UnityEngine;

namespace Genese.Nucleo
{
    /// <summary>
    /// Constrói props ricos e detalhados para os 10 biomas fantásticos do Claude Design
    /// (biomes-fantasy.js). Cada bioma tem 4 variantes distintas que combinam
    /// múltiplas primitivas com materiais e efeitos específicos do tema.
    /// </summary>
    public static class FantasyPropBuilder
    {
        static Color H(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }
        static Material Mat(Color c)          => Prim.Mat(c, Prim.Finish.Matte);
        static Material Sat(Color c)          => Prim.Mat(c, Prim.Finish.Satin);
        static Material Met(Color c)          => Prim.Mat(c, Prim.Finish.Metallic);
        static Material Iri(Color c)          => Prim.Mat(c, Prim.Finish.Iridescent);
        static Material Glow(Color c, float i=1.4f) => Prim.Mat(c, Prim.Finish.Satin, c, i);

        static GameObject Cyl(Transform p, Material m, float r, float h, Vector3 pos)
        { var g=Prim.Cylinder(p,m); g.transform.localScale=new Vector3(r*2,h,r*2); g.transform.localPosition=pos; return g; }
        static GameObject Cube(Transform p, Material m, Vector3 sz, Vector3 pos, Vector3 rot=default)
        { var g=Prim.Cube(p,m); g.transform.localScale=sz; g.transform.localPosition=pos; if(rot!=default) g.transform.localEulerAngles=rot; return g; }
        static GameObject Cone(Transform p, Material m, float r, float h, Vector3 pos, Quaternion rot=default, int seg=4)
        { var g=Prim.Cone(p,m,r,h,seg); g.transform.localPosition=pos; if(rot!=default) g.transform.localRotation=rot; return g; }
        static GameObject Sph(Transform p, Material m, float r, Vector3 pos)
        { var g=Prim.Sphere(p,m); g.transform.localScale=Vector3.one*r*2; g.transform.localPosition=pos; return g; }
        static void Light(Transform p, Color col, float range, float intensity, Vector3 pos)
        { var go=new GameObject("l").transform; go.SetParent(p,false); go.localPosition=pos; var l=go.gameObject.AddComponent<UnityEngine.Light>(); l.type=LightType.Point; l.color=col; l.range=range; l.intensity=intensity; }

        // ================================================================
        //  ENTRY POINT
        // ================================================================
        public static void Build(int theme, Transform parent,
            Color foliage, Color foliage2, Color rock, Color accent, Color trunk, float s)
        {
            int variant = Random.Range(0, 4);
            switch (theme)
            {
                case 1:  Cogumelos(variant, parent, foliage, trunk, accent, s);       break;
                case 2:  Cristais(variant, parent, foliage, foliage2, accent, s);     break;
                case 3:  Mecanico(variant, parent, trunk, foliage, accent, s);        break;
                case 4:  Nuvens(variant, parent, foliage, foliage2, accent, s);       break;
                case 5:  OceanoVivo(variant, parent, rock, foliage, accent, s);       break;
                case 6:  Bioluminescente(variant, parent, foliage, foliage2, accent, s); break;
                case 7:  Neural(variant, parent, foliage, foliage2, accent, s);       break;
                case 8:  Vidro(variant, parent, foliage, foliage2, accent, s);        break;
                case 9:  Fractal(variant, parent, foliage, foliage2, accent, s);      break;
                case 10: Silicio(variant, parent, trunk, foliage, accent, s);         break;
            }
        }

        // ================================================================
        //  1 — FLORESTA DE COGUMELOS GIGANTES
        // ================================================================
        static void Cogumelos(int v, Transform p, Color col, Color stem, Color accent, float s)
        {
            switch (v)
            {
                case 0: // cogumelo único com spots (simplificado)
                {
                    Cyl(p, Mat(stem), 0.2f*s, 1.0f*s, new Vector3(0, 1.0f, 0)*s);
                    var cap=Prim.Sphere(p,Mat(col)); cap.transform.localScale=new Vector3(1.55f,0.65f,1.55f)*s; cap.transform.localPosition=new Vector3(0,2.1f,0)*s;
                    for(int i=0;i<5;i++){float a=i/5f*Mathf.PI*2,rr=Random.Range(0.3f,0.75f); Sph(p,Glow(accent,1.5f),0.09f*s,new Vector3(Mathf.Cos(a)*rr*s,2.2f*s,Mathf.Sin(a)*rr*s));}
                    Light(p, accent, 5*s, 1.6f, new Vector3(0, 2.2f, 0)*s);
                    break;
                }
                case 1: // micélio simples (5 braços)
                {
                    var center=Sph(p,Glow(accent,1.6f),0.2f*s,new Vector3(0,0.2f,0)*s);
                    for(int i=0;i<5;i++){float a=i/5f*Mathf.PI*2,len=Random.Range(0.8f,1.4f)*s;
                        var branch=Cyl(p,Mat(accent*0.8f),0.04f*s,len/2,new Vector3(Mathf.Cos(a)*len*0.5f,0.1f*s,Mathf.Sin(a)*len*0.5f)); branch.transform.LookAt(center.transform); branch.transform.Rotate(90,0,0);
                        Sph(p,Glow(accent,1.2f),0.12f*s,new Vector3(Mathf.Cos(a)*len,0.12f*s,Mathf.Sin(a)*len));}
                    Light(p, accent, 4*s, 1.2f, new Vector3(0,0.4f,0)*s);
                    break;
                }
                case 2: // trio de cogumelos
                {
                    for(int i=0;i<3;i++){float a=i/3f*Mathf.PI*2,ms=s*(0.55f+Random.value*0.4f),rr=0.85f*s;
                        Cyl(p,Mat(stem),0.1f*ms,0.65f*ms,new Vector3(Mathf.Cos(a)*rr,0.65f*ms,Mathf.Sin(a)*rr));
                        var mc=Prim.Sphere(p,Mat(col)); mc.transform.localScale=new Vector3(0.75f,0.38f,0.75f)*ms; mc.transform.localPosition=new Vector3(Mathf.Cos(a)*rr,1.35f*ms,Mathf.Sin(a)*rr);
                        for(int k=0;k<2;k++) Sph(p,Glow(accent,1.4f),0.07f*s,new Vector3(Mathf.Cos(a)*rr+Random.Range(-0.3f,0.3f)*s,Random.Range(0.5f,1.8f)*s,Mathf.Sin(a)*rr+Random.Range(-0.3f,0.3f)*s));}
                    Light(p,accent,5*s,1.6f,new Vector3(0,1.2f,0)*s);
                    break;
                }
                default: // coluna simples
                {
                    Cyl(p,Mat(stem*0.85f),0.15f*s,1.7f*s,new Vector3(0,1.7f,0)*s);
                    for(int i=0;i<3;i++){float y=(0.7f+i*0.5f)*s,r=(0.28f+i*0.08f)*s;
                        for(int k=0;k<5;k++){float a=k/5f*Mathf.PI*2; Sph(p,Glow(accent,1.3f),0.07f*s,new Vector3(Mathf.Cos(a)*r,y,Mathf.Sin(a)*r));}}
                    var tc=Prim.Sphere(p,Mat(col)); tc.transform.localScale=new Vector3(2.2f,1.0f,2.2f)*s; tc.transform.localPosition=new Vector3(0,3.5f,0)*s;
                    Light(p,accent,6*s,1.8f,new Vector3(0,2.8f,0)*s);
                    break;
                }
            }
        }

        // ================================================================
        //  2 — MAR DE CRISTAIS
        // ================================================================
        static void Cristais(int v, Transform p, Color col, Color col2, Color accent, float s)
        {
            switch (v)
            {
                case 0: // formação principal com múltiplos cristais
                {
                    Color[] cc={col,col2,accent,col,col2};
                    for(int i=0;i<5;i++){float a=i/5f*Mathf.PI*2+(float)i*0.3f,h=Random.Range(0.8f,2.2f)*s,tilt=Random.Range(-15,15);
                        var c=Cone(p,Met(cc[i%cc.Length]),0.18f*s,h,new Vector3(Mathf.Cos(a)*0.35f*s,0,Mathf.Sin(a)*0.35f*s),Quaternion.Euler(tilt,a*Mathf.Rad2Deg,0));
                        Cone(p,Met(cc[(i+2)%cc.Length]),0.12f*s,h*0.35f,new Vector3(Mathf.Cos(a)*0.35f*s,h,Mathf.Sin(a)*0.35f*s),Quaternion.Euler(180+tilt,a*Mathf.Rad2Deg,0));}
                    // base plana de cristal
                    Cube(p,Iri(col),new Vector3(1.1f,0.12f,1.1f)*s,new Vector3(0,0.06f,0)*s);
                    // gema central
                    Sph(p,Glow(accent,1.8f),0.25f*s,new Vector3(0,0.3f,0)*s);
                    Light(p,accent,5*s,1.6f,new Vector3(0,1f,0)*s);
                    break;
                }
                case 1: // planalto de cristal
                {
                    // base plana grande
                    Cube(p,Iri(col),new Vector3(1.8f,0.25f,1.8f)*s,new Vector3(0,0.12f,0)*s);
                    // espiras crescendo da plataforma
                    for(int i=0;i<7;i++){float a=i/7f*Mathf.PI*2,rr=Random.Range(0.2f,0.8f)*s,h=Random.Range(0.4f,1.5f)*s;
                        Cone(p,Met(i%2==0?col:col2),0.12f*s,h,new Vector3(Mathf.Cos(a)*rr,0.25f*s,Mathf.Sin(a)*rr),Quaternion.Euler(Random.Range(-10,10),a*Mathf.Rad2Deg,0));}
                    // luz difusa
                    Light(p,col,6*s,1.4f,new Vector3(0,0.5f,0)*s);
                    break;
                }
                case 2: // flor de cristal
                {
                    // pétalas (8 cristais radiais)
                    for(int i=0;i<8;i++){float a=i/8f*Mathf.PI*2,h=1.4f*s;
                        var petal=Cone(p,Met(i%2==0?col:col2),0.16f*s,h,new Vector3(Mathf.Cos(a)*0.4f*s,0,Mathf.Sin(a)*0.4f*s),Quaternion.Euler(-55,a*Mathf.Rad2Deg,0));
                        Cone(p,Met(accent),0.08f*s,h*0.3f,petal.transform.localPosition+new Vector3(Mathf.Cos(a)*h*0.5f*s,h*0.5f*s,Mathf.Sin(a)*h*0.5f*s),Quaternion.Euler(180-55,a*Mathf.Rad2Deg,0));}
                    // gema central
                    Sph(p,Glow(accent,2.0f),0.3f*s,new Vector3(0,0.3f,0)*s);
                    for(int k=0;k<3;k++) Sph(p,Met(col),0.12f*s,new Vector3(0,(0.5f+k*0.3f)*s,0));
                    Light(p,accent,5*s,2.0f,new Vector3(0,0.5f,0)*s);
                    break;
                }
                default: // arco de cristal
                {
                    // dois pilares
                    for(int i=0;i<2;i++){float x=(i==0?-0.7f:0.7f)*s;
                        Cone(p,Met(col),0.2f*s,2.4f*s,new Vector3(x,0,0),Quaternion.Euler(i==0?-8:8,0,0));
                        Cone(p,Met(col),0.14f*s,0.6f*s,new Vector3(x,2.4f*s,0),Quaternion.Euler(180+(i==0?-8:8),0,0));}
                    // arquitrave cristalina
                    Cube(p,Iri(col2),new Vector3(1.8f,0.2f,0.22f)*s,new Vector3(0,2.3f,0)*s);
                    // cristais pendentes
                    for(int i=0;i<5;i++){float x=(i-2)*0.32f*s,h=Random.Range(0.3f,0.7f)*s; Cone(p,Met(i%2==0?accent:col),0.08f*s,h,new Vector3(x,2.2f-h*0.5f,0)*s,Quaternion.Euler(180,0,0));}
                    Light(p,accent,6*s,1.5f,new Vector3(0,2f,0)*s);
                    break;
                }
            }
        }

        // ================================================================
        //  3 — FLORESTA MECÂNICA
        // ================================================================
        static void Mecanico(int v, Transform p, Color metal, Color circuit, Color spark, float s)
        {
            switch (v)
            {
                case 0: // árvore de engrenagens
                {
                    Cyl(p,Met(metal),0.15f*s,1.1f*s,new Vector3(0,1.1f,0)*s);
                    // galhos metálicos com engrenagens
                    for(int i=0;i<3;i++){float a=i/3f*Mathf.PI*2+0.5f,y=(0.6f+i*0.3f)*s;
                        var arm=Prim.Cylinder(p,Met(metal)); arm.transform.localScale=new Vector3(0.06f,0.5f,0.06f)*s; arm.transform.localPosition=new Vector3(Mathf.Cos(a)*0.45f*s,y,Mathf.Sin(a)*0.45f*s); arm.transform.LookAt(p.position+new Vector3(0,y,0)*s); arm.transform.Rotate(90,0,0);
                        // engrenagem (cilindro dentado)
                        Cyl(p,Met(circuit),0.22f*s,0.08f*s,new Vector3(Mathf.Cos(a)*0.9f*s,y,Mathf.Sin(a)*0.9f*s));
                        for(int d=0;d<8;d++){float da=d/8f*Mathf.PI*2; Cube(p,Met(circuit),new Vector3(0.06f,0.06f,0.1f)*s,new Vector3(Mathf.Cos(a)*0.9f*s+Mathf.Cos(da)*0.24f*s,y,Mathf.Sin(a)*0.9f*s+Mathf.Sin(da)*0.24f*s),new Vector3(0,da*Mathf.Rad2Deg,0));}
                        Sph(p,Glow(spark,1.8f),0.1f*s,new Vector3(Mathf.Cos(a)*0.9f*s,y+0.12f*s,Mathf.Sin(a)*0.9f*s));}
                    // copa com painel solar
                    Cube(p,Met(circuit),new Vector3(1.2f,0.12f,1.2f)*s,new Vector3(0,2.3f,0)*s);
                    Light(p,spark,5*s,1.6f,new Vector3(0,2.3f,0)*s);
                    break;
                }
                case 1: // estação de vapor
                {
                    // base
                    Cube(p,Met(metal),new Vector3(1.0f,0.4f,1.0f)*s,new Vector3(0,0.2f,0)*s);
                    // chaminés
                    for(int i=0;i<3;i++){float x=(i-1)*0.38f*s,h=(1.2f+i*0.25f)*s;
                        Cyl(p,Met(metal),0.12f*s,h,new Vector3(x,0.4f+h*0.5f,0)*s);
                        Cyl(p,Met(metal*0.7f),0.15f*s,0.08f*s,new Vector3(x,0.4f+h,0)*s);
                        Sph(p,Glow(spark*0.7f,1.0f),0.12f*s,new Vector3(x,0.4f+h+0.14f,0)*s);}
                    // tubulação
                    for(int i=0;i<2;i++){var pipe=Prim.Cylinder(p,Met(circuit)); pipe.transform.localScale=new Vector3(0.08f,0.9f,0.08f)*s; pipe.transform.localPosition=new Vector3(0.4f*s,(0.45f)*s,(i==0?0.4f:-0.4f)*s); pipe.transform.localEulerAngles=new Vector3(0,0,90);}
                    // válvulas
                    for(int i=0;i<2;i++) Sph(p,Met(spark),0.1f*s,new Vector3((i==0?0.4f:-0.4f)*s,0.48f*s,0));
                    Light(p,spark,4*s,1.2f,new Vector3(0,1.5f,0)*s);
                    break;
                }
                case 2: // rede de circuitos no chão + antena
                {
                    // placa de circuito (chão)
                    Cube(p,Met(circuit*0.7f),new Vector3(2.0f,0.08f,2.0f)*s,new Vector3(0,0.04f,0)*s);
                    // trilhas de circuito
                    Color[] tc={circuit,spark,circuit*0.8f};
                    for(int i=0;i<6;i++){float a=i/6f*Mathf.PI*2;
                        var tr=Prim.Cylinder(p,Met(tc[i%tc.Length])); tr.transform.localScale=new Vector3(0.04f,0.7f,0.04f)*s; tr.transform.localPosition=new Vector3(Mathf.Cos(a)*0.6f*s,0.08f*s,Mathf.Sin(a)*0.6f*s); tr.transform.LookAt(p.position); tr.transform.Rotate(90,0,0);}
                    // nós (processadores)
                    for(int i=0;i<4;i++){float a=i/4f*Mathf.PI*2; Cube(p,Met(circuit),new Vector3(0.22f,0.12f,0.22f)*s,new Vector3(Mathf.Cos(a)*0.65f*s,0.1f*s,Mathf.Sin(a)*0.65f*s)); Sph(p,Glow(spark,1.8f),0.07f*s,new Vector3(Mathf.Cos(a)*0.65f*s,0.22f*s,Mathf.Sin(a)*0.65f*s));}
                    // antena central
                    Cyl(p,Met(metal),0.07f*s,1.8f*s,new Vector3(0,1.0f,0)*s);
                    Sph(p,Glow(spark,2.2f),0.18f*s,new Vector3(0,1.96f,0)*s);
                    Light(p,spark,6*s,1.8f,new Vector3(0,2f,0)*s);
                    break;
                }
                default: // inseto robótico gigante
                {
                    // corpo principal
                    var body=Prim.Sphere(p,Met(circuit)); body.transform.localScale=new Vector3(0.7f,0.45f,1.1f)*s; body.transform.localPosition=new Vector3(0,0.55f,0)*s;
                    // cabeça
                    Sph(p,Met(metal),0.32f*s,new Vector3(0,0.55f,0.65f)*s);
                    // olhos
                    Sph(p,Glow(spark,2.2f),0.12f*s,new Vector3(-0.14f*s,0.62f*s,0.92f*s));
                    Sph(p,Glow(spark,2.2f),0.12f*s,new Vector3(0.14f*s,0.62f*s,0.92f*s));
                    // pernas (6)
                    for(int i=0;i<6;i++){float side=i<3?1:-1,idx=i<3?i:i-3; float a=((float)idx/3-0.5f)*1.8f;
                        var leg=Prim.Cylinder(p,Met(metal)); leg.transform.localScale=new Vector3(0.07f,0.6f,0.07f)*s; leg.transform.localPosition=new Vector3(side*0.42f*s,0.35f*s,a*0.4f*s); leg.transform.localEulerAngles=new Vector3(0,0,side>0?40:-40);
                        Sph(p,Met(circuit*0.8f),0.09f*s,new Vector3(side*0.8f*s,0.1f*s,a*0.4f*s));}
                    // antenas
                    for(int i=0;i<2;i++){float x=(i==0?-0.18f:0.18f)*s; var ant=Prim.Cylinder(p,Met(metal)); ant.transform.localScale=new Vector3(0.04f,0.4f,0.04f)*s; ant.transform.localPosition=new Vector3(x,0.95f*s,0.72f*s); ant.transform.localEulerAngles=new Vector3(-30,0,i==0?-20:20);}
                    Light(p,spark,4*s,1.2f,new Vector3(0,0.5f,0)*s);
                    break;
                }
            }
        }

        // ================================================================
        //  4 — BIOMA DAS NUVENS
        // ================================================================
        static void Nuvens(int v, Transform p, Color cloud, Color light, Color accent, float s)
        {
            switch (v)
            {
                case 0: // ilha flutuante com nuvens
                {
                    // base de nuvem flutuante (5 esferas sobrepostas)
                    float[] offX={0,-0.5f,0.5f,-0.3f,0.3f}; float[] offZ={0,0.3f,-0.3f,-0.5f,0.5f}; float[] sc={1f,0.7f,0.7f,0.65f,0.65f};
                    for(int i=0;i<5;i++){var c=Prim.Sphere(p,Mat(cloud)); c.transform.localScale=Vector3.one*sc[i]*s*1.2f; c.transform.localPosition=new Vector3(offX[i]*s,0.35f*s*sc[i],offZ[i]*s);}
                    // topo da ilha (terra e grama)
                    Cube(p,Mat(H("#5A7A30")),new Vector3(1.0f,0.2f,1.0f)*s,new Vector3(0,0.85f,0)*s);
                    // árvorezinha em cima
                    Cyl(p,Mat(H("#6E5638")),0.08f*s,0.55f*s,new Vector3(0.1f*s,1.18f*s,0.1f*s));
                    Sph(p,Mat(H("#3C6E4A")),0.35f*s,new Vector3(0.1f*s,1.55f*s,0.1f*s));
                    // cortina de chuva suave
                    for(int i=0;i<8;i++){float a=i/8f*Mathf.PI*2; var d=Prim.Cylinder(p,Mat(accent*0.7f)); d.transform.localScale=new Vector3(0.025f,0.4f,0.025f)*s; d.transform.localPosition=new Vector3(Mathf.Cos(a)*0.65f*s,-0.2f*s,Mathf.Sin(a)*0.65f*s);}
                    break;
                }
                case 1: // baleia voadora
                {
                    // corpo
                    var body=Prim.Sphere(p,Mat(cloud)); body.transform.localScale=new Vector3(1.0f,0.58f,1.9f)*s; body.transform.localPosition=new Vector3(0,0.4f,0)*s;
                    // cauda
                    Cone(p,Mat(cloud),0.4f*s,0.6f*s,new Vector3(0,0.35f,-1.2f)*s,Quaternion.Euler(30,0,0),4);
                    // barbatanas peitorais
                    for(int i=0;i<2;i++){float x=(i==0?-0.6f:0.6f)*s; var fin=Prim.Sphere(p,Mat(cloud)); fin.transform.localScale=new Vector3(0.55f,0.18f,0.8f)*s; fin.transform.localPosition=new Vector3(x,0.2f*s,0.3f*s); fin.transform.localEulerAngles=new Vector3(0,0,i==0?-20:20);}
                    // bossas de nuvem no corpo
                    for(int i=0;i<6;i++){float a=i/6f*Mathf.PI*2; Sph(p,Mat(cloud*1.05f),0.28f*s,new Vector3(Mathf.Cos(a)*0.55f*s,0.55f*s,Mathf.Sin(a)*0.28f*s));}
                    // olho
                    Sph(p,Mat(H("#2A2A40")),0.09f*s,new Vector3(-0.22f*s,0.52f*s,0.82f*s));
                    Sph(p,Mat(H("#FFFFFF")),0.04f*s,new Vector3(-0.19f*s,0.54f*s,0.88f*s));
                    break;
                }
                case 2: // torre de nuvens
                {
                    // 5 nuvens empilhadas diminuindo
                    float[] heights={0,0.8f,1.55f,2.22f,2.8f}; float[] sizes={1.0f,0.85f,0.75f,0.62f,0.5f};
                    for(int i=0;i<5;i++){var c=Prim.Sphere(p,Mat(cloud*((float)(10+i)/13))); c.transform.localScale=Vector3.one*sizes[i]*s*1.2f; c.transform.localPosition=new Vector3(Random.Range(-0.1f,0.1f)*s,heights[i]*s,Random.Range(-0.1f,0.1f)*s);}
                    // cortina de chuva ao redor
                    for(int i=0;i<12;i++){float a=i/12f*Mathf.PI*2,h=Random.Range(0.3f,0.8f)*s; var d=Prim.Cylinder(p,Mat(accent*0.6f)); d.transform.localScale=new Vector3(0.02f,h,0.02f)*s; d.transform.localPosition=new Vector3(Mathf.Cos(a)*0.55f*s,0.2f*s-h*0.5f,Mathf.Sin(a)*0.55f*s);}
                    break;
                }
                default: // puff de nuvem simples mas grande
                {
                    for(int i=0;i<8;i++){float a=i/8f*Mathf.PI*2,rr=Random.Range(0.3f,0.7f)*s,y=Random.Range(0.1f,0.5f)*s; Sph(p,Mat(cloud*(0.9f+Random.value*0.15f)),Random.Range(0.3f,0.6f)*s,new Vector3(Mathf.Cos(a)*rr,y,Mathf.Sin(a)*rr));}
                    Sph(p,Mat(cloud),0.65f*s,new Vector3(0,0.42f,0)*s);
                    // raio dourado saindo embaixo
                    for(int i=0;i<5;i++){float a=i/5f*Mathf.PI*2; var ray=Prim.Cylinder(p,Mat(accent)); ray.transform.localScale=new Vector3(0.03f,0.5f,0.03f)*s; ray.transform.localPosition=new Vector3(Mathf.Cos(a)*0.3f*s,-0.35f*s,Mathf.Sin(a)*0.3f*s); ray.transform.LookAt(p.position+new Vector3(0,0.3f,0)*s); ray.transform.Rotate(90,0,0);}
                    break;
                }
            }
        }

        // ================================================================
        //  5 — OCEANO VIVO
        // ================================================================
        static void OceanoVivo(int v, Transform p, Color base_, Color glow_, Color accent, float s)
        {
            switch (v)
            {
                case 0: // formação de coral ramificado
                {
                    // base pedestal orgânica
                    var bs=Prim.Rock(p,Mat(base_),0.35f); bs.transform.localScale=new Vector3(1.1f,0.4f,1.1f)*s; bs.transform.localPosition=new Vector3(0,0.2f,0)*s;
                    // ramos de coral (bifurcados)
                    for(int i=0;i<6;i++){float a=i/6f*Mathf.PI*2,h=Random.Range(0.7f,1.6f)*s;
                        Cone(p,Mat(base_*1.1f),0.1f*s,h,new Vector3(Mathf.Cos(a)*0.3f*s,0.4f*s,Mathf.Sin(a)*0.3f*s),Quaternion.Euler(Random.Range(-30,30),a*Mathf.Rad2Deg,0));
                        // bifurcação
                        float a2=a+0.5f,h2=h*0.55f;
                        Cone(p,Mat(base_),0.07f*s,h2,new Vector3(Mathf.Cos(a)*0.3f*s+Mathf.Cos(a2)*h*0.4f,0.4f+h*0.5f,Mathf.Sin(a)*0.3f*s+Mathf.Sin(a2)*h*0.4f)*s,Quaternion.Euler(-25,a2*Mathf.Rad2Deg,0));
                        Sph(p,Glow(accent,1.6f),0.08f*s,new Vector3(Mathf.Cos(a)*(0.3f+h*0.8f)*s,(0.4f+h)*s,Mathf.Sin(a)*(0.3f+h*0.8f)*s));}
                    // anêmonas
                    for(int i=0;i<4;i++){float a=i/4f*Mathf.PI*2+0.4f;
                        for(int k=0;k<6;k++){float ta=k/6f*Mathf.PI*2; Cone(p,Mat(glow_),0.05f*s,0.22f*s,new Vector3(Mathf.Cos(a)*0.85f*s,0.35f*s,Mathf.Sin(a)*0.85f*s)+new Vector3(Mathf.Cos(ta)*0.16f*s,0,Mathf.Sin(ta)*0.16f*s),Quaternion.Euler(-60,ta*Mathf.Rad2Deg,0));}
                        Sph(p,Glow(accent,1.4f),0.09f*s,new Vector3(Mathf.Cos(a)*0.85f*s,0.45f*s,Mathf.Sin(a)*0.85f*s));}
                    Light(p,accent,5*s,1.4f,new Vector3(0,0.8f,0)*s);
                    break;
                }
                case 1: // amêijoa gigante com pérola
                {
                    // concha inferior
                    var lo=Prim.Sphere(p,Mat(base_)); lo.transform.localScale=new Vector3(1.3f,0.4f,1.0f)*s; lo.transform.localPosition=new Vector3(0,0.2f,0)*s;
                    // concha superior (aberta)
                    var up=Prim.Sphere(p,Mat(base_*1.1f)); up.transform.localScale=new Vector3(1.3f,0.5f,1.0f)*s; up.transform.localPosition=new Vector3(0,0.22f,0)*s; up.transform.localEulerAngles=new Vector3(-45,0,0);
                    // interior nacarado
                    var ins=Prim.Sphere(p,Sat(glow_)); ins.transform.localScale=new Vector3(1.0f,0.3f,0.8f)*s; ins.transform.localPosition=new Vector3(0,0.38f,0.15f)*s;
                    // pérola brilhante
                    Sph(p,Glow(accent,2.2f),0.28f*s,new Vector3(0,0.65f,0.2f)*s);
                    // tentáculos ao redor
                    for(int i=0;i<6;i++){float a=i/6f*Mathf.PI*2; Cone(p,Mat(glow_*0.8f),0.04f*s,0.35f*s,new Vector3(Mathf.Cos(a)*0.6f*s,0.15f*s,Mathf.Sin(a)*0.45f*s),Quaternion.Euler(-70,a*Mathf.Rad2Deg,0),6);}
                    Light(p,accent,4*s,1.8f,new Vector3(0,0.7f,0)*s);
                    break;
                }
                case 2: // coluna orgânica pulsante com veias
                {
                    // tronco orgânico central
                    Cone(p,Mat(base_),0.35f*s,2.2f*s,Vector3.zero,default,8);
                    // anéis bioluminescentes
                    for(int i=0;i<5;i++){float y=(0.4f+i*0.44f)*s,r=(0.38f-i*0.05f)*s; Cyl(p,Glow(accent,1.2f+i*0.15f),r,0.1f*s,new Vector3(0,y,0)); for(int k=0;k<8;k++){float a=k/8f*Mathf.PI*2; Sph(p,Glow(accent,1.8f),0.07f*s,new Vector3(Mathf.Cos(a)*r,y+0.06f*s,Mathf.Sin(a)*r));}}
                    // veias externas
                    for(int i=0;i<5;i++){float a=i/5f*Mathf.PI*2; var vein=Prim.Cylinder(p,Mat(glow_)); vein.transform.localScale=new Vector3(0.04f,1.1f,0.04f)*s; vein.transform.localPosition=new Vector3(Mathf.Cos(a)*0.36f*s,1.1f*s,Mathf.Sin(a)*0.36f*s); vein.transform.LookAt(p.position+new Vector3(0,2f,0)*s); vein.transform.Rotate(90,0,0);}
                    Light(p,accent,6*s,1.6f,new Vector3(0,1.2f,0)*s);
                    break;
                }
                default: // floresta de algas com bioluminescência
                {
                    for(int i=0;i<7;i++){float a=i/7f*Mathf.PI*2,rr=Random.Range(0.4f,1.0f)*s,h=Random.Range(0.8f,2.0f)*s;
                        var kelp=Cone(p,Mat(glow_*(0.7f+Random.value*0.4f)),0.07f*s,h,new Vector3(Mathf.Cos(a)*rr,h*0.5f,Mathf.Sin(a)*rr));
                        // nós ao longo da alga
                        for(int k=0;k<3;k++) Sph(p,Glow(accent,1.3f),0.07f*s,new Vector3(Mathf.Cos(a)*rr,h*(0.25f+k*0.25f),Mathf.Sin(a)*rr));}
                    Light(p,accent,5*s,1.2f,new Vector3(0,0.8f,0)*s);
                    break;
                }
            }
        }

        // ================================================================
        //  6 — SELVA BIOLUMINESCENTE
        // ================================================================
        static void Bioluminescente(int v, Transform p, Color green, Color glow_, Color accent, float s)
        {
            switch (v)
            {
                case 0: // árvore bioluminescente com copa de flores
                {
                    Cyl(p,Mat(H("#1E3A28")),0.18f*s,1.3f*s,new Vector3(0,1.3f,0)*s);
                    // galhos radiais com flores bio
                    for(int i=0;i<5;i++){float a=i/5f*Mathf.PI*2,y=(0.8f+i*0.15f)*s;
                        var br=Prim.Cylinder(p,Mat(H("#1E3A28"))); br.transform.localScale=new Vector3(0.06f,0.55f,0.06f)*s; br.transform.localPosition=new Vector3(Mathf.Cos(a)*0.5f*s,y,Mathf.Sin(a)*0.5f*s); br.transform.LookAt(p.position+new Vector3(0,y,0)*s); br.transform.Rotate(90,0,0);
                        // flores bioluminescentes nos tips
                        for(int k=0;k<5;k++){float fa=k/5f*Mathf.PI*2; Sph(p,Glow(k%2==0?accent:glow_,1.6f),0.08f*s,new Vector3(Mathf.Cos(a)*1.1f*s+Mathf.Cos(fa)*0.15f*s,y+0.08f*s,Mathf.Sin(a)*1.1f*s+Mathf.Sin(fa)*0.15f*s));}}
                    // musgo bioluminescente no chão
                    for(int i=0;i<8;i++){float a=i/8f*Mathf.PI*2; Sph(p,Glow(glow_,0.8f),0.1f*s,new Vector3(Mathf.Cos(a)*0.9f*s,0.1f*s,Mathf.Sin(a)*0.9f*s));}
                    Light(p,accent,7*s,2.0f,new Vector3(0,1.6f,0)*s);
                    break;
                }
                case 1: // cortina de vinhas luminosas
                {
                    // ponto de ancoragem superior
                    Sph(p,Mat(green*0.7f),0.3f*s,new Vector3(0,2.8f,0)*s);
                    // vinhas pendentes (12)
                    for(int i=0;i<7;i++){float a=i/7f*Mathf.PI*2,rr=Random.Range(0.1f,0.6f)*s,h=Random.Range(1.2f,2.6f)*s;
                        Cyl(p,Mat(green*0.8f),0.03f*s,h,new Vector3(Mathf.Cos(a)*rr,2.8f-h*0.5f,Mathf.Sin(a)*rr)*s);
                        // nós luminosos ao longo da vinha
                        for(int k=0;k<3;k++) Sph(p,Glow(k%2==0?accent:glow_,1.4f+Random.value*0.6f),0.07f*s,new Vector3(Mathf.Cos(a)*rr,2.8f-(h*(0.2f+k*0.3f)),Mathf.Sin(a)*rr)*s);}
                    Light(p,accent,6*s,1.8f,new Vector3(0,2f,0)*s);
                    break;
                }
                case 2: // anel de fadas (cogumelos bio + flores)
                {
                    for(int i=0;i<8;i++){float a=i/8f*Mathf.PI*2,rr=1.2f*s;
                        // cogumelo luminoso
                        Cyl(p,Mat(green*0.6f),0.08f*s,0.5f*s,new Vector3(Mathf.Cos(a)*rr,0.5f*s,Mathf.Sin(a)*rr));
                        var mc=Prim.Sphere(p,Glow(glow_,1.3f)); mc.transform.localScale=new Vector3(0.45f,0.22f,0.45f)*s; mc.transform.localPosition=new Vector3(Mathf.Cos(a)*rr,1.05f*s,Mathf.Sin(a)*rr);
                        // flor bio entre cogumelos
                        Sph(p,Glow(accent,1.7f),0.11f*s,new Vector3(Mathf.Cos(a+0.4f)*0.9f*s,0.18f*s,Mathf.Sin(a+0.4f)*0.9f*s));}
                    // poça central luminosa
                    Cyl(p,Glow(glow_,0.9f),0.55f*s,0.06f*s,new Vector3(0,0.04f,0)*s);
                    Light(p,accent,5*s,1.6f,new Vector3(0,0.5f,0)*s);
                    break;
                }
                default: // flor bio monumental
                {
                    Cyl(p,Mat(green*0.7f),0.12f*s,1.0f*s,new Vector3(0,1.0f,0)*s);
                    // 8 pétalas
                    for(int i=0;i<8;i++){float a=i/8f*Mathf.PI*2;
                        var petal=Prim.Sphere(p,Glow(i%2==0?glow_:green,1.2f)); petal.transform.localScale=new Vector3(0.18f,0.55f,0.18f)*s; petal.transform.localPosition=new Vector3(Mathf.Cos(a)*0.35f*s,1.95f*s,Mathf.Sin(a)*0.35f*s); petal.transform.localRotation=Quaternion.Euler(0,0,a*Mathf.Rad2Deg+90);}
                    Sph(p,Glow(accent,2.2f),0.32f*s,new Vector3(0,2.0f,0)*s);
                    // estames brilhantes
                    for(int i=0;i<6;i++){float a=i/6f*Mathf.PI*2; Cone(p,Glow(accent,1.8f),0.04f*s,0.28f*s,new Vector3(Mathf.Cos(a)*0.2f*s,2.1f*s,Mathf.Sin(a)*0.2f*s));}
                    Light(p,accent,7*s,2.4f,new Vector3(0,2.2f,0)*s);
                    break;
                }
            }
        }

        // ================================================================
        //  7 — BIOMA NEURAL
        // ================================================================
        static void Neural(int v, Transform p, Color neuron, Color glow_, Color synapse, float s)
        {
            switch (v)
            {
                case 0: // neurônio gigante com axônios
                {
                    var body=Sph(p,Mat(neuron*1.1f),0.45f*s,new Vector3(0,0.85f,0)*s);
                    Sph(p,Glow(synapse,2.0f),0.2f*s,new Vector3(0,0.85f,0)*s);
                    // dendrites (7 braços)
                    Vector3[] dirs={new Vector3(1,0.5f,0),new Vector3(-1,0.5f,0),new Vector3(0,1,0),new Vector3(0.7f,-0.3f,0.7f),new Vector3(-0.7f,-0.3f,0.7f),new Vector3(0,-0.3f,-1f),new Vector3(0.5f,0.8f,-0.5f)};
                    for(int i=0;i<dirs.Length;i++){var d=dirs[i].normalized; float len=Random.Range(0.7f,1.5f)*s;
                        var arm=Prim.Cylinder(p,Mat(neuron)); arm.transform.localScale=new Vector3(0.065f,len,0.065f)*s; arm.transform.localPosition=new Vector3(0,0.85f,0)*s+d*len*0.5f*s; arm.transform.LookAt(body.transform.position); arm.transform.Rotate(90,0,0);
                        Sph(p,Glow(synapse,1.5f),0.12f*s,new Vector3(0,0.85f,0)*s+d*len*s);}
                    Light(p,synapse,6*s,1.8f,new Vector3(0,0.85f,0)*s);
                    break;
                }
                case 1: // junção sináptica (dois neurônios conectados)
                {
                    // neurônio A
                    Sph(p,Mat(neuron),0.38f*s,new Vector3(-0.7f,0.8f,-0.3f)*s);
                    Sph(p,Glow(synapse,1.8f),0.16f*s,new Vector3(-0.7f,0.8f,-0.3f)*s);
                    // neurônio B
                    Sph(p,Mat(glow_),0.32f*s,new Vector3(0.7f,0.7f,0.3f)*s);
                    Sph(p,Glow(synapse,1.8f),0.14f*s,new Vector3(0.7f,0.7f,0.3f)*s);
                    // axônio conectando
                    var axon=Prim.Cylinder(p,Mat(neuron*0.8f)); axon.transform.localScale=new Vector3(0.07f,1.0f,0.07f)*s; axon.transform.localPosition=Vector3.Lerp(new Vector3(-0.7f,0.8f,-0.3f),new Vector3(0.7f,0.7f,0.3f),0.5f)*s; axon.transform.LookAt(p.position+new Vector3(0.7f,0.7f,0.3f)*s); axon.transform.Rotate(90,0,0);
                    // sinal elétrico (esfera no meio)
                    Sph(p,Glow(synapse,2.5f),0.1f*s,Vector3.Lerp(new Vector3(-0.7f,0.8f,-0.3f),new Vector3(0.7f,0.7f,0.3f),0.5f)*s);
                    // múltiplos dendritos em cada nó
                    for(int i=0;i<4;i++){float a=i/4f*Mathf.PI*2; var arm=Prim.Cylinder(p,Mat(neuron)); arm.transform.localScale=new Vector3(0.04f,0.4f,0.04f)*s; arm.transform.localPosition=new Vector3(-0.7f+Mathf.Cos(a)*0.45f,0.8f,(-0.3f+Mathf.Sin(a)*0.45f))*s; arm.transform.LookAt(p.position+new Vector3(-0.7f,0.8f,-0.3f)*s); arm.transform.Rotate(90,0,0);}
                    Light(p,synapse,5*s,1.6f,new Vector3(0,0.8f,0)*s);
                    break;
                }
                case 2: // floresta neural densa
                {
                    for(int i=0;i<5;i++){float a=i/5f*Mathf.PI*2,rr=Random.Range(0.4f,0.9f)*s;
                        Cyl(p,Mat(neuron),0.07f*s,0.9f*s,new Vector3(Mathf.Cos(a)*rr,0.9f*s,Mathf.Sin(a)*rr));
                        Sph(p,Mat(glow_),0.3f*s,new Vector3(Mathf.Cos(a)*rr,1.9f*s,Mathf.Sin(a)*rr));
                        Sph(p,Glow(synapse,1.5f),0.12f*s,new Vector3(Mathf.Cos(a)*rr,1.9f*s,Mathf.Sin(a)*rr));
                        // conexões entre neurônios vizinhos
                        float an=a+Mathf.PI*2/5; float lx=Mathf.Cos(a)*rr,lz=Mathf.Sin(a)*rr,rx=Mathf.Cos(an)*rr,rz=Mathf.Sin(an)*rr;
                        var con=Prim.Cylinder(p,Mat(neuron*0.7f)); con.transform.localScale=new Vector3(0.04f,Vector3.Distance(new Vector3(lx,0,lz),new Vector3(rx,0,rz))*0.5f,0.04f)*s; con.transform.localPosition=new Vector3((lx+rx)*0.5f,1.85f*s,(lz+rz)*0.5f); con.transform.LookAt(p.position+new Vector3(rx,1.85f,rz)*s); con.transform.Rotate(90,0,0);}
                    // nó central
                    Sph(p,Glow(synapse,2.2f),0.22f*s,new Vector3(0,0.22f,0)*s);
                    Light(p,synapse,6*s,2.0f,new Vector3(0,1.2f,0)*s);
                    break;
                }
                default: // teia neural no chão com nós
                {
                    // nós externos
                    for(int i=0;i<6;i++){float a=i/6f*Mathf.PI*2,rr=1.2f*s;
                        Sph(p,Glow(synapse,1.4f),0.16f*s,new Vector3(Mathf.Cos(a)*rr,0.18f*s,Mathf.Sin(a)*rr));
                        // fios para o centro
                        var w=Prim.Cylinder(p,Mat(neuron*0.6f)); w.transform.localScale=new Vector3(0.04f,rr/2+0.5f,0.04f)*s; w.transform.localPosition=new Vector3(Mathf.Cos(a)*rr*0.5f,0.08f*s,Mathf.Sin(a)*rr*0.5f); w.transform.LookAt(p.position+new Vector3(0,0.08f,0)*s); w.transform.Rotate(90,0,0);}
                    // nó central
                    Sph(p,Glow(synapse,2.4f),0.25f*s,new Vector3(0,0.28f,0)*s);
                    Light(p,synapse,5*s,1.6f,new Vector3(0,0.3f,0)*s);
                    break;
                }
            }
        }

        // ================================================================
        //  8 — FLORESTA DE VIDRO
        // ================================================================
        static void Vidro(int v, Transform p, Color glass, Color glass2, Color prism, float s)
        {
            switch (v)
            {
                case 0: // bosque de vidro (3 árvores)
                {
                    for(int i=0;i<3;i++){float x=(i-1)*0.85f*s,h=(1.2f+i*0.3f)*s;
                        Cyl(p,Iri(glass),0.12f*s,h,new Vector3(x,h*0.5f,0));
                        var crown=Cone(p,Iri(glass),0.6f*s,0.9f*s,new Vector3(x,h,0),Quaternion.Euler(0,45+i*30,0));
                        Cone(p,Sat(prism),0.3f*s,0.6f*s,new Vector3(x,h+0.3f,0),Quaternion.Euler(0,45+i*30,0));
                        // raios de luz saindo da copa
                        for(int k=0;k<4;k++){float a=k/4f*Mathf.PI*2; var ray=Prim.Cylinder(p,Sat(prism*0.7f+H("#FFFFFF")*0.3f)); ray.transform.localScale=new Vector3(0.025f,0.6f,0.025f)*s; ray.transform.localPosition=new Vector3(x+Mathf.Cos(a)*0.5f*s,h+0.1f,Mathf.Sin(a)*0.5f*s); ray.transform.LookAt(p.position+new Vector3(x,h,0)); ray.transform.Rotate(90,0,0);}}
                    break;
                }
                case 1: // conjunto de prismas com arco-íris
                {
                    // prismas em alturas variadas
                    Color[] pc={glass,glass2,prism,glass,glass2};
                    for(int i=0;i<5;i++){float a=i/5f*Mathf.PI*2+i*0.4f,h=Random.Range(0.7f,2.0f)*s;
                        Cone(p,Iri(pc[i]),0.2f*s,h,new Vector3(Mathf.Cos(a)*0.55f*s,h*0.5f,Mathf.Sin(a)*0.55f*s),Quaternion.Euler(Random.Range(-8,8),a*Mathf.Rad2Deg,0));
                        Cone(p,Iri(pc[i]),0.15f*s,h*0.3f,new Vector3(Mathf.Cos(a)*0.55f*s,h,Mathf.Sin(a)*0.55f*s),Quaternion.Euler(180,a*Mathf.Rad2Deg,0));}
                    // raios de arco-íris saindo do centro
                    Color[] rainbow={H("#E0708A"),H("#E0A85A"),H("#E8D45A"),H("#6FC07A"),H("#5FA0D8"),H("#9C7AD8")};
                    for(int i=0;i<rainbow.Length;i++){float a=i/6f*Mathf.PI*2; var r=Prim.Cylinder(p,Mat(rainbow[i])); r.transform.localScale=new Vector3(0.03f,1.4f,0.03f)*s; r.transform.localPosition=new Vector3(Mathf.Cos(a)*1.0f*s,0.6f*s,Mathf.Sin(a)*1.0f*s); r.transform.LookAt(p.position+new Vector3(0,0.6f,0)*s); r.transform.Rotate(90,0,0);}
                    break;
                }
                case 2: // jardim de vidro com espirais
                {
                    // base plana cristalina
                    Cube(p,Iri(glass),new Vector3(2.0f,0.1f,2.0f)*s,new Vector3(0,0.05f,0)*s);
                    // espirais/flores de vidro
                    for(int i=0;i<6;i++){float a=i/6f*Mathf.PI*2,rr=Random.Range(0.4f,0.9f)*s,h=Random.Range(0.5f,1.4f)*s;
                        Cone(p,Iri(i%2==0?glass:glass2),0.15f*s,h,new Vector3(Mathf.Cos(a)*rr,h*0.5f,Mathf.Sin(a)*rr),Quaternion.Euler(0,45,0));
                        // pétalas ao redor
                        for(int k=0;k<4;k++){float pa=k/4f*Mathf.PI*2; Sph(p,Sat(prism),0.1f*s,new Vector3(Mathf.Cos(a)*rr+Mathf.Cos(pa)*0.22f*s,h,Mathf.Sin(a)*rr+Mathf.Sin(pa)*0.22f*s));}}
                    break;
                }
                default: // espelho / superfície reflexiva
                {
                    // painel principal reflexivo
                    Cube(p,Met(glass),new Vector3(1.5f,2.2f,0.1f)*s,new Vector3(0,1.1f,0)*s,new Vector3(0,Random.Range(-20,20),0));
                    // moldura
                    Cube(p,Sat(glass2),new Vector3(1.65f,0.12f,0.14f)*s,new Vector3(0,0.06f,0)*s);
                    Cube(p,Sat(glass2),new Vector3(1.65f,0.12f,0.14f)*s,new Vector3(0,2.2f,0)*s);
                    // reflexo (imagem distorcida - esferas brilhantes)
                    for(int i=0;i<5;i++){float y=(0.4f+i*0.4f)*s; Sph(p,Iri(prism),0.1f*s,new Vector3(Random.Range(-0.5f,0.5f)*s,y,0.08f*s));}
                    // fragmentos menores ao redor
                    for(int i=0;i<4;i++){float a=i/4f*Mathf.PI*2+0.8f; Cube(p,Met(glass2),new Vector3(0.5f,0.8f,0.06f)*s,new Vector3(Mathf.Cos(a)*1.2f*s,0.5f*s,Mathf.Sin(a)*1.2f*s),new Vector3(0,a*Mathf.Rad2Deg,0));}
                    break;
                }
            }
        }

        // ================================================================
        //  9 — BIOMA FRACTAL
        // ================================================================
        static void Fractal(int v, Transform p, Color branch, Color leaf, Color accent, float s)
        {
            switch (v)
            {
                case 0: // árvore fractal profunda (5 níveis)
                    FractalBranch(p, Vector3.zero, Vector3.up, 0.7f*s, 5, branch, leaf, accent);
                    break;
                case 1: // cadeia de montanhas fractais
                {
                    for(int i=0;i<3;i++){float x=(i-1)*1.8f*s; var peak=Cone(p,Mat(branch),0.8f*s,(1.4f+i*0.35f)*s,new Vector3(x,0,0),default,4);
                        // sub-picos
                        float[] sx={-0.5f,0.5f}; for(int k=0;k<2;k++){float subh=(0.7f+i*0.2f)*s; Cone(p,Mat(branch*0.85f),0.45f*s,subh,new Vector3(x+sx[k]*0.65f*s,0,0),default,4); Cone(p,Mat(leaf),0.25f*s,subh*0.5f,new Vector3(x+sx[k]*0.65f*s,0,0),default,4);}
                        Cone(p,Mat(leaf),0.5f*s,(0.8f+i*0.2f)*s,new Vector3(x,0,0),default,4);}
                    break;
                }
                case 2: // espiral de Fibonacci
                {
                    float phi=1.618f; for(int i=0;i<8;i++){float r=Mathf.Pow(phi,i*0.18f)*0.12f*s,a=i*137.5f*Mathf.Deg2Rad;
                        Sph(p,Mat(i%3==0?accent:i%3==1?branch:leaf),r*0.8f,new Vector3(Mathf.Cos(a)*r,r*0.3f*i*0.08f,Mathf.Sin(a)*r));
                        if(i>0){float pr=Mathf.Pow(phi,(i-1)*0.18f)*0.12f*s,pa=(i-1)*137.5f*Mathf.Deg2Rad; var conn=Prim.Cylinder(p,Mat(branch*0.8f)); conn.transform.localScale=new Vector3(0.03f,0.5f,0.03f)*s; conn.transform.localPosition=(new Vector3(Mathf.Cos(a)*r,r*0.3f*i*0.08f,Mathf.Sin(a)*r)+new Vector3(Mathf.Cos(pa)*pr,pr*0.3f*(i-1)*0.08f,Mathf.Sin(pa)*pr))*0.5f; conn.transform.LookAt(p.position+new Vector3(Mathf.Cos(a)*r,r*0.3f*i*0.08f,Mathf.Sin(a)*r)); conn.transform.Rotate(90,0,0);}}
                    break;
                }
                default: // padrão de triângulo recursivo
                {
                    void Tri(Transform tp, Vector3 c, float size2, int depth2){
                        if(depth2<=0) return;
                        Cone(tp,Mat(depth2%2==0?branch:leaf),size2*0.5f,size2*1.5f,c,default,3);
                        float os=size2*0.55f;
                        Tri(tp,c+new Vector3(-os,size2*1.5f+0.1f,0),size2*0.5f,depth2-1);
                        Tri(tp,c+new Vector3(os,0.1f,0),size2*0.5f,depth2-1);
                        Tri(tp,c+new Vector3(0,0.1f,os),size2*0.5f,depth2-1);}
                    Tri(p, Vector3.zero, 0.55f*s, 3);
                    break;
                }
            }
        }
        static void FractalBranch(Transform p, Vector3 start, Vector3 dir, float len, int depth, Color col1, Color col2, Color accent)
        {
            if(depth<=0||len<0.05f) return;
            var end=start+dir*len;
            var br=Prim.Cylinder(p,Prim.Mat(depth>2?col1:depth>1?col2:accent)); br.transform.localScale=new Vector3(0.06f*depth*0.4f,len,0.06f*depth*0.4f); br.transform.localPosition=(start+end)*0.5f; br.transform.LookAt(p.position+end); br.transform.Rotate(90,0,0);
            float nl=len*0.72f;
            FractalBranch(p,end,Quaternion.AngleAxis(32f,Vector3.right)*dir,nl,depth-1,col1,col2,accent);
            FractalBranch(p,end,Quaternion.AngleAxis(-32f,Vector3.forward)*dir,nl,depth-1,col1,col2,accent);
        }

        // ================================================================
        //  10 — ECOSSISTEMA DE SILÍCIO
        // ================================================================
        static void Silicio(int v, Transform p, Color silicon, Color crystal, Color accent, float s)
        {
            switch (v)
            {
                case 0: // floresta de silício (4 árvores geométricas)
                {
                    for(int i=0;i<4;i++){float a=i/4f*Mathf.PI*2,rr=Random.Range(0.4f,0.8f)*s;
                        Cyl(p,Met(silicon),0.1f*s,0.8f*s,new Vector3(Mathf.Cos(a)*rr,0.8f*s,Mathf.Sin(a)*rr));
                        // diamante como copa (2 pirâmides)
                        float h=0.55f*s; Cone(p,Met(crystal),0.32f*s,h,new Vector3(Mathf.Cos(a)*rr,1.7f*s,Mathf.Sin(a)*rr),Quaternion.Euler(0,45,0));
                        Cone(p,Met(accent),0.28f*s,h*0.7f,new Vector3(Mathf.Cos(a)*rr,1.7f*s,Mathf.Sin(a)*rr),Quaternion.Euler(180,45,0));
                        // circuito no chão
                        var trace=Prim.Cylinder(p,Met(accent*0.7f)); trace.transform.localScale=new Vector3(0.04f,rr,0.04f)*s; trace.transform.localPosition=new Vector3(Mathf.Cos(a)*rr*0.5f,0.04f*s,Mathf.Sin(a)*rr*0.5f); trace.transform.LookAt(p.position+new Vector3(0,0.04f,0)*s); trace.transform.Rotate(90,0,0);
                        Sph(p,Glow(accent,1.8f),0.09f*s,new Vector3(Mathf.Cos(a)*rr,0.09f*s,Mathf.Sin(a)*rr));}
                    Light(p,accent,5*s,1.4f,new Vector3(0,1.2f,0)*s);
                    break;
                }
                case 1: // wafer de silício gigante
                {
                    // plataforma hexagonal
                    Cyl(p,Met(silicon*0.9f),1.2f*s,0.15f*s,new Vector3(0,0.07f,0)*s);
                    // grade de transistores
                    for(int x2=-2;x2<=2;x2++) for(int z2=-2;z2<=2;z2++) if(Mathf.Abs(x2)+Mathf.Abs(z2)<=2){
                        Cube(p,Met((x2+z2)%2==0?silicon:crystal),new Vector3(0.22f,0.12f,0.22f)*s,new Vector3(x2*0.42f*s,0.2f*s,z2*0.42f*s));
                        if(Random.value<0.4f) Sph(p,Glow(accent,1.5f),0.06f*s,new Vector3(x2*0.42f*s,0.32f*s,z2*0.42f*s));}
                    Light(p,accent,4*s,1.2f,new Vector3(0,0.5f,0)*s);
                    break;
                }
                case 2: // geodo de silício aberto
                {
                    // carcaça exterior
                    var outer=Prim.Rock(p,Met(silicon),0.3f); outer.transform.localScale=new Vector3(1.4f,1.0f,1.4f)*s; outer.transform.localPosition=new Vector3(0,0.6f,0)*s;
                    // metade cortada
                    var inner=Prim.Rock(p,Met(crystal),0.2f); inner.transform.localScale=new Vector3(1.0f,0.8f,1.0f)*s; inner.transform.localPosition=new Vector3(0,0.7f,0.3f)*s;
                    // cristais internos (vários)
                    Color[] cc={accent,crystal,silicon*1.2f};
                    for(int i=0;i<12;i++){float a=i/12f*Mathf.PI*2,rr=Random.Range(0.15f,0.45f)*s,h=Random.Range(0.2f,0.7f)*s; Cone(p,Met(cc[i%cc.Length]),0.08f*s,h,new Vector3(Mathf.Cos(a)*rr,0.7f+h*0.5f,Mathf.Sin(a)*rr)*s);}
                    Light(p,accent,5*s,1.8f,new Vector3(0,0.9f,0)*s);
                    break;
                }
                default: // chip de processamento monumental
                {
                    // corpo do chip
                    Cube(p,Met(silicon),new Vector3(1.6f,0.22f,1.6f)*s,new Vector3(0,0.11f,0)*s);
                    // pinos de conexão
                    for(int i=0;i<8;i++){float t=(float)i/7,x2=Mathf.Lerp(-0.72f,0.72f,t)*s;
                        Cyl(p,Met(silicon*0.8f),0.04f*s,0.22f*s,new Vector3(x2,-0.18f,0.82f)*s);
                        Cyl(p,Met(silicon*0.8f),0.04f*s,0.22f*s,new Vector3(x2,-0.18f,-0.82f)*s);}
                    // camadas de circuito na superfície
                    for(int i=0;i<4;i++){Cube(p,Met(accent*(0.6f+i*0.1f)),new Vector3(1.2f-i*0.25f,0.04f,0.08f)*s,new Vector3(0,0.23f+i*0.04f,(0.4f-i*0.12f))*s);}
                    // núcleo brilhante
                    Cube(p,Glow(accent,2.0f),new Vector3(0.4f,0.28f,0.4f)*s,new Vector3(0,0.25f,0)*s);
                    Light(p,accent,6*s,2.0f,new Vector3(0,0.4f,0)*s);
                    break;
                }
            }
        }
    }
}
