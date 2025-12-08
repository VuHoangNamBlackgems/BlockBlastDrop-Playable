Shader "URP/Custom/Lit_GeometryPlus3_Shadow"
{
    Properties
    {
        [MainTexture]_BaseMap ("Base Map", 2D) = "white" {}
        [MainColor]_BaseColor ("Base Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry+3"
        }

        ZWrite On
        Cull Back
        LOD 300

        // -------- ForwardLit --------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Stencil { Ref 1 Comp NotEqual }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // SRP Batcher: tất cả thuộc tính per-material nằm trong UnityPerMaterial
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float   _Metallic;
                float   _Smoothness;
                float2  _Padding_UM;   // giữ alignment 16-byte
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float3 posWS       : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float  fogCoord    : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs posIn = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs   nIn   = GetVertexNormalInputs(v.normalOS);

                o.positionHCS  = posIn.positionCS;
                o.posWS        = posIn.positionWS;
                o.normalWS     = nIn.normalWS;
                o.uv           = TRANSFORM_TEX(v.uv, _BaseMap);
                o.shadowCoord  = GetShadowCoord(posIn);
                o.fogCoord     = ComputeFogFactor(posIn.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float4 albedoTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                half3  albedo    = albedoTex.rgb;

                half3 normalWS = normalize(i.normalWS);

                // Main light + shadow
                Light mainLight = GetMainLight(i.shadowCoord);
                half3 L         = normalize(mainLight.direction);
                half   NdotL    = saturate(dot(normalWS, L));
                half   shadowA  = mainLight.shadowAttenuation;

                half3 diffuse = albedo * mainLight.color.rgb * NdotL * shadowA;

                // Additional lights (không shadow)
                #if defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                for (uint j = 0; j < count; j++)
                {
                    Light l = GetAdditionalLight(j, i.posWS);
                    half3 Lj = normalize(l.direction);
                    half   NdL = saturate(dot(normalWS, Lj));
                    diffuse += albedo * l.color.rgb * NdL;
                }
                #endif

                // Ambient (giữ nguyên cách tính đơn giản)
                half3 ambient = albedo * unity_AmbientSky.rgb;

                half3 color = diffuse + ambient;
                color = MixFog(color, i.fogCoord);
                return half4(color, 1);
            }
            ENDHLSL
        }

        // -------- ShadowCaster --------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            // SRP Batcher: lặp lại đúng layout UnityPerMaterial
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float   _Metallic;
                float   _Smoothness;
                float2  _Padding_UM;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs posIn = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs   nIn   = GetVertexNormalInputs(v.normalOS);

                float3 lightDirWS = normalize(_MainLightPosition.xyz);
                float3 biasedPos  = ApplyShadowBias(posIn.positionWS, nIn.normalWS, lightDirWS);
                o.positionHCS = TransformWorldToHClip(biasedPos);
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
