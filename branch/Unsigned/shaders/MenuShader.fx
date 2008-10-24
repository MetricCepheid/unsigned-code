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

float3 pLightPos[NUM_LIGHTS], pLightDiffuse[NUM_LIGHTS], pLightSpecular[NUM_LIGHTS];
float pLightNear[NUM_LIGHTS], pLightFar[NUM_LIGHTS];

float4x4 world, wRot, view, viewInverse, projection;

int pLightNum = 0;

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
texture cubeTexture : Diffuse;
samplerCUBE CubeTextureSampler = sampler_state
{
	Texture = <cubeTexture>;
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
    float3 normal : NORMAL;
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
struct EngineVertexToPixel_cube
{
    float4 pos : POSITION;
    float2 texCoord : TEXCOORD0;
    float3 lightCoord : TEXCOORD1;
    float alpha : COLOR1;
};
struct EnginePixelIn_cube
{
    float2 texCoord : TEXCOORD0;
    float3 lightCoord : TEXCOORD1;
    float alpha : COLOR1;
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



//*************************************************
// SHADERS
//*************************************************

//------------------------------------------------------------------------------------SPHERE MAPPING

EngineVertexToPixel_cube CubeMapVertexShader(EngineVertexInput_small input)
{
  EngineVertexToPixel_cube output = (EngineVertexToPixel_cube)0;

  output.pos = float4(input.pos,1);
  output.pos = TransformPosition(output.pos);
  
  float4 pos = mul(output.pos,world);
  
  float3 normal = normalize(mul(input.normal,wRot));
  
  float3 incident = float3(pos.xyz)-GetCameraPos();
  
  float3 reflect = incident-(2*dot(incident,normal)*normal);
  
  output.lightCoord = normalize(float3(normal.xyz));
  
  output.texCoord = input.texCoord;
  
  output.alpha = input.alpha;

  return output;
}

float4 CubeMapPixelShader(EnginePixelIn_cube input) : COLOR
{
  // the texture data
  float4 lightTex = texCUBE(CubeTextureSampler,input.lightCoord);
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord);

  return float4(diffuseTex.xyz*lightTex.xyz,diffuseTex.w*input.alpha);
}

//------------------------------------------------------------------------------------NO LIGHTING

EngineVertexToPixel_small MenuVertexShader_NO_LT(EngineVertexInput_small input)
{
  EngineVertexToPixel_small output = (EngineVertexToPixel_small)0;

  output.pos = float4(input.pos,1);
  output.pos = TransformPosition(output.pos);
  
  output.texCoord = input.texCoord;
  
  output.alpha = input.alpha;

  return output;
}

float4 MenuPixelShader_NO_LT(EnginePixelIn_small input) : COLOR
{
  // the texture data
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord);

  return float4(diffuseTex.xyz,diffuseTex.w*input.alpha);
}

//------------------------------------------------------------------------------------NO NORMAL MAPPING

EngineVertexToPixel MenuVertexShader_NO_NM(EngineVertexInput input)
{
  EngineVertexToPixel output = (EngineVertexToPixel)0;
  float4x4 rRot;
  
  rRot = wRot;
  output.pos = float4(input.pos,1);
  output.pos = TransformPosition(output.pos);
  
  output.normal=mul(input.normal,wRot);
  
  output.texCoord = input.texCoord;
  
  output.wPos = GetWorldPos(input.pos);
  
  float3 worldEyePos = GetCameraPos();
  output.viewVec = worldEyePos - output.wPos;
  output.alpha = input.alpha;

  return output;
}

