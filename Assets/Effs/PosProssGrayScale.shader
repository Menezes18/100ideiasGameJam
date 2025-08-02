Shader "Custom/PosProssGrayScale"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Luminosity ("Luminosity", Float) = 1.0
        _Enabled ("Enabled", Float) = 1.0 // Propriedade para controlar o efeito
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Luminosity;
            float _Enabled; // Variável para o shader

            // Função que gera um ruído aleatório. Não é usada na sua lógica, mas mantida no código.
            float rand(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // A única definição da função frag
            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 renderTex = tex2D(_MainTex, i.uv);
                
                // Se o efeito não estiver habilitado, retorne a cor original
                if (_Enabled < 0.5)
                {
                    return renderTex;
                }

                // Calcula a luminosidade para a escala de cinza
                float luminosity = 0.299 * renderTex.r + 0.587 * renderTex.g + 0.114 * renderTex.b;
                
                // Interpola entre a cor original e a escala de cinza
                fixed4 finalColor = lerp(renderTex, fixed4(luminosity, luminosity, luminosity, renderTex.a), _Luminosity);

                return finalColor;
            }
            ENDCG
        }
    }
    FallBack Off // 'FallBack Off' é o correto para shaders de pós-processamento
}
