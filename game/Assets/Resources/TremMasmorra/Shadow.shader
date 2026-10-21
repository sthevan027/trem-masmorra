// Sombra de bolha sob personagens: disco escuro e translucido, testado contra a profundidade.
Shader "TremMasmorra/Shadow"
{
    Properties { _Color ("Cor", Color) = (0,0,0,0.35) }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Geometry+1" "IgnoreProjector"="True" }
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Offset -1, -1
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); return o; }
            fixed4 frag (v2f i) : SV_Target { return _Color; }
            ENDCG
        }
    }
    Fallback Off
}
