Shader "Birthstone/View to Tangent Rainbow"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _Offset("Offset", Range(0, 1)) = 0
        _ColorA("ColorA", Color) = (0, 0, 0, 1)
        _ColorB("ColorB", Color) = (0, 0, 0, 1)
        _Smoothness("Smoothness", Range(0, 1)) = 0.8
        _SpecularColor("SpecularColor", Color) = (1, 1, 1, 1)
        _DispersionStrength("Dispersion Strength", Range(0, 0.5)) = 0.15
        _RainbowIntensity("Rainbow Intensity", Range(0, 3)) = 1.5
        _FresnelPower("Fresnel Power", Range(1, 8)) = 3.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue"="Geometry"
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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

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
                float3 bitangentWS : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorA;
                float4 _ColorB;
                float4 _SpecularColor;
                float _Offset;
                float _Smoothness;
                float4 _BaseMap_ST;
                float _DispersionStrength;
                float _RainbowIntensity;
                float _FresnelPower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                VertexNormalInputs vertex = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS = vertex.normalWS;
                OUT.tangentWS = vertex.tangentWS;
                OUT.bitangentWS = vertex.bitangentWS;

                return OUT;
            }

            // Compute specular with a shifted half-vector to simulate chromatic dispersion
            float ComputeSpecChannel(float3 normalWS, float3 viewDirWS, float3 lightDirWS, float shift, float smoothness)
            {
                float3 H = normalize(viewDirWS + lightDirWS);
                // Shift the half-vector along the tangent plane per channel
                H = normalize(H + shift * normalWS);
                float NdotH = saturate(dot(normalWS, H));
                float exponent = 32.0 * smoothness + 0.0001;
                return pow(NdotH, exponent);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 T = normalize(IN.tangentWS);
                float3 B = normalize(IN.bitangentWS);
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - IN.positionWS);
                float3 lightDirWS = normalize(_MainLightPosition.xyz - IN.positionWS);

                // --- Original View-to-Tangent color ---
                float TdotV = dot(T, viewDirWS);
                float BdotV = dot(B, viewDirWS);
                float NdotV = dot(N, viewDirWS);

                float3 viewVector = float3(TdotV, BdotV, NdotV);
                float3 negatedOffset = (-1.0 * _Offset) * normalize(viewVector);
                float2 uvOffset = IN.uv + negatedOffset.xy;

                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvOffset);
                half4 baseMapReflection = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseMap.r - negatedOffset.xy);
                half4 color = lerp(_ColorA, _ColorB, baseMapReflection.r);

                // --- Rainbow Specular (Chromatic Dispersion) ---
                // Each RGB channel gets a slightly different specular angle,
                // simulating how a real gem splits white light into a spectrum.
                float3 H = normalize(viewDirWS + lightDirWS);
                float NdotH = saturate(dot(N, H));
                float NdotL = saturate(dot(N, lightDirWS));

                // Dispersion: shift half-vector differently per channel
                float3 H_R = normalize(H + _DispersionStrength * T);
                float3 H_G = normalize(H);
                float3 H_B = normalize(H - _DispersionStrength * T);

                float exponent = 32.0 * _Smoothness + 0.0001;
                float specR = pow(saturate(dot(N, H_R)), exponent);
                float specG = pow(saturate(dot(N, H_G)), exponent);
                float specB = pow(saturate(dot(N, H_B)), exponent);

                // Second axis dispersion (bitangent) for richer rainbow
                float3 H_R2 = normalize(H + _DispersionStrength * 0.7 * B);
                float3 H_B2 = normalize(H - _DispersionStrength * 0.7 * B);

                specR = max(specR, pow(saturate(dot(N, H_R2)), exponent));
                specB = max(specB, pow(saturate(dot(N, H_B2)), exponent));

                float3 rainbowSpec = float3(specR, specG, specB) * _RainbowIntensity * _SpecularColor.rgb;

                // --- Fresnel-based iridescence tint ---
                float fresnel = pow(1.0 - saturate(NdotV), _FresnelPower);
                // Angle-dependent hue shift using tangent/bitangent dot products
                float hueAngle = atan2(BdotV, TdotV);
                float3 iridescentTint = float3(
                    sin(hueAngle * 2.0) * 0.5 + 0.5,
                    sin(hueAngle * 2.0 + 2.094) * 0.5 + 0.5,
                    sin(hueAngle * 2.0 + 4.189) * 0.5 + 0.5
                );
                float3 fresnelRainbow = fresnel * iridescentTint * _RainbowIntensity * 0.4;

                // --- Combine ---
                // Main light contribution
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                float shadow = mainLight.shadowAttenuation;

                color.rgb += rainbowSpec * shadow;
                color.rgb += fresnelRainbow * shadow;

                // Additional lights rainbow specular
                #ifdef _ADDITIONAL_LIGHTS
                uint lightCount = GetAdditionalLightsCount();
                for (uint li = 0u; li < lightCount; ++li)
                {
                    Light al = GetAdditionalLight(li, IN.positionWS);
                    float3 aL = normalize(al.direction);
                    float3 aH = normalize(viewDirWS + aL);
                    float3 aH_R = normalize(aH + _DispersionStrength * T);
                    float3 aH_B = normalize(aH - _DispersionStrength * T);

                    float aSpecR = pow(saturate(dot(N, aH_R)), exponent);
                    float aSpecG = pow(saturate(dot(N, aH)), exponent);
                    float aSpecB = pow(saturate(dot(N, aH_B)), exponent);

                    float3 aRainbow = float3(aSpecR, aSpecG, aSpecB) * _RainbowIntensity * _SpecularColor.rgb;
                    color.rgb += aRainbow * al.color * al.distanceAttenuation * 0.5;
                }
                #endif

                return color;
            }
            ENDHLSL
        }
    }
}
