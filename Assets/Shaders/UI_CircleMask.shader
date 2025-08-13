Shader "Unlit/UI_CircleMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color   ("Tint", Color) = (1,1,1,1)

        // 0.5 means a perfect circle inscribed in the rect (using the shorter side).
        _Radius  ("Radius (0-0.5)", Range(0.1, 0.5)) = 0.5

        // Edge softening in pixels (use 0 for a hard edge).
        _Feather ("Edge Feather (px)", Range(0,10)) = 1

        // Optional: set from script to the RectTransform's size in pixels (x = width, y = height).
        // If left at (0,0), the shader falls back to screen size.
        _RectSize("Rect Size (px)", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
            "CanvasEnable"="True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "CircleMaskUI"
            Tags { "LightMode"="Always" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float4 col : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            float  _Radius;   // 0..0.5
            float  _Feather;  // pixels
            float4 _RectSize; // x = width, y = height (optional)

            // Unity provides _ScreenParams (x = width, y = height, z = 1+1/width, w = 1+1/height)

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                o.col = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Center UV around (0.5,0.5)
                float2 uv = i.uv;
                float2 p  = uv - 0.5;

                // Determine pixel size basis:
                // if _RectSize set (both > 0), use it; otherwise use screen size.
                float2 sizePx = _RectSize.xy;
                if (sizePx.x <= 0.0 || sizePx.y <= 0.0)
                    sizePx = _ScreenParams.xy;

                // Keep circle round in non-square rects: scale by the shorter side.
                float  minAxis = max(min(sizePx.x, sizePx.y), 1.0);
                float2 scale   = sizePx / minAxis;
                p *= scale;

                // Distance from center (in normalized, aspect-corrected UV space)
                float dist = length(p);

                // Convert Feather from pixels to "UV-on-short-side"
                float featherUV = _Feather / minAxis;

                // Smooth edge alpha mask
                float alphaMask = smoothstep(_Radius, _Radius - featherUV, dist);

                fixed4 col = tex2D(_MainTex, uv) * i.col;
                col.a *= alphaMask;

                // Hard discard outside (better for correct hit-testing visuals and blending)
                clip(col.a - 0.001);

                return col;
            }
            ENDCG
        }
    }
}
