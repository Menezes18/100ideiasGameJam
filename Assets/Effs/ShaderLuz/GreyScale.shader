Shader "URP/Custom/GreyScaleRamp"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _RampTex("Ramp Texture", 2D) = "gray" {}
        _RampOffset("Ramp Offset", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        ZTest Always
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_RampTex);
            SAMPLER(sampler_RampTex);

            float _RampOffset;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float3 Luminance(float3 rgb)
            {
                return dot(rgb, float3(0.299, 0.587, 0.114));
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float grayscale = Luminance(original.rgb);
                float2 remap = float2(grayscale + _RampOffset, 0.5);
                float4 rampColor = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, remap);
                float4 output = original * (1.0 - original.a) + rampColor * original.a;
                output.a = original.a;
                return output;
            }
            ENDHLSL
        }
    }
}
