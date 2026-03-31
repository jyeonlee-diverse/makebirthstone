Shader "Birthstone/PearlShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.95, 0.92, 0.88, 1)
        _IridescenceColor1 ("Iridescence Color 1", Color) = (1.0, 0.85, 0.9, 1)
        _IridescenceColor2 ("Iridescence Color 2", Color) = (0.85, 0.95, 1.0, 1)
        _IridescenceColor3 ("Iridescence Color 3", Color) = (0.9, 1.0, 0.85, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.9
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 4.0
        _IridescenceStrength ("Iridescence Strength", Range(0, 1)) = 0.4
        _LusterIntensity ("Luster Intensity", Range(0, 3)) = 1.5
        _SubsurfaceColor ("Subsurface Color", Color) = (1.0, 0.95, 0.9, 1)
        _SubsurfaceStrength ("Subsurface Strength", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "PearlForward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _IridescenceColor1;
                float4 _IridescenceColor2;
                float4 _IridescenceColor3;
                float _Smoothness;
                float _FresnelPower;
                float _IridescenceStrength;
                float _LusterIntensity;
                float4 _SubsurfaceColor;
                float _SubsurfaceStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 N = normalize(input.normalWS);
                float3 V = normalize(input.viewDirWS);
                float NdotV = saturate(dot(N, V));

                // Fresnel
                float fresnel = pow(1.0 - NdotV, _FresnelPower);

                // Iridescence
                float3 iri = lerp(_IridescenceColor1.rgb, _IridescenceColor2.rgb,
                                  sin(NdotV * 3.14159 * 2.0) * 0.5 + 0.5);
                iri = lerp(iri, _IridescenceColor3.rgb,
                           sin(NdotV * 3.14159 * 3.0 + 1.0) * 0.5 + 0.5);

                // Main light
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                float3 L = normalize(mainLight.direction);
                float3 H = normalize(V + L);
                float NdotL = saturate(dot(N, L));
                float NdotH = saturate(dot(N, H));

                // Luster
                float roughness = 1.0 - _Smoothness;
                float r2 = max(roughness * roughness, 0.001);
                float luster = pow(NdotH, 1.0 / r2) * _LusterIntensity;

                // SSS
                float sss = saturate(dot(-N, L)) * _SubsurfaceStrength;
                float3 subsurface = sss * _SubsurfaceColor.rgb * mainLight.color;

                // Compose
                float3 base = _BaseColor.rgb;
                float3 lc = mainLight.color * mainLight.shadowAttenuation;
                float3 color = base * NdotL * lc
                             + luster * lc * 0.4
                             + iri * _IridescenceStrength * (fresnel + 0.2)
                             + subsurface
                             + base * 0.15
                             + fresnel * base * 0.3;

                #ifdef _ADDITIONAL_LIGHTS
                uint lightCount = GetAdditionalLightsCount();
                for (uint li = 0u; li < lightCount; ++li)
                {
                    Light al = GetAdditionalLight(li, input.positionWS);
                    float3 aL = normalize(al.direction);
                    float3 aH = normalize(V + aL);
                    float aNL = saturate(dot(N, aL));
                    float aNH = saturate(dot(N, aH));
                    float aLuster = pow(aNH, 1.0 / r2) * _LusterIntensity;
                    float3 alc = al.color * al.distanceAttenuation;
                    color += base * aNL * alc * 0.5 + aLuster * alc * 0.2;
                }
                #endif

                return half4(color, 1.0);
            }
            ENDHLSL
        }

        // Shadow caster
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                float3 wPos = TransformObjectToWorld(input.positionOS.xyz);
                float3 wNorm = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(wPos, wNorm, _LightDirection));
                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
