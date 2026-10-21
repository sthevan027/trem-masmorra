// Cel shading simples com contorno (casco invertido). Fica em Resources para entrar nos builds.
Shader "TremMasmorra/Toon"
{
    Properties
    {
        _Color ("Cor", Color) = (1,1,1,1)
        _OutlineColor ("Cor do contorno", Color) = (0.06,0.05,0.1,1)
        _OutlineWidth ("Espessura do contorno", Float) = 0.006
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "OUTLINE"
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _OutlineWidth;
            fixed4 _OutlineColor;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // Direcao suavizada: a propria posicao (funciona para cubos, esferas e capsulas).
                float3 dir = normalize(v.vertex.xyz + float3(0.0001, 0.0001, 0.0001));
                float3 n = mul((float3x3)UNITY_MATRIX_IT_MV, dir);
                float2 off = normalize(n.xy + float2(0.0001, 0.0001)) * _OutlineWidth * o.pos.w;
                off.x *= _ScreenParams.y / _ScreenParams.x;
                o.pos.xy += off;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target { return _OutlineColor; }
            ENDCG
        }

        Pass
        {
            Name "TOON"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 pos : SV_POSITION; float3 nrm : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.nrm = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float ndl = dot(normalize(i.nrm), normalize(float3(0.4, 0.8, -0.35)));
                float band = ndl > 0.3 ? 1.0 : (ndl > -0.25 ? 0.82 : 0.64);
                return fixed4(_Color.rgb * band, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
