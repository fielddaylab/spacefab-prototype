// Portions from Unity built-in shader source, under MIT license.

#ifndef FD_FORMATS_INCLUDED
#define FD_FORMATS_INCLUDED

#include "./Common.cginc"

/// Configuration Defines

/// Types

struct Attributes_Position
{
    float4 vertex   : POSITION;
    AttributesInstancing()
};

struct Varyings_Position
{
    float4 vertex   : SV_POSITION;
    VaryingsStereo()
};

#endif // FD_FORMATS_INCLUDED