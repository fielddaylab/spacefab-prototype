#ifndef FD_LAYERS_INCLUDED
#define FD_LAYERS_INCLUDED

#include "./Common.cginc"

/// Helpers

inline float4 LayerAlphaTexture(sampler2D alphaTexture, float2 uv, float4 color)
{
    return float4(color.rgb, color.a * SampleSingle(alphaTexture, uv));
}

inline float4 LayerIntensityTexture(sampler2D intensityTexture, float2 uv, float4 color)
{
    float intensity = SampleSingle(intensityTexture, uv);
    return float4(color.rgb * intensity, color.a);
}

inline float4 LayerIntensityAlphaTexture(sampler2D intensityTexture, float2 uv, float4 color)
{
    float intensity = SampleSingle(intensityTexture, uv);
    return float4(color.rgb * intensity, color.a * step(0.001, intensity));
}

#define LayerLerpColor(baseColor, lerpColor)    baseColor.rgb = lerp(baseColor.rgb, lerpColor.rgb, lerpColor.a)
#define LayerAdditiveColor(baseColor, additiveColor)    baseColor.rgb += (additiveColor).rgb * ((additiveColor).a * (baseColor).a)

#endif // FD_LAYERS_INCLUDED