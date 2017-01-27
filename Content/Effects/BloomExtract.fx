// Pixel shader extracts the brighter areas of an image.
// This is the first step in applying a bloom post-process.
#include "Macros.fxh"

DECLARE_TEXTURE(Texture, 0);

BEGIN_CONSTANTS
    float 	BloomThreshold;
END_CONSTANTS

float4 PixelShaderFunction(float4 position : SV_POSITION, float4 Color : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    // Look up the original image color.
    float4 c = SAMPLE_TEXTURE(Texture, texCoord);

    // Adjust it to keep only values brighter than the specified threshold.
    return saturate((c - BloomThreshold) / (1 - BloomThreshold));
}

TECHNIQUE_NO_VS(BloomExtract, PixelShaderFunction );
