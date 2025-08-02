Shader "Unlit/SpotlightReveal"
{
    Properties
    {
        [HDR]_Color("Light Color", Color) = (1,1,1,1)
        _BaseColor("Base Color", Color) = (0.1,0.1,0.1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _LightWorldPos("Light World Position", Vector) = (0,0,0,0)
        _LightDirection("Light Direction", Vector) = (0,0,1,0)
        _LightRadius("Light Influence Radius", Float) = 2.0
        _LightIntensity("Light Intensity", Range(0,1)) = 0.0
        _SpotAngle("Spotlight Angle", Float) = 30.0
        _FalloffPower("Distance Falloff", Range(0.1, 5.0)) = 1.0
        _ConeFalloffPower("Cone Edge Falloff", Range(0.1, 5.0)) = 2.0
        _RevealSpeed("Reveal Speed", Range(0.1, 5.0)) = 1.0
       
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
   
    CGINCLUDE
    #include "UnityCG.cginc"
 
    half4 _Color;
    half4 _BaseColor;
    sampler2D _MainTex;
    float4 _MainTex_ST;
    float4 _LightWorldPos;
    float4 _LightDirection;
    float _LightRadius;
    float _LightIntensity;
    float _SpotAngle;
    float _FalloffPower;
    float _ConeFalloffPower;
    float _RevealSpeed;
   
    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };
 
    struct v2f
    {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
        float3 worldPos : TEXCOORD1;
    };
 
    v2f vert (appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        return o;
    }
   
    half4 frag (v2f i) : SV_Target
    {
        half4 texColor = tex2D(_MainTex, i.uv);
        
        // Calcula vetores da luz para o pixel
        float3 lightToPixel = i.worldPos - _LightWorldPos.xyz;
        float distance = length(lightToPixel);
        float3 lightToPixelNorm = lightToPixel / distance;
        
        // Calcula o ângulo em relação à direção da spotlight
        float3 lightDir = normalize(_LightDirection.xyz);
        float angleFromCenter = acos(dot(-lightToPixelNorm, lightDir)) * 57.2958; // radianos para graus
        float halfSpotAngle = _SpotAngle * 0.5;
        
        // Influência baseada no cone da spotlight
        float coneInfluence = 1.0 - saturate(angleFromCenter / halfSpotAngle);
        coneInfluence = pow(coneInfluence, _ConeFalloffPower);
        
        // Influência baseada na distância radial do centro do cone
        float3 projectedPos = _LightWorldPos.xyz + lightDir * dot(lightToPixel, -lightDir);
        float radialDistance = length(i.worldPos - projectedPos);
        float radialInfluence = 1.0 - saturate(radialDistance / _LightRadius);
        radialInfluence = pow(radialInfluence, _FalloffPower);
        
        // Influência baseada na distância da luz
        float distanceInfluence = 1.0 - saturate(distance / 20.0); // máxima distância de influência
        distanceInfluence = pow(distanceInfluence, _FalloffPower);
        
        // Combina todas as influências
        float totalInfluence = coneInfluence * radialInfluence * distanceInfluence * _LightIntensity;
        
        // Aplica uma curva de revelação mais suave
        totalInfluence = smoothstep(0.0, 1.0, totalInfluence);
        
        // Interpola entre a cor base e a cor da luz
        half4 finalColor = lerp(_BaseColor, _Color, totalInfluence);
        
        return texColor * finalColor;
    }
    
    struct v2fShadow {
        V2F_SHADOW_CASTER;
        UNITY_VERTEX_OUTPUT_STEREO
    };
 
    v2fShadow vertShadow( appdata_base v )
    {
        v2fShadow o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
        return o;
    }
 
    float4 fragShadow( v2fShadow i ) : SV_Target
    {
        SHADOW_CASTER_FRAGMENT(i)
    }
   
    ENDCG
       
    SubShader
    {
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
       
        Pass
        {
            Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
            LOD 100
            Cull [_Culling]
            Offset [_Offset], [_Offset]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            ColorMask [_ColorMask]
           
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
       
        // Pass to render object as a shadow caster
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            LOD 80
            Cull [_Culling]
            Offset [_Offset], [_Offset]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
           
            CGPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma target 2.0
            #pragma multi_compile_shadowcaster
            ENDCG
        }
    }
}