sampler SceneSampler : register(s0) = sampler_state
{    
    MinFilter = Linear;
    MagFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};
texture gradientTex;

sampler gradientTexSampler = sampler_state
{
    Texture = (gradientTex);
    
    MinFilter = Linear;
    MagFilter = Linear;
    
    AddressU = Mirror;
    AddressV = Mirror;
};

bool dotGrainOn=false;
float grainStrength=0;
float time;

float GetRandom(float2 tc)
{
	return (saturate(tex2D(gradientTexSampler,float2(tc*time)).r)-0.5f)*grainStrength;
}

float4 PixelShaderFunction(float2 tc : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(SceneSampler, tc);
	float avg = color.r+color.g+color.b;
	avg/=3.0f;
	if(dotGrainOn)
		avg= avg+GetRandom(tc);
    return float4(avg, avg, avg, 1);
}

technique Desaturate
{
    pass pass0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}


float ValueShift=0;
float HueStrength=1;

float4 PS(float2 tc : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(SceneSampler, tc);
	float rg = color.y-color.x;
	float rb = color.z-color.x;
    float4 ret = float4(color.x+ValueShift,color.x+ValueShift,color.x+ValueShift,color.w);
    ret.y+=rg*HueStrength;
    ret.z+=rb*HueStrength;
    return ret;
}

technique Gamma
{
    pass pass0
    {
        PixelShader = compile ps_2_0 PS();
    }
}