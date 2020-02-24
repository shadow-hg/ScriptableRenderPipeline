#ifndef UNIVERSAL_COPY_DEPTH_PASS_INCLUDED
#define UNIVERSAL_COPY_DEPTH_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float2 uv           : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
    float2 uv           : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings vert(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    output.uv = UnityStereoTransformScreenSpaceTex(input.uv);
    //output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.positionCS = float4(input.positionOS.xyz, 1.0);
    return output;
}

#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
#define DEPTH_TEXTURE_MS(name, samples) Texture2DMSArray<float, samples> name
#define DEPTH_TEXTURE(name) TEXTURE2D_ARRAY_FLOAT(name)
#define LOAD(uv, sampleIndex) LOAD_TEXTURE2D_ARRAY_MSAA(_CameraDepthAttachment, uv, unity_StereoEyeIndex, sampleIndex)
#define SAMPLE(uv) SAMPLE_TEXTURE2D_ARRAY(_CameraDepthAttachment, sampler_CameraDepthAttachment, uv, unity_StereoEyeIndex).r
#else
#define DEPTH_TEXTURE_MS(name, samples) Texture2DMS<float, samples> name
#define DEPTH_TEXTURE(name) TEXTURE2D_FLOAT(name)
#define LOAD(uv, sampleIndex) LOAD_TEXTURE2D_MSAA(_CameraDepthAttachment, uv, sampleIndex)
#define SAMPLE(uv) SAMPLE_DEPTH_TEXTURE(_CameraDepthAttachment, sampler_CameraDepthAttachment, uv)
#endif

#if defined(_DEPTH_MSAA_2)
    #define MSAA_SAMPLES 2
#elif defined(_DEPTH_MSAA_4)
    #define MSAA_SAMPLES 4
#elif defined(_DEPTH_MSAA_8)
    #define MSAA_SAMPLES 8
#else
    #define MSAA_SAMPLES 1
#endif

#if MSAA_SAMPLES == 1
    DEPTH_TEXTURE(_CameraDepthAttachment);
    SAMPLER(sampler_CameraDepthAttachment);
#else
    DEPTH_TEXTURE_MS(_CameraDepthAttachment, MSAA_SAMPLES);
    float4 _CameraDepthAttachment_TexelSize;
#endif

#if UNITY_REVERSED_Z
    #define DEPTH_DEFAULT_VALUE 1.0
    #define DEPTH_OP min
#else
    #define DEPTH_DEFAULT_VALUE 0.0
    #define DEPTH_OP max
#endif

half4 _ScaleBiasRT;

float SampleDepth(float2 uv)
{
    // Currently CopyDepthPass.cs uses cmd.Blit() that sets implicitly a projection matrix with y-flip
    // This can cause some issues that might end up sampling depth with a wrong orientation depending on if URP
    // renders game to RT or screen (case https://issuetracker.unity3d.com/issues/lwrp-depth-texture-flipy)

    // How to fix it:
    // - Ideally remove cmd.Blit() madness from the pipeline. It needs all camera matrices to be setup by the pipeline and big XR work.
    // - We need a fix for now, so BUCKLE UP and try to follow the wonders of y-flip in Unity
    //
    // If URP is rendering to RT:
    //  - Source Depth is upside down. We copy depth with cmd.Blit that flips it to normal orientation. :tableflip:
    //  - When rendering objects that sample depth we flip again in the matrix because we rely on cmd.SetViewProjectionMatrices().
    //  - In this case we set bias to 0 and scale to 1.
    //  - uv.y = 0.0 + uv.y * 1.0. No op. All good!
    // If URP is NOT rendering to RT:
    //  - Source Depth is not fliped. We CANNOT flip when copying depth and don't flip when sampling.
    //  - Because we use cmd.Blit() it will flip, so we need unflip uv below by setting
    //  - bias to 1.0 and scale -1.0
    //  - uv.y = 1.0 + uv.y * (-1.0). We unflip the flip in matrix. All good.
    half scale = _ScaleBiasRT.x;
    half bias = _ScaleBiasRT.y;
    //uv.y = bias + uv.y * scale;

#if MSAA_SAMPLES == 1
    return SAMPLE(uv);
#else
    int2 coord = int2(uv * _CameraDepthAttachment_TexelSize.zw);
    float outDepth = DEPTH_DEFAULT_VALUE;

    UNITY_UNROLL
    for (int i = 0; i < MSAA_SAMPLES; ++i)
        outDepth = DEPTH_OP(LOAD(coord, i), outDepth);
    return outDepth;
#endif
}

float frag(Varyings input) : SV_Depth
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    UNITY_SETUP_INSTANCE_ID(input);
    return SampleDepth(input.uv);
}

#endif
