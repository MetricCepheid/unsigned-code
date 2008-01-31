/*

% HFPS Shader for XNA
% Handles Directional light...

keywords: texture dlight

date: 13.12.07

*/

/* *** *** *** **
   Variables
** *** *** *** */

bool fullbright;
bool vertexAlpha=true;
float wAlpha=1;

float4x4 view : View;
float4x4 proj : Projection;
float4x4 world : World;
float4x4 wRot : World;
float4x4 viewInverse : ViewInverse;

int numPSPLights = 3;
float3 pLightPos[16];
bool   pLightOn[16];
float3 pLightDiffuse[16];
float3 pLightSpecular[16];
float  pLightNear[16];
float  pLightFar[16];

bool BumpMappingEnabled = true;
bool SpecularEnabled = true;

float3 decalPos[8];
float decalRadius[8];
bool decalActivated[8];

float3 dLightDir = { 1, 1, 0 };
float4 dLDiffuseColor : Diffuse = { 0.8f, 0.8f, 0.8f, 1.0f };
float4 dLSpecularColor : Specular = { 0.0f, 0.0f, 0.0f, 1.0f };

float4 ambientColor : Ambient = { 0.1f, 0.1f, 0.1f, 1.0f };
float4 diffuseColor : Diffuse = { 0.5f, 0.5f, 0.5f, 1.0f };
float4 specularColor : Specular = { 1.0f, 1.0f, 1.0f, 1.0f };
float shininess : SpecularPower = 24.0f;




/* *** *** *** **
   Textures
** *** *** *** */

texture diffuseTexture : Diffuse
<
    string ResourceName = "rgbcheck.bmp";
>;
sampler DiffuseTextureSampler = sampler_state
{
    Texture = <diffuseTexture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = mirror; 
    AddressV = mirror;
};
texture decalTexture;
sampler DecalTextureSampler = sampler_state
{
    Texture = <decalTexture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = mirror; 
    AddressV = mirror;
};
texture bumpTexture : Specular
<
    string ResourceName = "TestBM.bmp";
>;
sampler BumpTextureSampler = sampler_state
{
    Texture = <bumpTexture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};







/* *** *** *** **
   Structures
** *** *** *** */

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
    float3 texCoord : TEXCOORD0;//3 is diffuse light val
    float3 wPos : TEXCOORD1;
    float3 viewVec : TEXCOORD2;
    float3x3 tangentMatrix : TEXCOORD3;
    float3 normal : COLOR0;
    float alpha : FOG;
};





/* *** *** *** **
   Functions
** *** *** *** */

// gets the transformation matrix
// to chg world space to tangent
// space for normal mapping
float3x3 ComputeTangentMatrix(float3 tangent, float3 normal)
{
	float3x3 worldToTangentSpace;
	worldToTangentSpace[0] = mul(cross(normal,tangent),wRot);
	worldToTangentSpace[1] = mul(tangent,wRot);
	worldToTangentSpace[2] = mul(normal,wRot);
	return worldToTangentSpace;
}

// transforms a position by
// the WVP
float4 TransformPosition(float3 pos)
{
	return mul(float4(pos,1), mul(mul(world,view),proj));
}

// transforms the position by
// the world matrix and returns
// the result
float3 GetWorldPos(float3 pos)
{
	return mul(float4(pos,1),world).xyz;
}

// gets the location of the cam
float3 GetCameraPos()
{
	return viewInverse[3].xyz;
}


/*void InsideLoop(int i, float3 normalVector, float3 viewVector, EngineVertexToPixel input)
{
  
   
}*/



/* *** *** *** **
    Shaders
** *** *** *** */

EngineVertexToPixel EngineVertexShader(EngineVertexInput input)
{
  EngineVertexToPixel output = (EngineVertexToPixel)0;
  output.pos = TransformPosition(input.pos);
  output.texCoord = float3(input.texCoord.xy,0);
  float3x3 worldToTangentSpace = ComputeTangentMatrix(input.tangent, input.normal);
  
  float3 worldEyePos = GetCameraPos();
  float3 worldVertPos = GetWorldPos(input.pos);
  
  output.wPos = worldVertPos;//mul(input.pos,world);
  if(vertexAlpha)
  {
    output.viewVec = mul(worldToTangentSpace, worldEyePos - worldVertPos);
    output.tangentMatrix = worldToTangentSpace;
    output.alpha = input.alpha;
    output.normal=float3(0,0,1);
  }
  else
  {
    output.viewVec = mul(worldEyePos - worldVertPos,wRot);
    output.tangentMatrix = float3x3(1,0,0,0,1,0,0,0,1);
    output.alpha = 1.0f;
    output.normal=mul(input.normal,wRot);
  }

  for(int i=4;i<16;i++)
  {
    float dist = sqrt( (float)pow(pLightPos[i].x-input.pos.x,2)+(float)pow(pLightPos[i].y-input.pos.y,2)+(float)pow(pLightPos[i].z-input.pos.z,2) );
    if(dist>pLightFar[i])
    {
    float fade=0.0f;
    if(dist<=pLightNear[i])
  	  fade=1.0f;
    else
  	  fade = 1.0f-((dist-pLightNear[i])/(pLightFar[i]-pLightNear[i]));
    output.texCoord.z += fade;
    }
  }

  return output;
}

