Shader "Birthstone/GemShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _DeepColor ("Deep Color", Color) = (0.5, 0.5, 0.5, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.95
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 3.0
        _FresnelIntensity ("Fresnel Intensity", Range(0, 3)) = 1.5
        _DispersionStrength ("Dispersion Strength", Range(0, 1)) = 0.5
        _InternalReflection ("Internal Reflection", Range(0, 1)) = 0.8
        _SparkleIntensity ("Sparkle Intensity", Range(0, 5)) = 2.0
        _SparkleScale ("Sparkle Scale", Range(1, 100)) = 40.0
        _SparkleSpeed ("Sparkle Speed", Range(0, 5)) = 1.0
        _Transparency ("Transparency", Range(0, 1)) = 0.7
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        // Back face pass
        Pass
        {
            Name "GemBack"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _DeepColor;
                float _Smoothness;
                float _FresnelPower;
                float _FresnelIntensity;
                float _DispersionStrength;
                float _InternalReflection;
                float _SparkleIntensity;
                float _SparkleScale;
                float _SparkleSpeed;
                float _Transparency;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = -TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(TransformObjectToWorld(input.positionOS.xyz));
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 N = normalize(input.normalWS);
                float3 V = normalize(input.viewDirWS);
                float NdotV = saturate(dot(N, V));

                float3 color = lerp(_DeepColor.rgb, _BaseColor.rgb, NdotV);
                return half4(color, _Transparency * 0.6);
            }
            ENDHLSL
        }

        // Front face pass
        Pass
        {
            Name "GemFront"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

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
                float4 _DeepColor;
                float _Smoothness;
                float _FresnelPower;
                float _FresnelIntensity;
                float _DispersionStrength;
                float _InternalReflection;
                float _SparkleIntensity;
                float _SparkleScale;
                float _SparkleSpeed;
                float _Transparency;
            CBUFFER_END

            // Simple hash for sparkle
            float gemHash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(443.897, 397.297, 491.187));
                p3 += dot(p3, p3.yzx + 19.19);
                return frac((p3.x + p3.y) * p3.z);
            }

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
                float fresnel = pow(1.0 - NdotV, _FresnelPower) * _FresnelIntensity;

                // Main light
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                float3 L = normalize(mainLight.direction);
                float3 H = normalize(V + L);
                float NdotL = saturate(dot(N, L));
                float NdotH = saturate(dot(N, H));

                // Specular GGX
                float roughness = 1.0 - _Smoothness;
                float r2 = max(roughness * roughness, 0.002);
                float dTerm = NdotH * NdotH * (r2 - 1.0) + 1.0;
                float spec = r2 / (3.14159 * dTerm * dTerm + 0.0001);
                spec = min(spec, 100.0);

                // Base color with depth
                float3 baseColor = lerp(_DeepColor.rgb, _BaseColor.rgb, NdotV);

                // Fake chromatic dispersion (rainbow from angle, no cubemap)
                float dispAngle = dot(reflect(-V, N), L) * 0.5 + 0.5;
                float3 rainbow = float3(
                    saturate(sin(dispAngle * 6.283 + 0.0) * 0.5 + 0.5),
                    saturate(sin(dispAngle * 6.283 + 2.094) * 0.5 + 0.5),
                    saturate(sin(dispAngle * 6.283 + 4.189) * 0.5 + 0.5)
                );
                float3 dispersion = rainbow * _DispersionStrength * baseColor * _InternalReflection;

                // Sparkle (fully unrolled, no loops)
                float2 sUV = input.positionWS.xz * _SparkleScale + float2(input.positionWS.y * 3.7, _Time.y * _SparkleSpeed);
                float2 sCell = floor(sUV);
                float2 sFrac = frac(sUV);
                float sMin = 1.0;
                float2 sOff; float2 sPt; float sDist;
                sOff = float2(-1,-1); sPt = sOff + float2(gemHash(sCell+sOff), gemHash(sCell+sOff+71.0)) - sFrac; sDist = dot(sPt,sPt); sMin = min(sMin, sDist);
                sOff = float2(-1, 0); sPt = sOff + float2(gemHash(sCell+sOff), gemHash(sCell+sOff+71.0)) - sFrac; sDist = dot(sPt,sPt); sMin = min(sMin, sDist);
                sOff = float2(-1, 1); sPt = sOff + float2(gemHash(sCell+sOff), gemHash(sCell+sOff+71.0)) - sFrac; sDist = dot(sPt,sPt); sMin = min(sMin, sDist);
                sOff = float2( 0,-1); sPt = sOff + float2(gemHash(sCell+sOff), gemHash(sCell+sOff+71.0)) - sFrac; sDist = dot(sPt,sPt); sMin = min(sMin, sDist);
                sOff = float2( 0, 0); sPt = sOff + float2(gemHash(sCell+sOff), gemHash(sCell+sOff+71.0)) - sFrac; sDist = dot(sPt,sPt); sMin = min(sMin, sDist);
                sOff = float2( 0, 1); sPt = sOff + float2(gemHash(sCell+sOff), gemHash(sCell+sOff+71.0)) - sFrac; sDist = dot(sPt,sPt); sMin = min(sMin, sDist);
                sOff = float2( 1,-1); sPt = sOff + float2(gemHash(sCell+sOff), gemHash(sCell+sOff+71.0)) - sFrac; sDist = dot(sPt,sPt); sMin = min(sMin, sDist);
                sOff = float2( 1, 0); sPt = sOff + float2(gemHash(sCell+sOff), gemHash(sCell+sOff+71.0)) - sFrac; sDist = dot(sPt,sPt); sMin = min(sMin, sDist);
                sOff = float2( 1, 1); sPt = sOff + float2(gemHash(sCell+sOff), gemHash(sCell+sOff+71.0)) - sFrac; sDist = dot(sPt,sPt); sMin = min(sMin, sDist);
                float sparkle = pow(saturate(1.0 - sMin * 3.0), 20.0) * _SparkleIntensity;
                sparkle *= pow(NdotH, 2.0) + 0.3;

                // Compose
                float3 lc = mainLight.color * mainLight.shadowAttenuation;
                float3 color = baseColor * NdotL * lc * 0.3
                             + dispersion
                             + spec * lc * 0.5
                             + sparkle * (baseColor + float3(1,1,1)) * 0.5
                             + fresnel * lerp(baseColor, float3(1,1,1), 0.5) * 0.3
                             + baseColor * 0.1;

                // Additional lights
                #ifdef _ADDITIONAL_LIGHTS
                uint lightCount = GetAdditionalLightsCount();
                for (uint li = 0u; li < lightCount; ++li)
                {
                    Light al = GetAdditionalLight(li, input.positionWS);
                    float3 aL = normalize(al.direction);
                    float3 aH = normalize(V + aL);
                    float aNL = saturate(dot(N, aL));
                    float aNH = saturate(dot(N, aH));
                    float aD = aNH * aNH * (r2 - 1.0) + 1.0;
                    float aS = r2 / (3.14159 * aD * aD + 0.0001);
                    aS = min(aS, 100.0);
                    float3 alc = al.color * al.distanceAttenuation;
                    color += baseColor * aNL * alc * 0.3 + aS * alc * 0.3;
                }
                #endif

                float alpha = lerp(_Transparency, 1.0, fresnel * 0.5);
                return half4(color, alpha);
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
