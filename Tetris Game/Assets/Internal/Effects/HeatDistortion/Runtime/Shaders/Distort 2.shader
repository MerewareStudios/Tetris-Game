Shader "Shader Graphs/Distortion2"
{
    Properties
    {
        [Normal][NoScaleOffset]_Distort("Distort", 2D) = "bump" {}
        _Power("Power", Float) = 0.93
        [HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
            "DisableBatching"="False"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalUnlitSubTarget"
            "LightMode" = "UseColorTexture"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                // LightMode: <None>
            }
        
        // Render State
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma shader_feature _ _SAMPLE_GI
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_UNLIT
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float3 positionWS : INTERP1;
             float3 normalWS : INTERP2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Distort_TexelSize;
        float _Power;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_GrabbedTexture);
        SAMPLER(sampler_GrabbedTexture);
        float4 _GrabbedTexture_TexelSize;
        TEXTURE2D(_Distort);
        SAMPLER(sampler_Distort);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_NormalUnpack_float(float4 In, out float3 Out)
        {
                        Out = UnpackNormal(In);
                    }
        
        void Unity_Preview_float2(float2 In, out float2 Out)
        {
            Out = In;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_GrabbedTexture);
            UnityTexture2D _Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Distort);
            float4 _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D.tex, _Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D.samplerstate, _Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_R_4_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.r;
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_G_5_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.g;
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_B_6_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.b;
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_A_7_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.a;
            float3 _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3;
            Unity_NormalUnpack_float(_SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4, _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3);
            float _Split_a4ced39de6b54dd8937d56fdd478f664_R_1_Float = _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3[0];
            float _Split_a4ced39de6b54dd8937d56fdd478f664_G_2_Float = _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3[1];
            float _Split_a4ced39de6b54dd8937d56fdd478f664_B_3_Float = _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3[2];
            float _Split_a4ced39de6b54dd8937d56fdd478f664_A_4_Float = 0;
            float2 _Vector2_7816cd57c8e84eefb04934c5a20ab756_Out_0_Vector2 = float2(_Split_a4ced39de6b54dd8937d56fdd478f664_R_1_Float, _Split_a4ced39de6b54dd8937d56fdd478f664_G_2_Float);
            float2 _Preview_2eb41e96ef6d47858bca3ceb025bd2fd_Out_1_Vector2;
            Unity_Preview_float2(_Vector2_7816cd57c8e84eefb04934c5a20ab756_Out_0_Vector2, _Preview_2eb41e96ef6d47858bca3ceb025bd2fd_Out_1_Vector2);
            float _Property_2040b8adc6aa4fb3a6f4233155615e8b_Out_0_Float = _Power;
            float2 _Multiply_461b9132971c4935a749170380267e6f_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Preview_2eb41e96ef6d47858bca3ceb025bd2fd_Out_1_Vector2, (_Property_2040b8adc6aa4fb3a6f4233155615e8b_Out_0_Float.xx), _Multiply_461b9132971c4935a749170380267e6f_Out_2_Vector2);
            float4 _ScreenPosition_7915ba73b96c4365a434313528022470_Out_0_Vector4 = float4(IN.NDCPosition.xy, 0, 0);
            float2 _Add_40cb266ccd93457587a19ae1fd8617e1_Out_2_Vector2;
            Unity_Add_float2(_Multiply_461b9132971c4935a749170380267e6f_Out_2_Vector2, (_ScreenPosition_7915ba73b96c4365a434313528022470_Out_0_Vector4.xy), _Add_40cb266ccd93457587a19ae1fd8617e1_Out_2_Vector2);
            float4 _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D.tex, _Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D.samplerstate, _Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D.GetTransformedUV(_Add_40cb266ccd93457587a19ae1fd8617e1_Out_2_Vector2) );
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_R_4_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.r;
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_G_5_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.g;
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_B_6_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.b;
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_A_7_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.a;
            surface.BaseColor = (_SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.xyz);
            surface.Alpha = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_A_7_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "DepthNormalsOnly"
            Tags
            {
                "LightMode" = "DepthNormalsOnly"
            }
        
        // Render State
        Cull Back
        ZTest LEqual
        ZWrite On
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
        #define _SURFACE_TYPE_TRANSPARENT 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float3 normalWS : INTERP1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Distort_TexelSize;
        float _Power;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_GrabbedTexture);
        SAMPLER(sampler_GrabbedTexture);
        float4 _GrabbedTexture_TexelSize;
        TEXTURE2D(_Distort);
        SAMPLER(sampler_Distort);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_NormalUnpack_float(float4 In, out float3 Out)
        {
                        Out = UnpackNormal(In);
                    }
        
        void Unity_Preview_float2(float2 In, out float2 Out)
        {
            Out = In;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_GrabbedTexture);
            UnityTexture2D _Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Distort);
            float4 _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D.tex, _Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D.samplerstate, _Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_R_4_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.r;
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_G_5_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.g;
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_B_6_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.b;
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_A_7_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.a;
            float3 _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3;
            Unity_NormalUnpack_float(_SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4, _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3);
            float _Split_a4ced39de6b54dd8937d56fdd478f664_R_1_Float = _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3[0];
            float _Split_a4ced39de6b54dd8937d56fdd478f664_G_2_Float = _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3[1];
            float _Split_a4ced39de6b54dd8937d56fdd478f664_B_3_Float = _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3[2];
            float _Split_a4ced39de6b54dd8937d56fdd478f664_A_4_Float = 0;
            float2 _Vector2_7816cd57c8e84eefb04934c5a20ab756_Out_0_Vector2 = float2(_Split_a4ced39de6b54dd8937d56fdd478f664_R_1_Float, _Split_a4ced39de6b54dd8937d56fdd478f664_G_2_Float);
            float2 _Preview_2eb41e96ef6d47858bca3ceb025bd2fd_Out_1_Vector2;
            Unity_Preview_float2(_Vector2_7816cd57c8e84eefb04934c5a20ab756_Out_0_Vector2, _Preview_2eb41e96ef6d47858bca3ceb025bd2fd_Out_1_Vector2);
            float _Property_2040b8adc6aa4fb3a6f4233155615e8b_Out_0_Float = _Power;
            float2 _Multiply_461b9132971c4935a749170380267e6f_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Preview_2eb41e96ef6d47858bca3ceb025bd2fd_Out_1_Vector2, (_Property_2040b8adc6aa4fb3a6f4233155615e8b_Out_0_Float.xx), _Multiply_461b9132971c4935a749170380267e6f_Out_2_Vector2);
            float4 _ScreenPosition_7915ba73b96c4365a434313528022470_Out_0_Vector4 = float4(IN.NDCPosition.xy, 0, 0);
            float2 _Add_40cb266ccd93457587a19ae1fd8617e1_Out_2_Vector2;
            Unity_Add_float2(_Multiply_461b9132971c4935a749170380267e6f_Out_2_Vector2, (_ScreenPosition_7915ba73b96c4365a434313528022470_Out_0_Vector4.xy), _Add_40cb266ccd93457587a19ae1fd8617e1_Out_2_Vector2);
            float4 _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D.tex, _Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D.samplerstate, _Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D.GetTransformedUV(_Add_40cb266ccd93457587a19ae1fd8617e1_Out_2_Vector2) );
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_R_4_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.r;
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_G_5_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.g;
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_B_6_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.b;
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_A_7_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.a;
            surface.Alpha = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_A_7_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Distort_TexelSize;
        float _Power;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_GrabbedTexture);
        SAMPLER(sampler_GrabbedTexture);
        float4 _GrabbedTexture_TexelSize;
        TEXTURE2D(_Distort);
        SAMPLER(sampler_Distort);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_NormalUnpack_float(float4 In, out float3 Out)
        {
                        Out = UnpackNormal(In);
                    }
        
        void Unity_Preview_float2(float2 In, out float2 Out)
        {
            Out = In;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_GrabbedTexture);
            UnityTexture2D _Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Distort);
            float4 _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D.tex, _Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D.samplerstate, _Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_R_4_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.r;
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_G_5_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.g;
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_B_6_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.b;
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_A_7_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.a;
            float3 _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3;
            Unity_NormalUnpack_float(_SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4, _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3);
            float _Split_a4ced39de6b54dd8937d56fdd478f664_R_1_Float = _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3[0];
            float _Split_a4ced39de6b54dd8937d56fdd478f664_G_2_Float = _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3[1];
            float _Split_a4ced39de6b54dd8937d56fdd478f664_B_3_Float = _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3[2];
            float _Split_a4ced39de6b54dd8937d56fdd478f664_A_4_Float = 0;
            float2 _Vector2_7816cd57c8e84eefb04934c5a20ab756_Out_0_Vector2 = float2(_Split_a4ced39de6b54dd8937d56fdd478f664_R_1_Float, _Split_a4ced39de6b54dd8937d56fdd478f664_G_2_Float);
            float2 _Preview_2eb41e96ef6d47858bca3ceb025bd2fd_Out_1_Vector2;
            Unity_Preview_float2(_Vector2_7816cd57c8e84eefb04934c5a20ab756_Out_0_Vector2, _Preview_2eb41e96ef6d47858bca3ceb025bd2fd_Out_1_Vector2);
            float _Property_2040b8adc6aa4fb3a6f4233155615e8b_Out_0_Float = _Power;
            float2 _Multiply_461b9132971c4935a749170380267e6f_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Preview_2eb41e96ef6d47858bca3ceb025bd2fd_Out_1_Vector2, (_Property_2040b8adc6aa4fb3a6f4233155615e8b_Out_0_Float.xx), _Multiply_461b9132971c4935a749170380267e6f_Out_2_Vector2);
            float4 _ScreenPosition_7915ba73b96c4365a434313528022470_Out_0_Vector4 = float4(IN.NDCPosition.xy, 0, 0);
            float2 _Add_40cb266ccd93457587a19ae1fd8617e1_Out_2_Vector2;
            Unity_Add_float2(_Multiply_461b9132971c4935a749170380267e6f_Out_2_Vector2, (_ScreenPosition_7915ba73b96c4365a434313528022470_Out_0_Vector4.xy), _Add_40cb266ccd93457587a19ae1fd8617e1_Out_2_Vector2);
            float4 _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D.tex, _Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D.samplerstate, _Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D.GetTransformedUV(_Add_40cb266ccd93457587a19ae1fd8617e1_Out_2_Vector2) );
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_R_4_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.r;
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_G_5_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.g;
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_B_6_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.b;
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_A_7_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.a;
            surface.Alpha = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_A_7_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Back
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Distort_TexelSize;
        float _Power;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_GrabbedTexture);
        SAMPLER(sampler_GrabbedTexture);
        float4 _GrabbedTexture_TexelSize;
        TEXTURE2D(_Distort);
        SAMPLER(sampler_Distort);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_NormalUnpack_float(float4 In, out float3 Out)
        {
                        Out = UnpackNormal(In);
                    }
        
        void Unity_Preview_float2(float2 In, out float2 Out)
        {
            Out = In;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_GrabbedTexture);
            UnityTexture2D _Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Distort);
            float4 _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D.tex, _Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D.samplerstate, _Property_0492ad123b6342fcaaae223efa406762_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_R_4_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.r;
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_G_5_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.g;
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_B_6_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.b;
            float _SampleTexture2D_a4a05462008943e29dc9a35214a790df_A_7_Float = _SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4.a;
            float3 _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3;
            Unity_NormalUnpack_float(_SampleTexture2D_a4a05462008943e29dc9a35214a790df_RGBA_0_Vector4, _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3);
            float _Split_a4ced39de6b54dd8937d56fdd478f664_R_1_Float = _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3[0];
            float _Split_a4ced39de6b54dd8937d56fdd478f664_G_2_Float = _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3[1];
            float _Split_a4ced39de6b54dd8937d56fdd478f664_B_3_Float = _NormalUnpack_3e8df1a0c9c34d49849b22459ae3747a_Out_1_Vector3[2];
            float _Split_a4ced39de6b54dd8937d56fdd478f664_A_4_Float = 0;
            float2 _Vector2_7816cd57c8e84eefb04934c5a20ab756_Out_0_Vector2 = float2(_Split_a4ced39de6b54dd8937d56fdd478f664_R_1_Float, _Split_a4ced39de6b54dd8937d56fdd478f664_G_2_Float);
            float2 _Preview_2eb41e96ef6d47858bca3ceb025bd2fd_Out_1_Vector2;
            Unity_Preview_float2(_Vector2_7816cd57c8e84eefb04934c5a20ab756_Out_0_Vector2, _Preview_2eb41e96ef6d47858bca3ceb025bd2fd_Out_1_Vector2);
            float _Property_2040b8adc6aa4fb3a6f4233155615e8b_Out_0_Float = _Power;
            float2 _Multiply_461b9132971c4935a749170380267e6f_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Preview_2eb41e96ef6d47858bca3ceb025bd2fd_Out_1_Vector2, (_Property_2040b8adc6aa4fb3a6f4233155615e8b_Out_0_Float.xx), _Multiply_461b9132971c4935a749170380267e6f_Out_2_Vector2);
            float4 _ScreenPosition_7915ba73b96c4365a434313528022470_Out_0_Vector4 = float4(IN.NDCPosition.xy, 0, 0);
            float2 _Add_40cb266ccd93457587a19ae1fd8617e1_Out_2_Vector2;
            Unity_Add_float2(_Multiply_461b9132971c4935a749170380267e6f_Out_2_Vector2, (_ScreenPosition_7915ba73b96c4365a434313528022470_Out_0_Vector4.xy), _Add_40cb266ccd93457587a19ae1fd8617e1_Out_2_Vector2);
            float4 _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D.tex, _Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D.samplerstate, _Property_6d6b628cb9144d3ea0f9abaa82ea57ff_Out_0_Texture2D.GetTransformedUV(_Add_40cb266ccd93457587a19ae1fd8617e1_Out_2_Vector2) );
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_R_4_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.r;
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_G_5_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.g;
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_B_6_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.b;
            float _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_A_7_Float = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_RGBA_0_Vector4.a;
            surface.Alpha = _SampleTexture2D_530e5cbdece14777a8b9bdba802e0de0_A_7_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    CustomEditorForRenderPipeline "UnityEditor.ShaderGraphUnlitGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
    FallBack "Hidden/Shader Graph/FallbackError"
}