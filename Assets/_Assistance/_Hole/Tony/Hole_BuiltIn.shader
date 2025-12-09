Shader "TCP2/Custom/HoleStencilMask_BuiltIn"
{
    SubShader
    {
        Tags { 
            "RenderType"="Opaque" 
            "Queue"="Geometry+1" // Vẽ trước cái bàn (quan trọng)
        }

        // Không ghi màu (Tàng hình)
        ColorMask 0
        // Ghi độ sâu để vật phía sau không bị vẽ đè bậy
        ZWrite On
        Cull Back

        // Logic Stencil: Luôn ghi số 1 vào bộ nhớ đệm
        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }

        Pass
        {
            // Pass rỗng để thực hiện ZWrite và Stencil
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return 0;
            }
            ENDCG
        }
    }
}