Shader "Custom/LitErode"
{
    Properties
    {
        [Header(Main Properties)]
        [HDR]_Color("Albedo", Color) = (1,1,1,1)
        _MainTex ("Albedo Texture", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        [Header(Erode Properties)]
        _ErodeTexture ("Erode Texture (Grayscale)", 2D) = "white" {}
        _ErodeValue ("Erode Amount", Range(0, 1)) = 0
        _ErodeEdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
        _ErodeEdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1)
        [HDR] _ErodeEdgeEmission ("Edge Emission", Color) = (2, 1, 0, 1)
        _ErodeNoiseScale ("Noise Scale", Range(0.1, 10)) = 1
        
        [Header(Advanced Erode)]
        _ErodeDirection ("Erode Direction", Vector) = (0, -1, 0, 0)
        _ErodeDirectionInfluence ("Direction Influence", Range(0, 1)) = 0
        
        [Header(Stencil)]
        _Stencil ("Stencil ID [0;255]", Float) = 0
        _ReadMask ("ReadMask [0;255]", Int) = 255
        _WriteMask ("WriteMask [0;255]", Int) = 255
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail", Int) = 0
        
        [Header(Rendering)]
        _Offset("Offset", float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Culling ("Cull Mode", Int) = 2
        [Enum(Off,0,On,1)] _ZWrite("ZWrite", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4
        [Enum(None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15)] _ColorMask("Color Mask", Int) = 15
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        
        Stencil
        {
            Ref [_Stencil]
            ReadMask [_ReadMask]
            WriteMask [_WriteMask]
            Comp [_StencilComp]
            Pass [_StencilOp]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
        }
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _ErodeTexture;
        
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_ErodeTexture;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _ErodeValue;
        float _ErodeEdgeWidth;
        fixed4 _ErodeEdgeColor;
        fixed4 _ErodeEdgeEmission;
        float _ErodeNoiseScale;
        float4 _ErodeDirection;
        float _ErodeDirectionInfluence;

        // Função para gerar ruído simples
        float random(float2 st)
        {
            return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
        }
        
        // Função para ruído suave
        float noise(float2 st)
        {
            float2 i = floor(st);
            float2 f = frac(st);
            
            float a = random(i);
            float b = random(i + float2(1.0, 0.0));
            float c = random(i + float2(0.0, 1.0));
            float d = random(i + float2(1.0, 1.0));
            
            float2 u = f * f * (3.0 - 2.0 * f);
            
            return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
        }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Obter valor da textura de erosão
            float erodeTexValue = tex2D(_ErodeTexture, IN.uv_ErodeTexture).r;
            
            // Adicionar ruído procedural para variação
            float proceduralNoise = noise(IN.uv_MainTex * _ErodeNoiseScale);
            erodeTexValue = lerp(erodeTexValue, proceduralNoise, 0.3);
            
            // Aplicar influência direcional (opcional)
            if (_ErodeDirectionInfluence > 0)
            {
                float3 worldNormal = WorldNormalVector(IN, float3(0, 0, 1));
                float directionFactor = dot(worldNormal, normalize(_ErodeDirection.xyz));
                directionFactor = (directionFactor + 1) * 0.5; // Normalizar para 0-1
                erodeTexValue = lerp(erodeTexValue, erodeTexValue * directionFactor, _ErodeDirectionInfluence);
            }
            
            // Calcular limiar de erosão
            float erodeThreshold = _ErodeValue;
            float edgeThreshold = erodeThreshold + _ErodeEdgeWidth;
            
            // Descartar pixels completamente erodidos
            clip(erodeTexValue - erodeThreshold);
            
            // Calcular cor base
            fixed4 albedo = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            // Calcular efeito da borda
            float edgeFactor = 0;
            if (erodeTexValue < edgeThreshold && _ErodeEdgeWidth > 0)
            {
                edgeFactor = 1.0 - saturate((erodeTexValue - erodeThreshold) / _ErodeEdgeWidth);
                albedo = lerp(albedo, _ErodeEdgeColor, edgeFactor);
            }
            
            // Aplicar propriedades do material
            o.Albedo = albedo.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = albedo.a;
            
            // Adicionar emissão na borda
            o.Emission = _ErodeEdgeEmission.rgb * edgeFactor;
        }
        ENDCG
        
        // Pass customizado para sombras com erosão
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            Cull [_Culling]
            Offset [_Offset], [_Offset]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            
            CGPROGRAM
            #pragma vertex vert_shadow
            #pragma fragment frag_shadow
            #pragma target 3.0
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            
            sampler2D _ErodeTexture;
            float _ErodeValue;
            float _ErodeNoiseScale;
            
            struct v2f_shadow
            {
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD1;
            };
            
            // Função de ruído para sombras (simplificada)
            float random_shadow(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            float noise_shadow(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);
                
                float a = random_shadow(i);
                float b = random_shadow(i + float2(1.0, 0.0));
                float c = random_shadow(i + float2(0.0, 1.0));
                float d = random_shadow(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            
            v2f_shadow vert_shadow(appdata_base v)
            {
                v2f_shadow o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.uv = v.texcoord;
                return o;
            }
            
            float4 frag_shadow(v2f_shadow i) : SV_Target
            {
                // Aplicar mesma lógica de erosão nas sombras
                float erodeTexValue = tex2D(_ErodeTexture, i.uv).r;
                float proceduralNoise = noise_shadow(i.uv * _ErodeNoiseScale);
                erodeTexValue = lerp(erodeTexValue, proceduralNoise, 0.3);
                
                clip(erodeTexValue - _ErodeValue);
                
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}