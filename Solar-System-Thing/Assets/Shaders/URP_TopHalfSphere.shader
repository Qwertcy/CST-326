Shader "Custom/URP/TopHalfSphereUnlit" // shader path in the material dropdown
{
    Properties // editable material properties exposed in the inspector
    {
        _BaseMap ("base map (rgba)", 2D) = "white" {} // texture for clouds/atmosphere detail; alpha is used for transparency
        _BaseColor ("base color (rgba)", Color) = (1,1,1,1) // tint color; alpha multiplies overall transparency
        _CutHeight ("cut height (world y)", Float) = 0.0 // world-space y value; anything below this is clipped away
        _Fade ("fade thickness", Float) = 0.25 // soft fade thickness above the cut; 0 = hard clip edge
        [Toggle]_TwoSided ("two sided", Float) = 1 // 1 disables backface culling so you can see inside/outside of the dome
    }

    SubShader // contains one or more render passes; urp uses these in its pipeline
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" } // marks this for urp and transparent rendering order

        Pass // single forward pass that draws the object
        {
            Name "UnlitForward" // friendly name for debugging/profiling
            Tags { "LightMode"="UniversalForward" } // tells urp when to use this pass

            Blend SrcAlpha OneMinusSrcAlpha // standard alpha blending for transparency
            ZWrite Off // do not write to depth so transparency layers blend properly
            ZTest LEqual // normal depth test so it still respects depth ordering
            Cull Off // default to double-sided; we will optionally override in shader via keyword-like toggle

            HLSLPROGRAM // begin hlsl program block
            #pragma vertex vert // tells unity our vertex function name
            #pragma fragment frag // tells unity our fragment function name
            #pragma target 3.0 // target shader model; safe baseline for urp

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" // urp core helpers: transforms, macros, etc.

            TEXTURE2D(_BaseMap); // declares the texture resource handle
            SAMPLER(sampler_BaseMap); // declares the sampler state for the texture

            CBUFFER_START(UnityPerMaterial) // srp batcher-friendly per-material constant buffer
                float4 _BaseColor; // rgba tint and overall alpha multiplier
                float _CutHeight; // world y cutoff plane height
                float _Fade; // thickness of the fade zone above the cutoff
                float _TwoSided; // toggle value (0 or 1) used to control culling behavior
            CBUFFER_END // end per-material buffer

            struct Attributes // per-vertex input from the mesh
            {
                float4 positionOS : POSITION; // object-space vertex position
                float2 uv : TEXCOORD0; // primary uv set from the mesh
                float3 normalOS : NORMAL; // object-space normal; useful if you extend to sun-facing glow later
            }; // end attributes struct

            struct Varyings // data interpolated from vertex to fragment
            {
                float4 positionHCS : SV_POSITION; // homogenous clip-space position for rasterization
                float2 uv : TEXCOORD0; // interpolated uv coordinates
                float3 positionWS : TEXCOORD1; // world-space position for height-based clipping
            }; // end varyings struct

            Varyings vert(Attributes IN) // vertex shader: transforms vertices and outputs varyings
            {
                Varyings OUT; // declare output struct
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz); // convert object-space position to world-space position
                OUT.positionHCS = TransformWorldToHClip(positionWS); // convert world-space position to clip-space for rendering
                OUT.uv = IN.uv; // pass through uv so the fragment can sample the texture
                OUT.positionWS = positionWS; // pass world-space position for cutoff logic in fragment
                return OUT; // return varyings to the rasterizer
            } // end vertex function

            half4 frag(Varyings IN) : SV_Target // fragment shader: computes final pixel color
            {
                float fade = max(_Fade, 0.0001); // prevent divide-by-zero when fade is set to 0
                float y = IN.positionWS.y; // get world-space y coordinate for this pixel
                float below = (_CutHeight - fade) - y; // positive when pixel is below the fully-clipped region
                clip(-below); // discard pixels that are below (_CutHeight - _Fade); clip(x<0) discards, clip(x>=0) keeps

                float t = saturate((y - (_CutHeight - fade)) / fade); // 0 at the start of fade, 1 at/above cut height, smooth transition zone
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv); // sample the base map using interpolated uvs
                half4 col = tex * _BaseColor; // tint the sampled texture and apply user alpha multiplier
                col.a *= t; // fade alpha near the cutoff so the dome softly disappears instead of hard slicing
                return col; // output final rgba color
            } // end fragment function
            ENDHLSL // end hlsl program block
        } // end pass
    } // end subshader
} // end shader
