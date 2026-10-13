Shader "UI/PixelDissolve_URP_UI_Directional"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _FadeAmount ("Fade Amount (1=Game Visible, 0=Black)", Range(0,1)) = 1.0
        _FadeColor ("Fade Color", Color) = (0,0,0,1)
        _PixelSize ("Pixel Size / Resolution Divider", Float) = 4.0

        _EdgeWidth ("Edge Width", Range(0.01, 0.6)) = 0.18

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "PixelDissolveUIDirectional"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _FadeAmount;
            fixed4 _FadeColor;
            float _PixelSize;
            float _EdgeWidth;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.screenPos = ComputeScreenPos(OUT.vertex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            // Hardcoded 4x4 Bayer ordered-dithering matrix (values 0..15)
            static const int bayer4x4[16] =
            {
                 0,  8,  2, 10,
                12,  4, 14,  6,
                 3, 11,  1,  9,
                15,  7, 13,  5
            };

            float BayerThreshold(float2 screenPixel)
            {
                int x = (int)fmod(abs(screenPixel.x), 4.0);
                int y = (int)fmod(abs(screenPixel.y), 4.0);
                int index = y * 4 + x;
                return (bayer4x4[index] + 0.5) / 16.0;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 rawScreenUV = IN.screenPos.xy / IN.screenPos.w;

                float2 screenPixelCoord = rawScreenUV * _ScreenParams.xy;
                float2 pixelated = floor(screenPixelCoord / max(_PixelSize, 1.0));
                float threshold = BayerThreshold(pixelated);

                // wipeCoord = 0 на правому краю екрану, = 1 на л≥вому.
                float wipeCoord = 1.0 - rawScreenUV.x;

                // «суваЇмо прогрес так, щоб при _FadeAmount = 1.0 фронт повн≥стю виходив за меж≥ екрана (-_EdgeWidth)
                float adjustedProgress = lerp(1.0, -_EdgeWidth, _FadeAmount);

                float dist = wipeCoord - adjustedProgress;

                float alpha;

                if (dist <= 0.0)
                {
                    // ¬же пройдена зона Ч суц≥льний чорний кол≥р
                    alpha = 1.0;
                }
                else if (dist >= _EdgeWidth)
                {
                    // ўе не д≥йшло Ч повн≥стю прозоро (видно гру)
                    alpha = 0.0;
                }
                else
                {
                    // «она фронту переходу
                    float localProgress = 1.0 - saturate(dist / _EdgeWidth);
                    alpha = step(threshold, localProgress);
                }

                fixed4 finalColor = _FadeColor;
                finalColor.a *= alpha;

                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                #ifdef UNITY_UI_ALPHACLIP
                clip(finalColor.a - 0.001);
                #endif

                return finalColor;
            }
            ENDHLSL
        }
    }
}