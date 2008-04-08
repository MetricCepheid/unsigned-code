sampler SceneSampler : register(s0) = sampler_state
{    
    MinFilter = Linear;
    MagFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

float blend = 0;

float4 PixelShaderFunction(float2 tc : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(SceneSampler, tc);
	if(tc.y<blend)
		color.w*=tc.y/blend;
    return color;
}

technique Fade
{
    pass pass0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}


