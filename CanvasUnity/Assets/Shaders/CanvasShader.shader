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

            #include "UnityCG.cginc"

            UNITY_DECLARE_TEX2D(_PaintingTex);

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

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = UnityObjectToClipPos(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // int radius = 15;

                // half4 colour = half4(1.0, 1.0, 1.0, 1.0);
                
                // for (int i = 0; i < radius * radius; i++)
                // {
                //     for (int j = 0; j < radius * radius; j++)
                //     {
                //         if (i + j < radius)
                //         {
                //             float2 coord = IN.uv + float2(i, j);
                //             half4 newCol = colour * UNITY_SAMPLE_TEX2D(_PaintingTex, coord);
                //             colour += newCol * (float)(newCol != half4(0.0, 0.0, 0.0, 0.0));
                //         }
                //     }
                // }

                half4 colour = UNITY_SAMPLE_TEX2D(_PaintingTex, IN.uv);
                colour += half4(1.0, 1.0, 1.0, 1.0) * (float)(colour == half4(0.0, 0.0, 0.0, 0.0));
                return  colour;
            }
            ENDHLSL
        }
    }
}