float4 EnginePixelShader(EngineVertexToPixel input) : COLOR
{
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord.xy);
  
  
  // The following is decal code that is not yet 
  // implemented as it causes other code to crash
  // please ignore for the time being
  /*float3 tanpos = mul(input.wPos,input.tangentMatrix);
  [unroll] for(int i=0;i<8;i++)
  {
	if(decalActivated[i])
	if(sqrt(pow(input.wPos.x-decalPos[i].x,2)+pow(input.wPos.y-decalPos[i].y,2)+pow(input.wPos.z-decalPos[i].z,2))<decalRadius[i])
	{
		float3 tPos = mul(decalPos[i],input.tangentMatrix);
		float2 decalTexCoords = float2(tanpos.x-tPos.x,tanpos.y-tPos.y);
		decalTexCoords /= decalRadius[i] * 2;
		decalTexCoords.x += 0.5f;
		decalTexCoords.y += 0.5f;
		float4 diffuseDecal = tex2D(DecalTextureSampler,decalTexCoords);
		float OldW = diffuseTex.w;
		diffuseTex = (diffuseTex*(1-diffuseDecal.w))+(diffuseDecal*diffuseDecal.w);
		diffuseTex.w = OldW;
	}
  }*/
  
  
  
  float3 normalVector =normalize(input.normal);
  if(BumpMappingEnabled)
  {
	normalVector = (2.0 * tex2D(BumpTextureSampler, input.texCoord.xy).rgb) - 1.0;
	normalVector = normalize(normalVector);
  }//else: normalVector=(0,0,1)
  
  
  float4 ambientCol = ambientColor;//default values for the output
  float4 diffuseCol = diffuseColor*input.texCoord.z;
  float4 specularCol = float4( 0, 0, 0, 0 );
if(!fullbright)
{
  
  float3 viewVector = normalize(input.viewVec);
  
  {// Directional Light
  float3 dLightVector = normalize(mul(input.tangentMatrix, dLightDir));
	float bump = saturate(dot(normalVector, dLightVector));
	float3 reflect = normalize(2 * bump * normalVector - dLightVector);
	float spec = pow(saturate(dot(reflect, viewVector)), shininess);
	diffuseCol = saturate(dot(normalVector, dLightVector))*dLDiffuseColor;
	if(SpecularEnabled)
		specularCol = bump*spec*dLSpecularColor;
  }

  // Point Lights
  [unroll] for (int i=0;i<4;i++)
  {
   if(pLightOn[i])
   {
     float dist = sqrt( (float)pow(pLightPos[i].x-input.wPos.x,2)+(float)pow(pLightPos[i].y-input.wPos.y,2)+(float)pow(pLightPos[i].z-input.wPos.z,2) );
	if(dist <= pLightFar[i])
	{
		float fade=1.0f;
		if(dist<=pLightNear[i])
		  fade=1.0f;
		else
		  fade = 1.0f-((dist-pLightNear[i])/(pLightFar[i]-pLightNear[i]));
		float3 pLightDir = pLightPos[i]-input.wPos;
		float3 pLightVector = normalize(mul(input.tangentMatrix, pLightDir));
		diffuseCol += saturate(dot(normalVector, pLightVector))*fade*diffuseColor*float4(pLightDiffuse[i].xyz,1);
		if(SpecularEnabled)
		{
	          float bump = saturate(dot(normalVector, pLightVector));
	          float3 reflect = normalize(2 * bump * normalVector - pLightVector);
	          float spec = pow(saturate(dot(reflect, viewVector)), shininess);
		  specularCol += fade*saturate(bump*spec*specularColor*float4(pLightSpecular[i].xyz,1));
		}
	}
   }
   else
	break;
  }
}
  else
  {
	ambientCol = float4(ambientColor.xyz,wAlpha*input.alpha);
	diffuseCol = float4(diffuseColor.xyz,wAlpha*input.alpha);
	specularCol = float4(0,0,0,0);
  }
  diffuseTex.w *= wAlpha*input.alpha;

  return float4((diffuseTex * saturate(ambientColor + diffuseCol) + specularCol).xyz,diffuseTex.w*wAlpha*input.alpha);
}





/* *** *** *** **
   Techniques
** *** *** *** */

technique maintechnique {
	pass pass0 {
		VertexShader = compile vs_3_0 EngineVertexShader();
		PixelShader  = compile ps_3_0 EnginePixelShader();
	}
}
