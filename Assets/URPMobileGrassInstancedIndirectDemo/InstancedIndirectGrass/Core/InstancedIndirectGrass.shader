Shader "Universal Render Pipeline/Custom/UnlitTexture"
{
    Properties
    {
        [MainColor] _BaseColor("BaseColor", Color) = (1,1,1,1)
        _GroundColor("_GroundColor", Color) = (0.5,0.5,0.5)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

        Pass
        {
            Cull Off
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // -------------------------------------
            // Universal Render Pipeline keywords
            // When doing custom shaders you most often want to copy and paste these #pragmas
            // These multi_compile variants are stripped from the build depending on:
            // 1) Settings in the URP Asset assigned in the GraphicsSettings at build time
            // e.g If you disabled AdditionalLights in the asset then all _ADDITIONA_LIGHTS variants
            // will be stripped from build
            // 2) Invalid combinations are stripped. e.g variants with _MAIN_LIGHT_SHADOWS_CASCADE
            // but not _MAIN_LIGHT_SHADOWS are invalid and therefore stripped.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            // -------------------------------------

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                half3 color        : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
            half3 _BaseColor;
            half3 _GroundColor;
            StructuredBuffer<float4> _TransformBuffer;
            float3 _PivotPosWS;
            float _BoundSize;
            CBUFFER_END

            sampler2D _GrassBendingRT;
            Varyings vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                Varyings OUT;

                float4 bufferData = _TransformBuffer[instanceID];
                
                float2 uv = ((bufferData.xz - _PivotPosWS.xz) / _BoundSize) * 0.5 + 0.5;
                float stepped = tex2Dlod(_GrassBendingRT, float4(uv, 0, 0)).x;

                float3 positionWS = IN.positionOS.xyz;

                //rotation(face camera in world space XZ only)
                //=========================================
                half3 viewDirWS = normalize(GetCameraPositionWS() - bufferData.xyz);

                half3 billboardTangentWS = normalize(float3(-viewDirWS.z, 0, viewDirWS.x));
                half3 billboardNormalWS = float3(billboardTangentWS.z, 0, -billboardTangentWS.x);
                //Sign!
                half3 billboardBitangentWS = -cross(billboardNormalWS, billboardTangentWS);

                //Expand Billboard
                float2 percent = IN.positionOS.xy;
                float _Shrink = 1; //temp
                float3 billboardPos = (percent.x - 0.5) * _Shrink * billboardTangentWS;

                //_PIVOTTOBOTTOM
                billboardPos.y += percent.y;

                //posOS finish
                positionWS = billboardPos;
                //=========================================

                //bending by RT (hard code)
                //positionWS.xyz = lerp(positionWS.xzy, positionWS.xyz, stepped*0.9);

                //scale
                positionWS.y *= bufferData.w;
                //pos
                positionWS.xyz += bufferData.xyz;
                //wind animation
                positionWS.xz += sin(_Time.y * 4 + bufferData.x * 0.1) * IN.positionOS.y * 0.2;


                //complete clip pos
                OUT.positionCS = TransformWorldToHClip(positionWS);

                //lighting & color
                Light mainLight;
#ifdef _MAIN_LIGHT_SHADOWS
                mainLight = GetMainLight(TransformWorldToShadowCoord(positionWS));
#else
                mainLight = GetMainLight();
#endif 
                half3 lighting = mainLight.shadowAttenuation;
                lighting *= mainLight.color;
                lighting += SampleSH(0);//indirect
                OUT.color = lerp(_GroundColor,_BaseColor, IN.positionOS.y) * lighting;

                float fogFactor = ComputeFogFactor(OUT.positionCS.z);
                // Mix the pixel color with fogColor. You can optionaly use MixFogColor to override the fogColor
                // with a custom one.
                OUT.color = MixFog(OUT.color, fogFactor);




                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return half4(IN.color,1);
            }
            ENDHLSL
        }


        Pass
        {
            Cull Off
            Tags { "LightMode" = "ShadowCaster" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // -------------------------------------
            // Universal Render Pipeline keywords
            // When doing custom shaders you most often want to copy and paste these #pragmas
            // These multi_compile variants are stripped from the build depending on:
            // 1) Settings in the URP Asset assigned in the GraphicsSettings at build time
            // e.g If you disabled AdditionalLights in the asset then all _ADDITIONA_LIGHTS variants
            // will be stripped from build
            // 2) Invalid combinations are stripped. e.g variants with _MAIN_LIGHT_SHADOWS_CASCADE
            // but not _MAIN_LIGHT_SHADOWS are invalid and therefore stripped.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            // -------------------------------------

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                half3 color        : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
            half3 _BaseColor;
            half3 _GroundColor;
            StructuredBuffer<float4> _TransformBuffer;
            float3 _PivotPosWS;
            float _BoundSize;
            CBUFFER_END

            sampler2D _GrassBendingRT;
            Varyings vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                Varyings OUT;

                float4 bufferData = _TransformBuffer[instanceID];
                
                float2 uv = ((bufferData.xz - _PivotPosWS.xz) / _BoundSize) * 0.5 + 0.5;
                float stepped = tex2Dlod(_GrassBendingRT, float4(uv, 0, 0)).x;

                float3 positionWS = IN.positionOS.xyz;

                //rotation(face camera in world space XZ only)
                //=========================================
                half3 viewDirWS = normalize(GetCameraPositionWS() - bufferData.xyz);

                half3 billboardTangentWS = normalize(float3(-viewDirWS.z, 0, viewDirWS.x));
                half3 billboardNormalWS = float3(billboardTangentWS.z, 0, -billboardTangentWS.x);
                //Sign!
                half3 billboardBitangentWS = -cross(billboardNormalWS, billboardTangentWS);

                //Expand Billboard
                float2 percent = IN.positionOS.xy;
                float _Shrink = 1; //temp
                float3 billboardPos = (percent.x - 0.5) * _Shrink * billboardTangentWS;

                //_PIVOTTOBOTTOM
                billboardPos.y += percent.y;

                //posOS finish
                positionWS = billboardPos;
                //=========================================

                //bending by RT (hard code)
                //positionWS.xyz = lerp(positionWS.xzy, positionWS.xyz, stepped*0.9);

                //scale
                positionWS.y *= bufferData.w;
                //pos
                positionWS.xyz += bufferData.xyz;
                //wind animation
                positionWS.xz += sin(_Time.y * 4 + bufferData.x * 0.1) * IN.positionOS.y * 0.2;


                //complete clip pos
                OUT.positionCS = TransformWorldToHClip(positionWS);

                //lighting & color
                Light mainLight;
#ifdef _MAIN_LIGHT_SHADOWS
                mainLight = GetMainLight(TransformWorldToShadowCoord(positionWS));
#else
                mainLight = GetMainLight();
#endif 
                half3 lighting = mainLight.shadowAttenuation;
                lighting *= mainLight.color;
                lighting += SampleSH(0);//indirect
                OUT.color = lerp(_GroundColor,_BaseColor, IN.positionOS.y) * lighting;

                float fogFactor = ComputeFogFactor(OUT.positionCS.z);
                // Mix the pixel color with fogColor. You can optionaly use MixFogColor to override the fogColor
                // with a custom one.
                OUT.color = MixFog(OUT.color, fogFactor);




                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return half4(IN.color,1);
            }
            ENDHLSL
        }
    }
}