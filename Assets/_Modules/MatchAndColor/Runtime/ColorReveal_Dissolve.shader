Shader "Custom/ColorReveal_Dissolve"
{
    Properties
    {
        _GrayscaleTex ("Grayscale Texture", 2D) = "white" {}
        _ColorTex ("Color Texture", 2D) = "white" {}

        // Transparency toggle
        [Toggle] _UseTransparency ("Use Transparency", Float) = 0

        // Dissolve parameters
        _DissolveSpeed ("Dissolve Speed", Range(0.5, 5.0)) = 2.0
        _DissolveEdgeWidth ("Edge Width", Range(0.01, 0.3)) = 0.1
        _DissolveNoiseScale ("Noise Scale", Range(0.5, 5.0)) = 2.0
        _DissolveContrast ("Dissolve Contrast", Range(0.5, 3.0)) = 1.5

        // Edge glow (optional)
        _EdgeGlowColor ("Edge Glow Color", Color) = (1, 0.8, 0.4, 1)
        _EdgeGlowIntensity ("Edge Glow Intensity", Range(0, 2)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        LOD 100

        // Pass for Transparent mode
        Pass
        {
            Name "TRANSPARENT"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _USETRANSPARENCY_ON
            #include "UnityCG.cginc"

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

            sampler2D _GrayscaleTex;
            sampler2D _ColorTex;
            float4 _GrayscaleTex_ST;

            float _DissolveSpeed;
            float _DissolveEdgeWidth;
            float _DissolveNoiseScale;
            float _DissolveContrast;
            float4 _EdgeGlowColor;
            float _EdgeGlowIntensity;

            uniform float4 _RevealPositions[MAX_REVEALS];
            uniform int _RevealCount;

            // Noise functions for dissolve pattern
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Layered noise for organic dissolve pattern
            float fbm(float2 p, int octaves)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                for (int i = 0; i < octaves; i++)
                {
                    value += amplitude * noise(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _GrayscaleTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample both textures
                fixed4 grayCol = tex2D(_GrayscaleTex, i.uv);
                fixed4 colorCol = tex2D(_ColorTex, i.uv);

                float maxRevealAmount = 0.0;
                float edgeGlow = 0.0;

                for (int j = 0; j < _RevealCount; j++)
                {
                    float3 revealPos = _RevealPositions[j].xyz;
                    float currentRadius = _RevealPositions[j].w;

                    float2 offset = i.worldPos.xy - revealPos.xy;
                    float dist = length(offset);

                    // === DISSOLVE EFFECT ===

                    // 1. Generate dissolve noise pattern
                    float2 noiseCoord = i.worldPos.xy * _DissolveNoiseScale;

                    // Multi-layer noise for organic pattern
                    float dissolveNoise = fbm(noiseCoord, 3);

                    // Add some directional variation based on distance from center
                    float angle = atan2(offset.y, offset.x);
                    float radialNoise = noise(float2(angle * 3.0, dist * 0.5));
                    dissolveNoise = lerp(dissolveNoise, radialNoise, 0.3);

                    // 2. Calculate dissolve threshold based on distance
                    float normalizedDist = dist / currentRadius;

                    // Dissolve from center outward
                    float dissolveThreshold = normalizedDist;

                    // 3. Compare noise with threshold to create dissolve effect
                    float dissolveMask = dissolveNoise - dissolveThreshold;

                    // Apply contrast to make edges sharper
                    dissolveMask = (dissolveMask - 0.5) * _DissolveContrast + 0.5;

                    // Smooth the edge
                    float revealAmount = smoothstep(0.0, _DissolveEdgeWidth, dissolveMask);

                    // 4. Edge detection for glow effect
                    float edgeDetect = smoothstep(0.0, _DissolveEdgeWidth * 0.5, dissolveMask) -
                        smoothstep(_DissolveEdgeWidth * 0.5, _DissolveEdgeWidth, dissolveMask);
                    edgeGlow = max(edgeGlow, edgeDetect);

                    maxRevealAmount = max(maxRevealAmount, revealAmount);
                }

                // Blend between grayscale and color textures
                fixed4 finalCol = lerp(grayCol, colorCol, maxRevealAmount);

                // Add edge glow effect
                if (_EdgeGlowIntensity > 0.0)
                {
                    finalCol.rgb += _EdgeGlowColor.rgb * edgeGlow * _EdgeGlowIntensity;
                }

                #ifdef _USETRANSPARENCY_ON
                    // Keep alpha channel when transparency is enabled
                    return finalCol;
                #else
                // Force opaque when transparency is disabled
                return fixed4(finalCol.rgb, 1.0);
                #endif
            }
            ENDCG
        }

        // Pass for Opaque mode (fallback)
        Pass
        {
            Name "OPAQUE"
            Tags
            {
                "RenderType"="Opaque"
            }
            ZWrite On
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _USETRANSPARENCY_ON
            #include "UnityCG.cginc"

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

            sampler2D _GrayscaleTex;
            sampler2D _ColorTex;
            float4 _GrayscaleTex_ST;

            float _DissolveSpeed;
            float _DissolveEdgeWidth;
            float _DissolveNoiseScale;
            float _DissolveContrast;
            float4 _EdgeGlowColor;
            float _EdgeGlowIntensity;

            uniform float4 _RevealPositions[MAX_REVEALS];
            uniform int _RevealCount;

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p, int octaves)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                for (int i = 0; i < octaves; i++)
                {
                    value += amplitude * noise(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _GrayscaleTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 grayCol = tex2D(_GrayscaleTex, i.uv);
                fixed4 colorCol = tex2D(_ColorTex, i.uv);

                float maxRevealAmount = 0.0;
                float edgeGlow = 0.0;

                for (int j = 0; j < _RevealCount; j++)
                {
                    float3 revealPos = _RevealPositions[j].xyz;
                    float currentRadius = _RevealPositions[j].w;

                    float2 offset = i.worldPos.xy - revealPos.xy;
                    float dist = length(offset);

                    float2 noiseCoord = i.worldPos.xy * _DissolveNoiseScale;
                    float dissolveNoise = fbm(noiseCoord, 3);

                    float angle = atan2(offset.y, offset.x);
                    float radialNoise = noise(float2(angle * 3.0, dist * 0.5));
                    dissolveNoise = lerp(dissolveNoise, radialNoise, 0.3);

                    float normalizedDist = dist / currentRadius;
                    float dissolveThreshold = normalizedDist;
                    float dissolveMask = dissolveNoise - dissolveThreshold;
                    dissolveMask = (dissolveMask - 0.5) * _DissolveContrast + 0.5;

                    float revealAmount = smoothstep(0.0, _DissolveEdgeWidth, dissolveMask);

                    float edgeDetect = smoothstep(0.0, _DissolveEdgeWidth * 0.5, dissolveMask) -
                        smoothstep(_DissolveEdgeWidth * 0.5, _DissolveEdgeWidth, dissolveMask);
                    edgeGlow = max(edgeGlow, edgeDetect);

                    maxRevealAmount = max(maxRevealAmount, revealAmount);
                }

                fixed4 finalCol = lerp(grayCol, colorCol, maxRevealAmount);

                if (_EdgeGlowIntensity > 0.0)
                {
                    finalCol.rgb += _EdgeGlowColor.rgb * edgeGlow * _EdgeGlowIntensity;
                }

                // Always opaque in this pass
                return fixed4(finalCol.rgb, 1.0);
            }
            ENDCG
        }
    }
}