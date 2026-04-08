Shader "Custom/LiquidHealthBar"
{
    Properties
    {
        _MainTex        ("Sprite Texture", 2D)      = "white" {}
        _FillAmount     ("Fill Amount",    Range(0,1)) = 1.0
        _WaveSpeed      ("Wave Speed",     Float)   = 2.0
        _WaveAmplitude  ("Wave Amplitude", Range(0, 0.05)) = 0.02
        _WaveFrequency  ("Wave Frequency", Float)   = 8.0
        _LiquidColor    ("Liquid Color",   Color)   = (0.2, 0.8, 0.3, 1)
        _FoamColor      ("Foam Color",     Color)   = (0.9, 1.0, 0.9, 1)
        _EmptyColor     ("Empty Color",    Color)   = (0.1, 0.1, 0.1, 0.5)
        _FoamThickness  ("Foam Thickness", Range(0, 0.05)) = 0.015
        _PixelSize      ("Pixel Snap Size", Float)  = 4.0
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "RenderType"      = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;

            float  _FillAmount;
            float  _WaveSpeed;
            float  _WaveAmplitude;
            float  _WaveFrequency;
            float4 _LiquidColor;
            float4 _FoamColor;
            float4 _EmptyColor;
            float  _FoamThickness;
            float  _PixelSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // --- Pixel snapping (chunky pixel art look) ---
                float2 snappedUV = floor(uv * _PixelSize) / _PixelSize;

                // --- Wave on the vertical edge, fills left to right ---
                float wave = sin((snappedUV.y * _WaveFrequency)
                               + (_Time.y * _WaveSpeed))
                             * _WaveAmplitude;

                wave += sin((snappedUV.y * _WaveFrequency * 1.7)
                           + (_Time.y * _WaveSpeed * 0.8 + 1.5))
                        * (_WaveAmplitude * 0.4);

                float fillEdge = _FillAmount + wave;

                // --- Sprite mask ---
                fixed4 spriteSample = tex2D(_MainTex, uv);
                clip(spriteSample.a - 0.01);

                // --- Classify pixel (X axis for left-to-right fill) ---
                bool inLiquid = uv.x < fillEdge;
                bool inFoam   = uv.x >= fillEdge &&
                                uv.x <  (fillEdge + _FoamThickness);

                fixed4 color;

                if (inFoam)
                {
                    float foamT = (uv.x - fillEdge) / _FoamThickness;
                    color = lerp(_FoamColor, _EmptyColor, foamT);
                }
                else if (inLiquid)
                {
                    // Subtle brightness gradient — slightly darker toward right
                    float depth = (_FillAmount - uv.x) / max(_FillAmount, 0.001);
                    color = _LiquidColor * lerp(1.0, 0.75, depth * 0.5);
                }
                else
                {
                    color = _EmptyColor;
                }

                color.a *= spriteSample.a;
                return color;
            }
            ENDCG
        }
    }
}