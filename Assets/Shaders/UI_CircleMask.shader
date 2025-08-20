Shader "Unlit/UI_CircleMask_Fixed"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color   ("Tint", Color) = (1,1,1,1)
        _Radius  ("Radius (0-0.5)", Range(0.0, 0.5)) = 0.5
        _Feather ("Edge Feather (px)", Range(0,10)) = 1
        _RectSize("Rect Size (px)", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        Pass
        {
            Name "CircleMaskUI"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
                float2 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float  _Radius;
            float  _Feather;
            float4 _RectSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                o.col = v.color * _Color;
                o.worldPos = v.vertex.xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Get the base texture color
                fixed4 col = tex2D(_MainTex, i.uv) * i.col;
                
                // Center UV coordinates around (0,0)
                float2 center = float2(0.5, 0.5);
                float2 uv = i.uv - center;
                
                // Handle aspect ratio correction
                float2 rectSize = _RectSize.xy;
                
                // If RectSize not set, assume square (fallback)
                if (rectSize.x <= 0 || rectSize.y <= 0) {
                    rectSize = float2(1, 1);
                }
                
                // Calculate aspect ratio - make circle fit in shorter dimension
                float aspect = rectSize.x / rectSize.y;
                
                // Scale UV to make perfect circle
                if (aspect > 1.0) {
                    // Wide rectangle - scale X down
                    uv.x *= aspect;
                } else {
                    // Tall rectangle - scale Y down  
                    uv.y /= aspect;
                }
                
                // Distance from center
                float dist = length(uv);
                
                // Convert feather from pixels to UV space
                float minDim = min(rectSize.x, rectSize.y);
                float featherUV = (_Feather / minDim) * 0.5;
                
                // Create circular mask with smooth edge
                float mask = smoothstep(_Radius + featherUV, _Radius - featherUV, dist);
                
                // Apply mask to alpha
                col.a *= mask;
                
                // Optional: hard clip for better performance
                clip(col.a - 0.001);
                
                return col;
            }
            ENDCG
        }
    }
}