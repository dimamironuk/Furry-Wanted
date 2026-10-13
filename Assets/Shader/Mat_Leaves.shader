Shader "Custom/FoliageWindSway"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _NightColor ("Night Multiplier", Color) = (1,1,1,1)

        _WindSpeed ("Wind Speed", Range(0, 10)) = 2
        _WindStrength ("Wind Strength", Range(0, 1)) = 0.15
        _WindScale ("Wind Noise Scale", Range(0, 1)) = 1
        _TopFactor ("Sway From Bottom (0) To Top (1)", Range(0, 1)) = 1
        _GustSpeed ("Gust Speed", Range(0, 5)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _NightColor;
                float _WindSpeed;
                float _WindStrength;
                float _WindScale;
                float _TopFactor;
                float _GustSpeed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 worldPos = TransformObjectToWorld(input.positionOS);

                float heightMask = pow(saturate(input.uv.y), lerp(3.0, 1.0, _TopFactor));
                float wave = sin(_Time.y * _WindSpeed + worldPos.x * _WindScale);
                float gust = 0.5 + 0.5 * sin(_Time.y * _GustSpeed + worldPos.y * 0.5);
                float offsetX = wave * _WindStrength * heightMask * gust;

                input.positionOS.x += offsetX;

                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color * _NightColor;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                return texColor * input.color;
            }
            ENDHLSL
        }
    }
}