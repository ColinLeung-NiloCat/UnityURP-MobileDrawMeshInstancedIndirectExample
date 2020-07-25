Shader "MobileDrawMeshInstancedIndirect/SingleGrass"
{
    Properties
    {
        [MainColor] _BaseColor("BaseColor", Color) = (1,1,1,1)
        _GroundColor("_GroundColor", Color) = (0.5,0.5,0.5)
        _GrassWidth("_GrassWidth", Float) = 1
        _GrassHeight("_GrassHeight", Float) = 1
        _WindAIntensity("_WindAIntensity", Float) = 1
        _WindAFrequency("_WindAFrequency", Float) = 2
        _WindBIntensity("_WindBIntensity", Float) = 0.25
        _WindBFrequency("_WindBFrequency", Float) = 7.7
        //make SRP batcher happy
        [HideInInspector]_PivotPosWS("_PivotPosWS", Vector) = (0,0,0,0)
        [HideInInspector]_BoundSize("_BoundSize", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

        Pass
        {
            Cull Back //use default culling because this shader is billboard 

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
                float3 _PivotPosWS;
                float _BoundSize;
                float _GrassWidth;
                float _GrassHeight;
                float _WindAIntensity;
                float _WindAFrequency;
                float _WindBIntensity;
                float _WindBFrequency;
                half3 _BaseColor;
                half3 _GroundColor;

                StructuredBuffer<float4> _TransformBuffer;
            CBUFFER_END

            sampler2D _GrassBendingRT;

            Varyings vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                Varyings OUT;

                //bufferData.xyz    is posWS inside ComputeBuffer
                //bufferData.w      is scaleWS inside ComputeBuffer
                float4 bufferData = _TransformBuffer[instanceID];
                float3 perGrassPivotPosWS = bufferData.xyz;
                float perGrassHeight = bufferData.w * _GrassHeight;

                //get "is grass stepped" data(bending)
                float2 grassBendingUV = ((perGrassPivotPosWS.xz - _PivotPosWS.xz) / _BoundSize) * 0.5 + 0.5;//where is this grass inside bound (can optimize to 2 MAD)
                float stepped = tex2Dlod(_GrassBendingRT, float4(grassBendingUV, 0, 0)).x;

                //rotation(billboard grass LookAt() camera)
                //=========================================
                float3 cameraTransformRightWS = UNITY_MATRIX_V[0].xyz;//UNITY_MATRIX_V[0].xyz == world space camera Right unit vector
                float3 cameraTransformUpWS = UNITY_MATRIX_V[1].xyz;//UNITY_MATRIX_V[1].xyz == world space camera Up unit vector
                float3 cameraTransformForwardWS = -UNITY_MATRIX_V[2].xyz;//UNITY_MATRIX_V[2].xyz == -world space camera Forward unit vector

                //Expand Billboard (billboard Left+right)
                float3 positionOS = IN.positionOS.x * cameraTransformRightWS * _GrassWidth;

                //Expand Billboard (billboard Up)
                positionOS += IN.positionOS.y * cameraTransformUpWS;         
                //=========================================

                //bending by RT (hard code)
                float3 bendDir = cameraTransformForwardWS;
                bendDir.xz *= 0.5; //looks better
                positionOS = lerp(positionOS.xyz + bendDir * positionOS.y / -cameraTransformForwardWS.y, positionOS.xyz, stepped * 0.95 + 0.05);//don't fully bend, will produce ZFighting

                //scale
                positionOS.y *= perGrassHeight;

                //camera distance scale (make grass large if grass is far away to camera, to hide small pixel flicker)
                float3 viewWS = _WorldSpaceCameraPos - perGrassPivotPosWS;
                float lengthViewWS = length(viewWS);
                positionOS += IN.positionOS.x * cameraTransformRightWS * max(0, lengthViewWS * 0.015);

                //move grass to target posWS
                float3 positionWS = positionOS + perGrassPivotPosWS;

                //wind animation (biilboard Left Right direction sin wave)            
                float wind = sin(_Time.y * _WindAFrequency + perGrassPivotPosWS.x * 0.1) * _WindAIntensity; //windA
                wind += sin(_Time.y * _WindBFrequency + perGrassPivotPosWS.x * 0.7) * _WindBIntensity; //windB
                wind *= IN.positionOS.y; //only affect top region, don't affect root region
                float3 windOffset = cameraTransformRightWS * wind; //swing using billboard left right direction
                positionWS.xyz += windOffset;
                
                //complete posWS -> posCS
                OUT.positionCS = TransformWorldToHClip(positionWS);

                /////////////////////////////////////////////////////////////////////
                //lighting & color
                /////////////////////////////////////////////////////////////////////

                //lighting data
                Light mainLight;
#if _MAIN_LIGHT_SHADOWS
                mainLight = GetMainLight(TransformWorldToShadowCoord(positionWS));
#else
                mainLight = GetMainLight();
#endif
                half3 randomAddToN = sin(instanceID) * cameraTransformRightWS * 0.15;
                half3 N = normalize(half3(0,1,0) + randomAddToN);//random normal per grass
                half3 V = viewWS / lengthViewWS;
                half3 H = normalize(mainLight.direction + V);

                //direct diffuse 
                half3 lighting = mainLight.color;
                lighting *= saturate(dot(N, mainLight.direction) * 0.5 + 0.5); //half lambert

                lighting *= mainLight.shadowAttenuation;

                //indirect
                lighting += SampleSH(0);

                half3 albedo = lerp(_GroundColor,_BaseColor, IN.positionOS.y);
                OUT.color = albedo * lighting;

                //direct Specular
                float specular = saturate(dot(N,H));
                specular *= specular;
                specular *= specular;
                specular *= specular;
                specular *= specular;

                OUT.color += specular * mainLight.color * mainLight.shadowAttenuation * 0.1 * (positionOS.y * 0.5 + 0.5); 

                //fog
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

        //copy pass, change LightMode to ShadowCaster will make grass cast shadow
        //copy pass, change LightMode to DepthOnly will make grass render into _CameraDepthTexture
    }
}