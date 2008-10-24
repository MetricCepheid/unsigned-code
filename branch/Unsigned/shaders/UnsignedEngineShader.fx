// Copyright 2008 FV Productions
// Do not redistribute this file without permission from FV Productions

#define NUM_LIGHTS 8

float3 ambientMaterialColor : Ambient = { 0.5f, 0.5f, 0.5f};
float3 diffuseMaterialColor : Diffuse = { 1.0f, 1.0f, 1.0f};
float3 specularMaterialColor : Specular = { 1.0f, 1.0f, 1.0f};
float  materialAlpha = 1.0f;

float3 dLightDir = {0,-1,0}, dLightDiffuse = {0.0f,0.0f,0.0f}, dLightSpecular = {0.0f,0.0f,0.0f};
float shininess = 2.0f;
float alpha = 1.0f;

float3 sLightPos[NUM_LIGHTS], sLightDir[NUM_LIGHTS], sLightDiffuse[NUM_LIGHTS], sLightSpecular[NUM_LIGHTS];
float sLightNarrow[NUM_LIGHTS], sLightWide[NUM_LIGHTS];
int sLightNum = 0;

float4x4 world, wRot, view, viewInverse, projection;

texture diffuseTexture : Diffuse;
sampler DiffuseTextureSampler = sampler_state
{
    Texture = (diffuseTexture);
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = mirror; 
    AddressV = mirror;
};
texture bumpTexture : Specular;
sampler BumpTextureSampler = sampler_state
{
    Texture = <bumpTexture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

struct EngineVertexInput
{
    float3 pos : POSITION;
    float2 texCoord : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float alpha : FOG;
};
struct EngineVertexToPixel
{
    float4 pos : POSITION;
    float2 texCoord : TEXCOORD0;
    float3 wPos : TEXCOORD1;
    float3 viewVec : TEXCOORD2;
    float3x3 tangentMatrix : TEXCOORD3;
    float3 normal : COLOR0;
    float alpha : TEXCOORD6;
};
struct EnginePixelIn
{
    float4 pos : POSITION;
    float2 texCoord : TEXCOORD0;
    float3 wPos : TEXCOORD1;
    float3 viewVec : TEXCOORD2;
    float3x3 tangentMatrix : TEXCOORD3;
    float3 normal : COLOR0;
    float alpha : TEXCOORD6;
};

struct EngineVertexInput_small
{
    float3 pos : POSITION;
    float2 texCoord : TEXCOORD0;
    float alpha : FOG;
};
struct EngineVertexToPixel_small
{
    float4 pos : POSITION;
    float2 texCoord : TEXCOORD0;
    float alpha : COLOR0;
};
struct EnginePixelIn_small
{
    float2 texCoord : TEXCOORD0;
    float alpha : COLOR0;
};

float4 TransformPosition(float4 pos)
{
	return mul(pos, mul(mul(world,view),projection));
}

float3x3 ComputeTangentMatrix(float3 tangent, float3 normal)
{
	float3x3 worldToTangentSpace;
	worldToTangentSpace[0] = mul(cross(normal,tangent),wRot);
	worldToTangentSpace[1] = mul(tangent,wRot);
	worldToTangentSpace[2] = mul(normal,wRot);
	return worldToTangentSpace;
}

float3 GetWorldPos(float3 pos)
{
	return mul(float4(pos,1),world).xyz;
}

float3 GetCameraPos()
{
	return viewInverse[3].xyz;
}

EngineVertexToPixel EngineVertexShader(EngineVertexInput input)
{
  EngineVertexToPixel output = (EngineVertexToPixel)0;
  
  output.pos = float4(input.pos,1);
  output.pos = TransformPosition(output.pos);
  output.texCoord = float3(input.texCoord.xy,0);
  
  float3 worldEyePos = GetCameraPos();
  float3 worldVertPos = GetWorldPos(input.pos);
  
  output.wPos = worldVertPos;
  output.tangentMatrix = ComputeTangentMatrix(input.tangent, input.normal);
  output.viewVec = mul(output.tangentMatrix, worldEyePos - worldVertPos);
  output.alpha = input.alpha;
  output.normal=float3(0,0,1);

  return output;
}

EngineVertexToPixel_small EngineVertexShader_NO_LT(EngineVertexInput_small input)
{
  EngineVertexToPixel_small output = (EngineVertexToPixel_small)0;
  
  output.pos = float4(input.pos,1);
  output.pos = TransformPosition(output.pos);
  output.texCoord = float3(input.texCoord.xy,0);
  output.alpha = input.alpha;

  return output;
}

float4 EnginePixelShader(EngineVertexToPixel input) : COLOR
{
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord.xy);
  
  float3 normalVectorTS = normalize((2.0 * tex2D(BumpTextureSampler, input.texCoord.xy).rgb) - 1.0);
  
  float3 diffuseCol = float3(0,0,0);
  float3 specularCol = float3(0,0,0);
  
  float3 viewVector = normalize(input.viewVec);
  
  {// Directional Light
	float3 dLightVector = normalize(mul(input.tangentMatrix, dLightDir));
	float bump = saturate(dot(normalVectorTS, dLightVector));
	float3 reflect = normalize(2 * bump * normalVectorTS - dLightVector);
	float spec = pow(saturate(dot(reflect, viewVector)), shininess);
	diffuseCol = saturate(dot(normalVectorTS, dLightVector))*dLightDiffuse;
	specularCol = bump*spec*dLightSpecular;
  }

  // Spot Lights
  /*for (int i=0;i<sLightNum;i++)
  {
	float3 sDirToLight = sLightPos[i]-input.wPos;
    float angle = acos(dot(normalize(sDirToLight),-sLightDir[i]));
	float fade = 1.0f-saturate((angle-sLightNarrow[i])/(sLightWide[i]-sLightNarrow[i]));
	
	float3 sLightVector = normalize(mul(input.tangentMatrix, sDirToLight));
	float bump = saturate(dot(normalVectorTS, sLightVector));
	diffuseCol += bump*fade*sLightDiffuse[i];
	
	float3 reflect = normalize(2 * bump * normalVectorTS - sLightVector);
	float spec = pow(saturate(dot(reflect, viewVector)), shininess);
	specularCol += bump*spec*fade*sLightSpecular[i];
  }*/

  return float4((diffuseTex * saturate(ambientMaterialColor.xyz + (diffuseCol.xyz*diffuseMaterialColor.xyz)) + saturate(specularCol.xyz*specularMaterialColor.xyz)).xyz,diffuseTex.w*alpha*input.alpha);
}

