Shader "Custom/Chapter 1/View to Tangent HLSL"
{
    Properties
    {        
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _Offset("Offset", Range(0, 1)) = 0
        _ColorA("ColorA", Color) = (0, 0, 0, 1)
        _ColorB("ColorB", Color) = (0, 0, 0, 1)
        _Smoothness("Smoothness", Range(0, 1)) = 0
        _SpecularColor("SpecularColor", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue"="Geometry"
            "DisableBatching"="False"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalLitSubTarget"
        }

        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }        
           
            Cull Back
            Blend One Zero
            ZTest LEqual
            ZWrite On
        
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;
                float3 tangentWS : TANGENT;
                float3 bitangetWS : BITANGENT;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _ColorA;
                half4 _ColorB;
                half4 _SpecularColor;
                half _Offset;
                half _Smoothness;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);                
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);    // <-- UV Node : channel 0
                
                VertexNormalInputs vertex = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS = vertex.normalWS;         // <-- Normal Vector Node
                OUT.tangentWS = vertex.tangentWS;       // <-- Tangent Vector Node
                OUT.bitangetWS = vertex.bitangentWS;    // <-- Bitangent Vector Node
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {    
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - IN.positionWS);         // <-- View Direction Node 
                float3 lightDirectionWS = normalize(_MainLightPosition - IN.positionWS);    // <-- Main Light Direction Node
                
                float TdotV = dot(IN.tangentWS, viewDirWS);
                float BdotV = dot(IN.bitangetWS, viewDirWS);
                float NdotV = dot(IN.normalWS, viewDirWS);

                float3 viewVector = float3(TdotV, BdotV, NdotV);    // <-- View Vector Node
                float3 negatedOffset = (-1.0 * _Offset) * normalize(viewVector);
                float2 uvOffset = IN.uv + negatedOffset.xy;
                
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvOffset);  // <-- Texture 2D + Sample Texture 2D Node
                half4 baseMapReflection = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseMap.r - negatedOffset);
                half4 color = lerp(_ColorA, _ColorB, baseMapReflection.r);

                half3 specular = LightingSpecular(
                    _SpecularColor,
                    lightDirectionWS,
                    IN.normalWS,
                    viewDirWS,
                    1.0,
                    32.0 * _Smoothness + 0.0001);
                
                color.rgb += specular; // <-- Render                
                return  color;
            }
            ENDHLSL
        }
    }
}
