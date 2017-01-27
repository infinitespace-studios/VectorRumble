// Pixel shader applies a one dimensional gaussian blur filter.
// This is used twice by the bloom post-process, first to
// blur horizontally, and then again to blur vertically.
#include "Macros.fxh"

#define SAMPLE_COUNT 15

DECLARE_TEXTURE(Texture, 0);

BEGIN_CONSTANTS
    float2  SampleOffsets[SAMPLE_COUNT];
    float   SampleWeights[SAMPLE_COUNT];
END_CONSTANTS

float4 PixelShaderFunction(float4 position : SV_POSITION, float4 Color : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        c += SAMPLE_TEXTURE(Texture, texCoord + SampleOffsets[i]) * SampleWeights[i];
    }
    
    return c;
}

TECHNIQUE_NO_VS(GaussianBlur, PixelShaderFunction );
