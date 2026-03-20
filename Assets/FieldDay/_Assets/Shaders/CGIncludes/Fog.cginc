// Portions from Unity built-in shader source, under MIT license.

#ifndef FD_FOG_INCLUDED
#define FD_FOG_INCLUDED

#include "./Common.cginc"

/// Configuration Defines

// FD_ENABLE_FOG        Enables fog effects

/// Types

#if FD_ENABLE_FOG
    #define VaryingsFog(channel)    UNITY_FOG_COORDS(channel)
#else
    #define VaryingsFog(channel)
#endif // FD_USE_FOG

/// Instancing

/// Uniforms

/// Helpers

#if FD_ENABLE_FOG
    #define     FogTransfer(output, clipPosition)   UNITY_TRANSFER_FOG(output, clipPosition)
    #define     FogApply(color, input)  UNITY_APPLY_FOG(input.fogCoord, color)
#else
    #define     FogTransfer(output, clipPosition)
    #define     FogApply(color, input)
#endif // FD_ENABLE_FOG

#define UnityFogColor()     (unity_FogColor).rgb

/// Programs

#endif // FD_FOG_INCLUDED