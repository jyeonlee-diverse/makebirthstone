Shader "Custom/Chapter 1/Keyword Boolean"
{
    Properties
    {
        [Header(Color Properties)]
        _ColorB("ColorB", Color) = (0, 0, 0, 1)
        _ColorA("ColorA", Color) = (0, 0, 0, 1)
        [Header(Keyword)]
        [Toggle(_BOOLEAN)]_BOOLEAN("Boolean", Float) = 0        
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
            #pragma shader_feature_local_fragment _ _BOOLEAN

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
                half4 _ColorB;
                float4 _ColorA;
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
                half4 color = (0.0, 0.0, 0.0, 1.0);

                #if defined(_BOOLEAN)
                color = _ColorA;
                #else
                color = _ColorB;
                #endif
                
                return color;
            }
            ENDHLSL
        }
    }
}
