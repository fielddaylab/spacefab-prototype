#ifndef FD_LAYERS_INCLUDED
#define FD_LAYERS_INCLUDED

#include "./Common.cginc"

/// Helpers

inline float4 LayerAlphaTexture(sampler2D alphaTexture, float2 uv, float4 color)
{
    return float4(color.rgb, color.a * SampleSingle(alphaTexture, uv));
}

inline half4 LayerAlphaTexture(sampler2D alphaTexture, half2 uv, half4 color)
{
    return float4(color.rgb, color.a * SampleSingle(alphaTexture, uv));
}

#endif // FD_LAYERS_INCLUDED