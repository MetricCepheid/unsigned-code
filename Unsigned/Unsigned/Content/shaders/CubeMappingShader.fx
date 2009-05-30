float4x4 World;
float4x4 View;
float4x4 Projection;

float3 LightDirection = float3(0,4,2.5);
float4 LightDiffuse = float4(1,1,1,1);
float4 LightSpecular = float4(1,1,1,1);

float4 Ambient = float4(0,0,0,1);
float4 Diffuse = float4(1,1,1,1);
float4 Specular = float4(1,1,1,1);

float SpecularPower = 1.0f;

float Alpha = 1.0f;

float3 EyePos;

texture diffuseTex : Diffuse;
sampler DiffuseTextureSampler = sampler_state
{
    Texture = (diffuseTex);
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = mirror; 
    AddressV = mirror;
};
texture cubeTex : Diffuse;
samplerCUBE CubeTextureSampler = sampler_state
{
	Texture = <cubeTex>;
};

struct EngineVertexInput
{
    float3 pos : POSITION;
    float2 texCoord : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};
struct EngineVertexToPixel
{
    float4 pos : POSITION;
    float2 texCoord : TEXCOORD0;
    float3 lightCoord : TEXCOORD1;
};
struct EnginePixelIn
{
    float2 texCoord : TEXCOORD0;
    float3 lightCoord : TEXCOORD1;
};

float4 TransformPosition(float4 pos)
{
	return mul(pos, mul(mul(World,View),Projection));
}

EngineVertexToPixel CubeMapVertexShader(EngineVertexInput input)
{
  EngineVertexToPixel output = (EngineVertexToPixel)0;

  output.pos = float4(input.pos,1);
  output.pos = TransformPosition(output.pos);
  
  float4 pos = mul(output.pos,World);
  
  float3 normal = normalize(mul(input.normal,(float3x3)World));
  
  float3 incident = float3(pos.xyz)-EyePos;
  
  float3 reflect = reflect(incident,normal);
  
  output.lightCoord = reflect;
  
  output.texCoord = input.texCoord;

  return output;
}

float4 CubeMapPixelShader(EnginePixelIn input) : COLOR
{
  float4 lightTex = texCUBE(CubeTextureSampler,normalize(input.lightCoord));
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord);

  return float4(diffuseTex.xyz*lightTex.xyz,diffuseTex.w);
}

technique CubeMapTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 CubeMapVertexShader();
        PixelShader = compile ps_2_0 CubeMapPixelShader();
    }
}
