Shader "Custom/Parallax2D6Layer_URP"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
        _WorldSpeed ("World Speed (units/s)", Float) = 6

        [Header(Layer1)]
        _LayerTex1 ("Layer 1", 2D) = "white" {}
        _Speed1 ("Speed 1", Float) = 0.1
        _Tiling1 ("Tiling 1", Vector) = (1,1,0,0)
        _Offset1 ("Offset 1", Vector) = (0,0,0,0)
        _Alpha1 ("Alpha 1", Range(0,1)) = 1
        _Tint1 ("Tint 1", Color) = (1,1,1,1)

        [Header(Layer2)]
        _LayerTex2 ("Layer 2", 2D) = "white" {}
        _Speed2 ("Speed 2", Float) = 0.2
        _Tiling2 ("Tiling 2", Vector) = (1,1,0,0)
        _Offset2 ("Offset 2", Vector) = (0,0,0,0)
        _Alpha2 ("Alpha 2", Range(0,1)) = 1
        _Tint2 ("Tint 2", Color) = (1,1,1,1)

        [Header(Layer3)]
        _LayerTex3 ("Layer 3", 2D) = "white" {}
        _Speed3 ("Speed 3", Float) = 0.35
        _Tiling3 ("Tiling 3", Vector) = (1,1,0,0)
        _Offset3 ("Offset 3", Vector) = (0,0,0,0)
        _Alpha3 ("Alpha 3", Range(0,1)) = 1
        _Tint3 ("Tint 3", Color) = (1,1,1,1)

        [Header(Layer4)]
        _LayerTex4 ("Layer 4", 2D) = "white" {}
        _Speed4 ("Speed 4", Float) = 0.5
        _Tiling4 ("Tiling 4", Vector) = (1,1,0,0)
        _Offset4 ("Offset 4", Vector) = (0,0,0,0)
        _Alpha4 ("Alpha 4", Range(0,1)) = 1
        _Tint4 ("Tint 4", Color) = (1,1,1,1)

        [Header(Layer5)]
        _LayerTex5 ("Layer 5", 2D) = "white" {}
        _Speed5 ("Speed 5", Float) = 0.7
        _Tiling5 ("Tiling 5", Vector) = (1,1,0,0)
        _Offset5 ("Offset 5", Vector) = (0,0,0,0)
        _Alpha5 ("Alpha 5", Range(0,1)) = 1
        _Tint5 ("Tint 5", Color) = (1,1,1,1)

        [Header(Layer6)]
        _LayerTex6 ("Layer 6", 2D) = "white" {}
        _Speed6 ("Speed 6", Float) = 0.9
        _Tiling6 ("Tiling 6", Vector) = (1,1,0,0)
        _Offset6 ("Offset 6", Vector) = (0,0,0,0)
        _Alpha6 ("Alpha 6", Range(0,1)) = 1
        _Tint6 ("Tint 6", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            Name "FORWARD_UNLIT"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Tint;
                float _WorldSpeed;

                float4 _Tiling1, _Offset1, _Tint1; float _Speed1, _Alpha1;
                float4 _Tiling2, _Offset2, _Tint2; float _Speed2, _Alpha2;
                float4 _Tiling3, _Offset3, _Tint3; float _Speed3, _Alpha3;
                float4 _Tiling4, _Offset4, _Tint4; float _Speed4, _Alpha4;
                float4 _Tiling5, _Offset5, _Tint5; float _Speed5, _Alpha5;
                float4 _Tiling6, _Offset6, _Tint6; float _Speed6, _Alpha6;
            CBUFFER_END

            TEXTURE2D(_LayerTex1); SAMPLER(sampler_LayerTex1);
            TEXTURE2D(_LayerTex2); SAMPLER(sampler_LayerTex2);
            TEXTURE2D(_LayerTex3); SAMPLER(sampler_LayerTex3);
            TEXTURE2D(_LayerTex4); SAMPLER(sampler_LayerTex4);
            TEXTURE2D(_LayerTex5); SAMPLER(sampler_LayerTex5);
            TEXTURE2D(_LayerTex6); SAMPLER(sampler_LayerTex6);

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            float4 SampleLayer(TEXTURE2D_PARAM(tex, samp), float2 uv, float4 tiling, float4 offs, float speed, float alpha, float4 tint)
            {
                float timeShift = -_Time.y * _WorldSpeed * speed; // negativo => izquierda
                float2 baseUV = uv * tiling.xy + offs.xy;
                // Envuelve el eje X para evitar estiramiento por Clamp/precisión
                float x = baseUV.x + timeShift;
                x = frac(x); // mantiene 0..1
                float2 uvShift = float2(x, baseUV.y);
                float4 c = SAMPLE_TEXTURE2D(tex, samp, uvShift) * tint;
                c.a *= alpha;
                c.rgb *= c.a; // pre-mult
                return c;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float4 acc = float4(0,0,0,0);
                // De fondo a frente
                acc = acc + SampleLayer(_LayerTex1, sampler_LayerTex1, IN.uv, _Tiling1, _Offset1, _Speed1, _Alpha1, _Tint1);
                acc = acc + (1 - acc.a) * SampleLayer(_LayerTex2, sampler_LayerTex2, IN.uv, _Tiling2, _Offset2, _Speed2, _Alpha2, _Tint2);
                acc = acc + (1 - acc.a) * SampleLayer(_LayerTex3, sampler_LayerTex3, IN.uv, _Tiling3, _Offset3, _Speed3, _Alpha3, _Tint3);
                acc = acc + (1 - acc.a) * SampleLayer(_LayerTex4, sampler_LayerTex4, IN.uv, _Tiling4, _Offset4, _Speed4, _Alpha4, _Tint4);
                acc = acc + (1 - acc.a) * SampleLayer(_LayerTex5, sampler_LayerTex5, IN.uv, _Tiling5, _Offset5, _Speed5, _Alpha5, _Tint5);
                acc = acc + (1 - acc.a) * SampleLayer(_LayerTex6, sampler_LayerTex6, IN.uv, _Tiling6, _Offset6, _Speed6, _Alpha6, _Tint6);

                // Des-pre-multiplicar
                float3 rgb = acc.a > 1e-5 ? acc.rgb / acc.a : acc.rgb;
                float4 outCol = float4(rgb, acc.a) * _Tint;
                return outCol;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
