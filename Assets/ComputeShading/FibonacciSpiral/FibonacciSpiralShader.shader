Shader "Custom/FibonacciSpiralShader"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline"
        }
        LOD 200

        Pass
        {
            HLSLPROGRAM
            
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            
            struct Varying
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Attributes
            {
                float4 pos : POSITION;
                float4 color : COLOR0;
            };

            CBUFFER_START(UnityPerMaterial);
            uniform StructuredBuffer<float3> _Positions;
            CBUFFER_END;
            
            Attributes vert(Varying i, uint instanceID : SV_InstanceID)
            {
                Attributes o;

                // 色を変える（Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlslにマクロが用意されてる）
                float colorValue = instanceID / 20.0f;
                float3 hsvColor = float3(colorValue, 1, 1);
                o.color = float4(HsvToRgb(hsvColor), 1);

                // サイズを変える
                i.vertex *= instanceID * 1.1 + 1;

                // ポジションを指定
                float4 wPos = TransformObjectToHClip(i.vertex + _Positions[instanceID]);
                o.pos = wPos;
                
                return o;
            }

            half4 frag(Attributes i) : SV_Target
            {
                return i.color;
            }
            
            ENDHLSL   
        }
    }
}
