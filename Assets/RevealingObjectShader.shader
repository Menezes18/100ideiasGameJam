Shader "Custom/RevealingObjectShader"
{
    Properties
    {
        _RevealingColor ("Revealing Color", Color) = (1,1,1,1)
        _Tolerance ("Color Tolerance", Range(0.0, 1.0)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="UniversalForward" }
        LOD 200

        CGPROGRAM
        #pragma surface surf RevealingLighting
        
        #pragma multi_compile_fwdbase
        #include "UnityCG.cginc"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        sampler2D _MainTex;
        fixed4 _RevealingColor;
        half _Tolerance;

        struct Input
        {
            float2 uv_MainTex;
        };

        // Função principal do Surface Shader, define as propriedades da superfície.
        void surf (Input IN, inout SurfaceOutput o)
        {
            // A cor do objeto (mesmo quando invisível) é a cor que será revelada
            o.Albedo = _RevealingColor.rgb;
            o.Alpha = _RevealingColor.a;
        }

        // Função de iluminação personalizada
        half4 RevealingLighting (SurfaceOutput s, half3 lightDir, half atten) 
        {
            // Obtém a cor da luz principal, já considerando a atenuação do URP.
            fixed3 lightColor = _LightColor0.rgb;

            // Compara a cor da luz com a cor do objeto (Albedo)
            fixed3 diff = abs(s.Albedo - lightColor.rgb);
            
            if (dot(diff, diff) < _Tolerance)
            {
                // Se as cores forem semelhantes, retorne a cor revelada (cor do objeto * cor da luz * atenuação)
                return fixed4(s.Albedo * lightColor * atten, s.Alpha);
            }
            else
            {
                // Se as cores forem diferentes, retorne preto com alpha 0 para tornar invisível
                return fixed4(0, 0, 0, 0); 
            }
        }
        ENDCG
    }
    FallBack Off
}
