Shader "Custom/BallShader"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white"
        _Colours("Colours", 2D) = "white"
        [HideInInspector] _Tier("Tier", int) = 0
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

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                int amount = pow(2, _Tier);
                float tau = radians(360.0);

                float4 paintColour = float4(0.0, 0.0, 0.0, 1.0);
                half2 centreUv = (IN.uv - 0.5) * 2.0;
                float distToCentre = distance(centreUv, half2(0.0, 0.0));
    
                half2 positions[256];
                
                float disTo[256];
                
                int selection = 0;
                float lastDist = 1000000.0;
                for (int i = 0; i < amount; ++i)
                {
                    float progress = tau * 1.0 * distToCentre + tau * float(i) / float(amount);
                    positions[i] = half2(cos(progress), sin(progress)) * distToCentre;
                    disTo[i] = distance(centreUv, positions[i]);
;
                    bool isCloser =  disTo[i] <= lastDist;
                    selection = i * int(isCloser) + selection * int(!isCloser);
                    lastDist = disTo[selection];
                }
    
                half2 sampleCoord = half2(selection,selection);
                sampleCoord.x %= 16;
                sampleCoord.y /= 16;
                sampleCoord /= 16;

                paintColour = SAMPLE_TEXTURE2D(_Colours, sampler_Colours, sampleCoord);
                paintColour.a = 1.0;

                half4 colour = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * paintColour;
                return colour;
            }
            ENDHLSL
        }
    }
}
