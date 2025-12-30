Shader "Custom/CanvasShader"
{
    Properties
    {
        _PaintingTex("Painting Texture", 2D) = "white"
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
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
            
            TEXTURE2D(_PaintingTex);
            SAMPLER(sampler_PaintingTex);
            half3 _TextureSize;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 colour = SAMPLE_TEXTURE2D(_PaintingTex, sampler_PaintingTex, IN.uv);
                colour = saturate(colour + half4(0.1, 0.1, 0.1, 1.0));
                return colour;
            }
            ENDHLSL
        }
    }
}
