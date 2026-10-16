Shader "Custom/Xray_Shader"
{
    Properties
    {
        _BaseColor ("Color", Color) = (0, 1, 0, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent+100" }

        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float4 _BaseColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }

            float4 frag (v2f i) : SV_TARGET
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}