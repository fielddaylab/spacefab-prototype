#ifndef FD_INTENSITY_INCLUDED
#define FD_INTENSITY_INCLUDED

#include "./Common.cginc"

/// Configuration Defines

// FD_INTENSITY_COLOR          Multiplies color by texture intensity
// FD_INTENSITY_ALPHA          Multiplies alpha by texture intensity
// FD_INTENSITY_COLOR_ALPHA    Multiplies color and alpha by texture intensity

/// Uniforms

half _IntensityColorThreshold;
half _IntensityColorMinThreshold;
half _IntensityAlphaThreshold;

/// Helpers

inline void LayerIntensityTextureComponents(sampler2D intensityTexture, float2 uv, out float colorComponent, out float alphaComponent)
{
    float intensity = SampleSingle(intensityTexture, uv);
#if FD_INTENSITY_COLOR || FD_INTENSITY_COLOR_ALPHA
    colorComponent = saturate((intensity - _IntensityColorMinThreshold) / (_IntensityColorThreshold - _IntensityColorMinThreshold));
#else
    colorComponent = 1;
#endif // FD_INTENSITY_COLOR || FD_INTENSITY_COLOR_ALPHA
    
#if FD_INTENSITY_ALPHA || FD_INTENSITY_COLOR_ALPHA
    alphaComponent = saturate(intensity / _IntensityAlphaThreshold);
#else
    alphaComponent = 1;
#endif // FD_INTENSITY_ALPHA || FD_INTENSITY_COLOR_ALPHA
}

inline float4 LayerIntensityTexture(sampler2D intensityTexture, float2 uv)
{
    float colorComponent, alphaComponent;
    LayerIntensityTextureComponents(intensityTexture, uv, colorComponent, alphaComponent);
    return float4(colorComponent, colorComponent, colorComponent, alphaComponent);
}

inline float4 LayerIntensityTexture(sampler2D intensityTexture, float2 uv, float4 color)
{
    return color * LayerIntensityTexture(intensityTexture, uv);
}

#endif // FD_INTENSITY_INCLUDED