Shader "TCP2/Custom/Table_Toon_Stencil_BuiltIn"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base Map", 2D) = "white" {}
        
        // Tính năng Toon: Gắn Texture Ramp (trắng đen) để chỉnh độ cứng của bóng
        _Ramp ("Toon Ramp (RGB)", 2D) = "gray" {} 
        
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    
    SubShader
    {
        Tags { 
            "RenderType"="Opaque" 
            "Queue"="Geometry+3" // Vẽ sau cái Hole (quan trọng)
        }
        
        LOD 200
        Cull Back

        // --- LOGIC CẮT LỖ (STENCIL) ---
        // Chỉ vẽ pixel nếu giá trị Stencil KHÁC 1
        Stencil
        {
            Ref 1
            Comp NotEqual
            Pass Keep
        }
        // -----------------------------

        CGPROGRAM
        // Khai báo Surface Shader dùng mô hình ánh sáng ToonRamp
        #pragma surface surf ToonRamp fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Ramp;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Custom Lighting Model để tạo hiệu ứng Toon đơn giản
        #pragma lighting ToonRamp exclude_path:prepass
        inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
        {
            #ifndef USING_DIRECTIONAL_LIGHT
            lightDir = normalize(lightDir);
            #endif

            // Tính độ sáng dựa trên góc chiếu (Lambert)
            half d = dot (s.Normal, lightDir) * 0.5 + 0.5;
            
            // Dùng Texture Ramp để bẻ cong ánh sáng thành các nấc (Toon effect)
            half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;

            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
            c.a = 0;
            return c;
        }

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Lấy màu từ Texture và Color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            
            // Setup Specular đơn giản (nếu cần bóng loáng)
            o.Specular = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}