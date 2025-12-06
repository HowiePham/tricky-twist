Shader "Custom/ColorRevealMultiple"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GrayscaleAmount ("Grayscale Amount", Range(0, 1)) = 1
        _EdgeSoftness ("Edge Softness", Range(0.1, 2.0)) = 0.5
        
        // Texture cho custom shape (ví dụ: vũng nước)
        _ShapeMask ("Shape Mask (Optional)", 2D) = "white" {}
        _UseShapeMask ("Use Shape Mask", Float) = 0
        _ShapeScale ("Shape Scale", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Số lượng vòng tròn reveal tối đa
            #define MAX_REVEALS 20

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ShapeMask;
            float4 _ShapeMask_ST;
            float _GrayscaleAmount;
            float _EdgeSoftness;
            float _UseShapeMask;
            float _ShapeScale;
            
            // Arrays cho nhiều reveal circles
            uniform float4 _RevealPositions[MAX_REVEALS]; // x, y, z = position, w = radius
            uniform int _RevealCount; // Số lượng reveals hiện tại

            // Helper functions để convert RGB <-> HSV (PHẢI KHAI BÁO TRƯỚC)
            float3 RGBtoHSV(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
            
            float3 HSVtoRGB(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Lấy màu gốc từ texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Tạo grayscale - GIỮ NGUYÊN ĐỘ SÁNG
                float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                fixed4 grayCol = fixed4(gray, gray, gray, col.a);
                
                // Tính toán reveal amount từ TẤT CẢ các vòng tròn
                float maxRevealAmount = 0.0;
                
                for (int j = 0; j < _RevealCount; j++)
                {
                    // Lấy vị trí và radius của vòng tròn thứ j
                    float3 revealPos = _RevealPositions[j].xyz;
                    float revealRadius = _RevealPositions[j].w;
                    
                    // Tính khoảng cách từ pixel hiện tại đến tâm vòng tròn
                    float dist = distance(i.worldPos.xy, revealPos.xy);
                    
                    float revealAmount = 0.0;
                    
                    if (_UseShapeMask > 0.5)
                    {
                        // SỬ DỤNG CUSTOM SHAPE MASK
                        
                        // Tính offset từ tâm reveal
                        float2 offset = (i.worldPos.xy - revealPos.xy);
                        
                        // Scale offset theo radius và shape scale
                        float2 maskUV = (offset / (revealRadius * _ShapeScale)) * 0.5 + 0.5;
                        
                        // Sample shape mask texture
                        float shapeMask = tex2D(_ShapeMask, maskUV).r;
                        
                        // Tính reveal dựa trên shape và distance
                        float distFactor = 1.0 - saturate(dist / revealRadius);
                        revealAmount = shapeMask * distFactor;
                        
                        // Smooth edge
                        revealAmount = smoothstep(0.1, 0.9, revealAmount);
                    }
                    else
                    {
                        // SỬ DỤNG HÌNH TRÒN (MẶC ĐỊNH)
                        
                        // Edge sắc nét hơn với smoothstep
                        revealAmount = 1.0 - smoothstep(
                            revealRadius - _EdgeSoftness * 0.5,  // Edge trong
                            revealRadius + _EdgeSoftness * 0.5,  // Edge ngoài
                            dist
                        );
                    }
                    
                    // Power curve để transition rõ hơn
                    revealAmount = pow(revealAmount, 0.6);
                    
                    // Lấy giá trị lớn nhất (nếu pixel nằm trong nhiều vòng tròn)
                    maxRevealAmount = max(maxRevealAmount, revealAmount);
                }
                
                // Tăng saturation của màu gốc (không đổi độ sáng)
                float3 hsv = RGBtoHSV(col.rgb);
                hsv.y = saturate(hsv.y * 1.3); // Tăng saturation 30%
                float3 colorBoosted = HSVtoRGB(hsv);
                
                // Blend giữa grayscale và màu có saturation cao
                fixed4 finalCol = lerp(grayCol, fixed4(colorBoosted, col.a), maxRevealAmount);
                
                return finalCol;
            }
            ENDCG
        }
    }
}