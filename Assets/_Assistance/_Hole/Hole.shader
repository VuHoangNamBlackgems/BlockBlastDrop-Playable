Shader "URP/Custom/HoleStencilMask"
{
    SubShader
    {
        Tags{
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry+1"
        }

        // Không ghi màu, chỉ depth + stencil
        ColorMask 0
        ZWrite On
        Cull Back

        Stencil { Ref 1 Comp Always Pass Replace }

        Pass
        {
            Name "StencilOnly"
            Tags { "LightMode"="SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // SRP Batcher: vẫn khai báo UnityPerMaterial (rỗng) trong mọi pass
            CBUFFER_START(UnityPerMaterial)
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
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
                o.positionHCS = posIn.positionCS;
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                // ColorMask 0 => không ghi màu, chỉ để stencil/depth hoạt động
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