float4 EnginePixelShader_NO_NM(EngineVertexToPixel input) : COLOR
{
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord.xy);
  
  float3 normalVectorTS = float3(0,0,1);
  
  float3 diffuseCol = float3(0,0,0);
  float3 specularCol = float3(0,0,0);
  
  float3 viewVector = normalize(input.viewVec);
  
  {// Directional Light
	float3 dLightVector = normalize(mul(input.tangentMatrix, dLightDir));
	float bump = saturate(dot(normalVectorTS, dLightVector));
	float3 reflect = normalize(2 * bump * normalVectorTS - dLightVector);
	float spec = pow(saturate(dot(reflect, viewVector)), shininess);
	diffuseCol = saturate(dot(normalVectorTS, dLightVector))*dLightDiffuse;
	specularCol = bump*spec*dLightSpecular;
  }

  // Spot Lights
  /*for (int i=0;i<sLightNum;i++)
  {
	float3 sDirToLight = sLightPos[i]-input.wPos;
    float angle = acos(dot(normalize(sDirToLight),-sLightDir[i]));
	float fade = 1.0f-saturate((angle-sLightNarrow[i])/(sLightWide[i]-sLightNarrow[i]));
	
	float3 sLightVector = normalize(mul(input.tangentMatrix, sDirToLight));
	float bump = saturate(dot(normalVectorTS, sLightVector));
	diffuseCol += bump*fade*sLightDiffuse[i];
	
	float3 reflect = normalize(2 * bump * normalVectorTS - sLightVector);
	float spec = pow(saturate(dot(reflect, viewVector)), shininess);
	specularCol += bump*spec*fade*sLightSpecular[i];
  }*/

  return float4((diffuseTex * saturate(ambientMaterialColor.xyz + (diffuseCol*diffuseMaterialColor.xyz)) + saturate(specularCol.xyz*specularMaterialColor.xyz)).xyz,diffuseTex.w*alpha*input.alpha);
}

