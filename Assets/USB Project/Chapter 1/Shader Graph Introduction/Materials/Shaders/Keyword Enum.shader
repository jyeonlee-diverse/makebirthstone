Shader "Custom/Chapter 1/Keyword Enum"
{
    Properties
    {
        _ColorA("ColorA", Color) = (0, 0, 0, 1)
        _ColorB("ColorB", Color) = (0, 0, 0, 1)        
        _ColorC("ColorC", Color) = (0, 0, 0, 1)
        [KeywordEnum(A, B, C)]_ENUM("Enum", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            // Keywords
            #pragma shader_feature_local _ENUM_A _ENUM_B _ENUM_C

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            CBUFFER_START(UnityPerMaterial)
                half4 _ColorA;
                half4 _ColorB;
                half4 _ColorC;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = 0.0;

                #if defined(_ENUM_A)
                color = _ColorA;
                #elif defined(_ENUM_B)
                color = _ColorB;
                #else
                color = _ColorC;
                #endif    
                
                return color;
            }
            ENDHLSL
        }
    }
}
