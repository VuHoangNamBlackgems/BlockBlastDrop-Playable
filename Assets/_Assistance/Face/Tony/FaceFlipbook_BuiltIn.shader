Shader "Face/FaceFlipbook_BuiltIn"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Sheet", 2D) = "white" {}
        _Columns ("Columns", Float) = 4
        _Rows ("Rows", Float) = 4
        [HideInInspector] _Index ("Index", Float) = 0
        _Cutoff ("Alpha Clip", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
        }
        LOD 100

        Pass
        {
            Name "Unlit"
            
            // Setup chế độ hòa trộn trong suốt
            Blend SrcAlpha OneMinusSrcAlpha
            // Tắt ghi độ sâu để tránh lỗi hiển thị khi chồng lên mặt
            ZWrite Off
            // Vẽ cả 2 mặt (nếu cần nhìn từ sau)
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Columns;
            float _Rows;
            float _Index;
            float _Cutoff;

            v2f vert (appdata v)
            {
                v2f o;
                // Chuyển đổi tọa độ vertex sang Clip Space (Chuẩn Built-in)
                o.pos = UnityObjectToClipPos(v.vertex);
                // Tính toán UV cơ bản
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // --- LOGIC TÍNH TOÁN FLIPBOOK ---
                // (Giữ nguyên thuật toán từ file URP gốc)
                
                float colCount = max(1.0, _Columns);
                float rowCount = max(1.0, _Rows);

                float2 tile = float2(1.0 / colCount, 1.0 / rowCount);
                float total = colCount * rowCount;
                
                // Kẹp chỉ số index trong khoảng hợp lệ
                float idx = clamp(_Index, 0.0, max(1.0, total) - 1.0);

                // Tính toán dòng và cột hiện tại dựa trên Index
                float row = floor(idx / colCount);
                float col = idx - row * colCount;

                // Tính UV mới:
                // Lưu ý: (rowCount - 1.0 - row) là để lật trục Y 
                // vì Unity UV gốc (0,0) ở dưới cùng bên trái, còn SpriteSheet thường đọc từ trên xuống.
                float2 finalUV = i.uv * tile + float2(col * tile.x, (rowCount - 1.0 - row) * tile.y);

                // Sample texture
                fixed4 c = tex2D(_MainTex, finalUV);
                
                // Alpha clipping (nếu cần cắt bỏ vùng trong suốt hoàn toàn)
                clip(c.a - _Cutoff);

                return c;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}