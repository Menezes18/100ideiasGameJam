Shader "Hidden/ScreenFade"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)
        _Alpha ("Alpha", Range(0,1)) = 1
    }
    SubShader
    {
        // Garante que desenha por cima de tudo
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            // Vertex + Fragment básicos
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // variáveis mapeadas das Properties
            float4 _Color;
            float _Alpha;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // transforma vértice diretamente em clip-space
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // devolve a cor preta * alpha
                return _Color * _Alpha;
            }
            ENDHLSL
        }
    }
}