float4 MenuPixelShader_NO_NM(EngineVertexToPixel input) : COLOR
{
  // the texture data
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord);
  
  // the normal in world space (pointless?)
  float3 normalVector =normalize(input.normal);
  
  // normalize the view vector (because of interpolation)
  float3 viewVector = normalize(input.viewVec);
  
  float3 diffuseCol = float3(0,0,0);
  float3 specularCol = float3(0,0,0);
  
  // directional lighting

  {
    float3 dLightVector = normalize(dLightDir);
	float bump = saturate(dot(normalVector, dLightVector));
	float3 reflect = normalize(2 * bump * normalVector - dLightVector);
	float spec = pow(saturate(dot(reflect, viewVector)), shininess);
	diffuseCol = saturate(bump*dLightDiffuse);
	specularCol = bump*spec*dLightSpecular;
  }

  // Point Lights
  /*float dist, fade;
  float3 pLightVector;
  for (int i=0;i<pLightNum;i++)
  {
		dist = sqrt( (float)pow(pLightPos[i].x-input.wPos.x,2)+(float)pow(pLightPos[i].y-input.wPos.y,2)+(float)pow(pLightPos[i].z-input.wPos.z,2) );
		fade = 1.0f-saturate((dist-pLightNear[i])/(pLightFar[i]-pLightNear[i]));
		
		pLightVector = normalize(pLightPos[i]-input.wPos);
	    float bump = saturate(dot(normalVector, pLightVector));
	    
		diffuseCol.xyz += saturate(bump*fade*pLightDiffuse[i].xyz);
	    float3 reflect = normalize(2 * bump * normalVector - pLightVector);
	    float spec = pow(saturate(dot(reflect, viewVector)), shininess);
		specularCol.xyz += fade*saturate(bump*spec*pLightSpecular[i].xyz);
   }*/

  return float4((diffuseTex * saturate(ambientMaterialColor.xyz + (diffuseMaterialColor.xyz*diffuseCol.xyz)) + saturate(specularMaterialColor.xyz*specularCol.xyz)).xyz,1);//diffuseTex.w*alpha*input.alpha);
}

float4 MenuPixelShader_NO_NMSP(EngineVertexToPixel input) : COLOR
{
  // the texture data
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord);
  
  // the normal in world space
  float3 normalVector =normalize(input.normal);
  
  float3 diffuseCol = float3(0,0,0);
  
  // directional lighting
  {
	float bump = saturate(dot(normalVector, dLightDir));
	diffuseCol = saturate(bump*dLightDiffuse);
  }

  // Point Lights
  /*float dist, fade;
  float3 pLightVector;
  for (int i=0;i<pLightNum;i++)
  {
		dist = sqrt( (float)pow(pLightPos[i].x-input.wPos.x,2)+(float)pow(pLightPos[i].y-input.wPos.y,2)+(float)pow(pLightPos[i].z-input.wPos.z,2) );
		fade = 1.0f-saturate((dist-pLightNear[i])/(pLightFar[i]-pLightNear[i]));
		
		pLightVector = normalize(pLightPos[i]-input.wPos);
	    float bump = saturate(dot(normalVector, pLightVector));
	    
		diffuseCol.xyz += saturate(bump*fade*pLightDiffuse[i].xyz);
   }*/

  return float4((diffuseTex * saturate(ambientMaterialColor.xyz + (diffuseMaterialColor.xyz*diffuseCol.xyz))).xyz,1);//diffuseTex.w*alpha*input.alpha);
}


//------------------------------------------------------------------------------------FULL SUPPORT

EngineVertexToPixel MenuVertexShader(EngineVertexInput input)
{
  EngineVertexToPixel output = (EngineVertexToPixel)0;
  float4x4 rRot;
  
  rRot = wRot;
  output.pos = float4(input.pos,1);
  output.pos = TransformPosition(output.pos);
  
  //output.normal=mul(input.normal,rRot);
  
  output.texCoord = input.texCoord;
  float3x3 worldToTangentSpace = ComputeTangentMatrix(input.tangent, input.normal);
  
  output.wPos = GetWorldPos(input.pos);
  
  float3 worldEyePos = GetCameraPos();
  output.viewVec = mul(worldToTangentSpace, worldEyePos - output.wPos);
  output.tangentMatrix = worldToTangentSpace;
  output.alpha = input.alpha;

  return output;
}

