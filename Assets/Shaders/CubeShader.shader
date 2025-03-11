Shader "CubeMania/CubeShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _TextureScale("Texture scale", Float) = 1
        _TexWidth("Texture width", Float) = 1972
        _BlockSize("Block width", Float) = 34
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float _TextureScale;
            float _TexWidth;
            float _BlockSize;
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
            CBUFFER_END
            
            struct Attributes
            {                
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float3 wNormal      : NORMAL;
            };

            struct Varyings
            {                
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 wNormal      : NORMAL;
                float3 worldPos     : TEXCOORD1;
            };
            
            Varyings vert(Attributes IN)
            {                
                Varyings OUT;                
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;//TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.wNormal = IN.wNormal;
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            float myFmod(float a, float b)
            {
                return frac(abs(a/b)) * abs(b);
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                float size = _BlockSize/_TexWidth;

                float x = IN.worldPos.x * _TextureScale;
                float y = IN.worldPos.y * _TextureScale;
                float z = IN.worldPos.z * _TextureScale;
                float isUp = abs(IN.wNormal.y);
                
                float2 offset = float2(myFmod((z + x * (1 - isUp)), size), myFmod((y + x * isUp), size));
                
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv + offset);
                return color;
            }
            ENDHLSL
        }
    }
}