float4 EnginePixelShader_NO_SP(EngineVertexToPixel input) : COLOR
{
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord.xy);
  
  float3 normalVectorTS = normalize((2.0 * tex2D(BumpTextureSampler, input.texCoord.xy).rgb) - 1.0);
  
  float3 diffuseCol = float3(0,0,0);
  
  float3 viewVector = normalize(input.viewVec);
  
  {// Directional Light
	float3 dLightVector = normalize(mul(input.tangentMatrix, dLightDir));
	float bump = saturate(dot(normalVectorTS, dLightVector));
	diffuseCol = saturate(dot(normalVectorTS, dLightVector))*dLightDiffuse;
  }

  // Spot Lights
  /*for (int i=0;i<sLightNum;i++)
  {
	float3 sDirToLight = sLightPos[i]-input.wPos;
    float angle = acos(dot(normalize(sDirToLight),-sLightDir[i]));
	float fade = 1.0f-saturate((angle-sLightNarrow[i])/(sLightWide[i]-sLightNarrow[i]));
	
	float3 sLightVector = normalize(mul(input.tangentMatrix, sDirToLight));
	float bump = saturate(dot(normalVectorTS, sLightVector));
	diffuseCol += bump*fade*sLightDiffuse[i];
  }*/

  return float4((diffuseTex * saturate(ambientMaterialColor.xyz + (diffuseCol*diffuseMaterialColor.xyz))).xyz,diffuseTex.w*alpha*input.alpha);
}

float4 EnginePixelShader_NO_NMSP(EngineVertexToPixel input) : COLOR
{
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord.xy);
  
  float3 normalVectorTS = float3(0,0,1);
  
  float3 diffuseCol = float3(0,0,0);
  
  float3 viewVector = normalize(input.viewVec);
  
  {// Directional Light
	float3 dLightVector = normalize(mul(input.tangentMatrix, dLightDir));
	float bump = saturate(dot(normalVectorTS, dLightVector));
	diffuseCol = saturate(dot(normalVectorTS, dLightVector))*dLightDiffuse;
  }

  // Spot Lights
  /*for (int i=0;i<sLightNum;i++)
  {
	float3 sDirToLight = sLightPos[i]-input.wPos;
    float angle = acos(dot(normalize(sDirToLight),-sLightDir[i]));
	float fade = 1.0f-saturate((angle-sLightNarrow[i])/(sLightWide[i]-sLightNarrow[i]));
	
	float3 sLightVector = normalize(mul(input.tangentMatrix, sDirToLight));
	float bump = saturate(dot(normalVectorTS, sLightVector));
	diffuseCol += bump*fade*sLightDiffuse[i];
  }*/

  return float4((diffuseTex * saturate(ambientMaterialColor.xyz + (diffuseCol*diffuseMaterialColor.xyz))).xyz,diffuseTex.w*alpha*input.alpha);
}

float4 EnginePixelShader_NO_LT(EngineVertexToPixel_small input) : COLOR
{
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord.xy);

  return float4(diffuseTex.xyz,diffuseTex.w*alpha*input.alpha);
}

technique maintechnique {
	pass pass0 {
		VertexShader = compile vs_3_0 EngineVertexShader();
		PixelShader  = compile ps_3_0 EnginePixelShader();
	}
}

technique maintechnique_NO_SP {
	pass pass0 {
		VertexShader = compile vs_3_0 EngineVertexShader();
		PixelShader  = compile ps_3_0 EnginePixelShader_NO_SP();
	}
}

technique maintechnique_NO_NM {
	pass pass0 {
		VertexShader = compile vs_3_0 EngineVertexShader();
		PixelShader  = compile ps_3_0 EnginePixelShader_NO_NM();
	}
}

technique maintechnique_NO_NMSP {
	pass pass0 {
		VertexShader = compile vs_3_0 EngineVertexShader();
		PixelShader  = compile ps_3_0 EnginePixelShader_NO_NMSP();
	}
}

technique maintechnique_NO_LT {
	pass pass0 {
		VertexShader = compile vs_1_1 EngineVertexShader_NO_LT();
		PixelShader  = compile ps_1_1 EnginePixelShader_NO_LT();
	}
}