float4 MenuPixelShader(EngineVertexToPixel input) : COLOR
{
  // the texture data
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord);
  
  // the normal in world space (pointless?)
  // float3 normalVector =normalize(input.normal);
  
  // the normal in tangent space
  float3 normalVectorTS = normalize((2.0 * tex2D(BumpTextureSampler, input.texCoord).rgb) - 1.0);
  
  // normalize the view vector (because of interpolation)
  float3 viewVector = normalize(input.viewVec);
  
  float3 diffuseCol = float3(0,0,0);
  float3 specularCol = float3(0,0,0);
  
  // directional lighting

  {
    float3 dLightVector = normalize(mul(input.tangentMatrix, dLightDir));
	float bump = saturate(dot(normalVectorTS, dLightVector));
	float3 reflect = normalize(2 * bump * normalVectorTS - dLightVector);
	float spec = pow(saturate(dot(reflect, viewVector)), shininess);
	diffuseCol = saturate(bump*dLightDiffuse);
	specularCol = bump*spec*dLightSpecular;
  }

  // Point Lights
  /*float dist, fade;
  float3 pLightVector;
  for (int i=0;i<pLightNum;i++)
  {
		dist = sqrt( (float)pow(pLightPos[i].x-input.wPos.x,2)+(float)pow(pLightPos[i].y-input.wPos.y,2)+(float)pow(pLightPos[i].z-input.wPos.z,2) );
		fade = 1.0f-saturate((dist-pLightNear[i])/(pLightFar[i]-pLightNear[i]));
		
		pLightVector = normalize(mul(input.tangentMatrix, pLightPos[i]-input.wPos));
	    float bump = saturate(dot(normalVectorTS, pLightVector));
	    
		diffuseCol.xyz += saturate(bump*fade*pLightDiffuse[i].xyz);
	    float3 reflect = normalize(2 * bump * normalVectorTS - pLightVector);
	    float spec = pow(saturate(dot(reflect, viewVector)), shininess);
		specularCol.xyz += fade*saturate(bump*spec*pLightSpecular[i].xyz);
   }*/

  return float4((diffuseTex * saturate(ambientMaterialColor.xyz + (diffuseMaterialColor.xyz*diffuseCol.xyz)) + saturate(specularMaterialColor.xyz*specularCol.xyz)).xyz,1);//diffuseTex.w*alpha*input.alpha);
}

float4 MenuPixelShader_NO_SP(EngineVertexToPixel input) : COLOR
{
  // the texture data
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord);
  
  // the normal in world space (pointless?)
  // float3 normalVector =normalize(input.normal);
  
  // the normal in tangent space
  float3 normalVectorTS = normalize((2.0 * tex2D(BumpTextureSampler, input.texCoord).rgb) - 1.0);
  
  // normalize the view vector (because of interpolation)
  float3 viewVector = normalize(input.viewVec);
  
  float3 diffuseCol = float3(0,0,0);
  float3 specularCol = float3(0,0,0);
  
  // directional lighting

  {
    float3 dLightVector = normalize(mul(input.tangentMatrix, dLightDir));
	float bump = saturate(dot(normalVectorTS, dLightVector));
	diffuseCol = saturate(bump*dLightDiffuse);
  }

  // Point Lights
  /*float dist, fade;
  float3 pLightVector;
  for (int i=0;i<pLightNum;i++)
  {
		dist = sqrt( (float)pow(pLightPos[i].x-input.wPos.x,2)+(float)pow(pLightPos[i].y-input.wPos.y,2)+(float)pow(pLightPos[i].z-input.wPos.z,2) );
		fade = 1.0f-saturate((dist-pLightNear[i])/(pLightFar[i]-pLightNear[i]));
		
		pLightVector = normalize(mul(input.tangentMatrix, pLightPos[i]-input.wPos));
	    float bump = saturate(dot(normalVectorTS, pLightVector));
	    
		diffuseCol.xyz += saturate(bump*fade*pLightDiffuse[i].xyz);
   }*/

  return float4((diffuseTex * saturate(ambientMaterialColor.xyz + (diffuseMaterialColor.xyz*diffuseCol.xyz)) + saturate(specularMaterialColor.xyz*specularCol.xyz)).xyz,1);//diffuseTex.w*alpha*input.alpha);
}












technique menutechnique {
	pass pass0 {
		VertexShader = compile vs_1_1 MenuVertexShader();
		PixelShader  = compile ps_2_0 MenuPixelShader();
	}
}

technique menutechnique_NO_NM {
	pass pass0 {
		VertexShader = compile vs_1_1 MenuVertexShader_NO_NM();
		PixelShader  = compile ps_2_0 MenuPixelShader_NO_NM();
	}
}

technique menutechnique_NO_SP {
	pass pass0 {
		VertexShader = compile vs_1_1 MenuVertexShader();
		PixelShader  = compile ps_2_0 MenuPixelShader_NO_SP();
	}
}

technique menutechnique_NO_NMSP {
	pass pass0 {
		VertexShader = compile vs_1_1 MenuVertexShader_NO_NM();
		PixelShader  = compile ps_2_0 MenuPixelShader_NO_NMSP();
	}
}

technique menutechnique_NO_LT {
	pass pass0 {
		VertexShader = compile vs_1_1 MenuVertexShader_NO_LT();
		PixelShader  = compile ps_1_1 MenuPixelShader_NO_LT();
	}
}

technique spheremappingtechnique {
	pass pass0 {
		VertexShader = compile vs_1_1 CubeMapVertexShader();
		PixelShader  = compile ps_1_1 CubeMapPixelShader();
	}
}