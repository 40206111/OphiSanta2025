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
                half2 sampleCoord = half2(0,0);
                float4 paintColour = SAMPLE_TEXTURE2D(_Colours, sampler_Colours, sampleCoord);
                half4 colour = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * paintColour;
                return colour;
            }
            ENDHLSL
        }
    }
}
