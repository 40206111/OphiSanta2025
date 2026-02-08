Shader "Custom/BallShader"
{

    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white"
        _Colours("Colours", 2D) = "white"
        [Toggle] _USE_SPIRALS ("Use Spirals", Integer) = 0
        _Offset ("Offset", float) = 0.5
        [HideInInspector] _Tier("Tier", Integer) = 0
    }

    SubShader
    {

        Tags { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _USE_SPIRALS_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_Colours);
            SAMPLER(sampler_Colours);
            int _Tier;
            float _Offset;
            float3 colours[256];

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 paintColour = float4(0.0, 0.0, 0.0, 1.0);
                int amount = pow(2, _Tier);

            #if _USE_SPIRALS_ON
                // Normalized pixel coordinates (from -1 to 1)
                float2 uv = (IN.uv - 0.5) * 2.0;
    
                float dist = length(uv);
    
                float2 start = float2(0.0, 1.0);
                uv = uv / dist;
    
                float dotProduct = dot(start, uv);
                dotProduct = clamp(dotProduct, -1.0, 1.0);
                float angle = acos(dotProduct);
    
                angle *= float(uv.x >= 0) * 2.0 - 1.0;
    
                angle += radians(180.0);
    
                angle /= radians(360.0);
    
                angle += _Offset * (1.0 - pow(1.0 - dist, 5.0));
                angle %= 1.0;
    
                int index = floor(angle * float(amount));

                index = clamp(index, 0, amount-1);
                float butts = float(index) / float(amount);

                float2 sampleCoord;
                sampleCoord.x = float(index % 16);
                sampleCoord.y = float(index / 16);
                sampleCoord /= 16.0;

                sampleCoord.x = clamp(sampleCoord.x, 0.0, 15.0);
                sampleCoord.y = clamp(sampleCoord.y, 0.0, 15.0);

                paintColour = SAMPLE_TEXTURE2D(_Colours, sampler_Colours, sampleCoord);
                paintColour.a = 1.0;

            #else
                paintColour = SAMPLE_TEXTURE2D(_Colours, sampler_Colours, half2(0,0));
                for (int i = 1; i < amount; ++i)
                {
                    half2 sampleCoord = half2(i,i);
                    sampleCoord.x %= 16;
                    sampleCoord.y /= 16;
                    sampleCoord /= 16;
                    paintColour = paintColour * 0.5 + SAMPLE_TEXTURE2D(_Colours, sampler_Colours, sampleCoord) * 0.5;
                }
            #endif

                half4 colour = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * paintColour;
                return colour;
            }
            ENDHLSL
        }
    }
}
