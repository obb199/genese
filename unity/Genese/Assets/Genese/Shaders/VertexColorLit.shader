// Terreno: cor por vértice (mistura suave entre biomas) iluminada pelo sol (Built-in RP).
Shader "Genese/VertexColorLit"
{
    Properties { _Glossiness ("Smoothness", Range(0,1)) = 0.0 }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert
        #pragma target 3.0

        struct Input { float4 vcolor : COLOR; };

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vcolor = v.color;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = IN.vcolor.rgb;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
