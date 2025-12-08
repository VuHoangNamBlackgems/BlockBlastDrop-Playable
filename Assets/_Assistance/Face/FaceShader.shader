Shader "Face/FaceFlipbookURP"
{
    Properties
    {
        [MainTexture]_MainTex("Sprite Sheet", 2D) = "white" {}
        _Columns("Columns", Float) = 4
        _Rows("Rows", Float) = 4
        [HideInInspector]_Index("Index", Float) = 0
        _Cutoff("Alpha Clip", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            #pragma prefer_hlslcc gles
            #pragma enable_d3d11_debug_symbols

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // Dữ liệu cho SRP Batcher (phải nằm trong CBUFFER)
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Columns;
                float _Rows;
                float _Index;
                float _Cutoff;
            CBUFFER_END

            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float colCount = max(1.0, _Columns);
                float rowCount = max(1.0, _Rows);

                float2 tile = float2(1.0 / colCount, 1.0 / rowCount);
                float total = colCount * rowCount;
                float idx = clamp(_Index, 0.0, max(1.0, total) - 1.0);

                float row = floor(idx / colCount);
                float col = idx - row * colCount;

                // Lật UV theo chiều dọc để khớp sprite sheet
                float2 uv = i.uv * tile + float2(col * tile.x, (rowCount - 1.0 - row) * tile.y);

                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                clip(c.a - _Cutoff);
                return c;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
