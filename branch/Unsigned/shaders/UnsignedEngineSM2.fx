EngineVertexToPixel EngineVertexShadert(EngineVertexInput input)
{
  EngineVertexToPixel output = (EngineVertexToPixel)0;
  float4x4 rRot;

  if(skinned)
  {
    float4x4 skinTransform = 0;

    skinTransform += Bones[input.BoneIndices.x] * input.BoneWeights.x;
    skinTransform += Bones[input.BoneIndices.y] * input.BoneWeights.y;
    skinTransform += Bones[input.BoneIndices.z] * input.BoneWeights.z;
    skinTransform += Bones[input.BoneIndices.w] * input.BoneWeights.w;

    output.pos = mul(float4(input.pos,1), skinTransform);
    
    rRot = skinTransform*wRot;
  }
  else
  {
    rRot = wRot;
    output.pos = float4(input.pos,1);
  }
  output.pos = TransformPosition(output.pos);
  output.texCoord = float3(input.texCoord.xy,0);
  
  float3 worldEyePos = GetCameraPos();
  float3 worldVertPos = GetWorldPos(input.pos);
  
  output.wPos = worldVertPos;
  if(vertexAlpha)
  {
    output.tangentMatrix = ComputeTangentMatrix(input.tangent, input.normal);
    output.viewVec = mul(output.tangentMatrix, worldEyePos - worldVertPos);
    output.alpha = input.alpha;
    output.normal=float3(0,0,1);
  }
  else
  {
    output.viewVec = mul(worldEyePos - worldVertPos,rRot);
    output.tangentMatrix = float3x3(1,0,0,0,1,0,0,0,1);
    output.alpha = 1.0f;
    output.normal=mul(input.normal,rRot);
  }

  /*for(int i=4;i<16;i++)
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
  }*/

  return output;
}

float4 EnginePixelShadert(EngineVertexToPixel input) : COLOR
{
  float4 diffuseTex = tex2D(DiffuseTextureSampler,input.texCoord.xy);
  
  float3 normalVector =normalize(input.normal);
  
  float4 diffuseCol = diffuseColor*input.texCoord.z;
  return float4((diffuseTex * saturate(ambientColor + diffuseCol)).xyz,diffuseTex.w*wAlpha*input.alpha);
if(!fullbright)
{
  float3 viewVector = normalize(input.viewVec);
  
  /*{// Directional Light
	float3 dLightVector = normalize(mul(input.tangentMatrix, dLightDir));
	float bump = saturate(dot(normalVector, dLightVector));
	float3 reflect = normalize(2 * bump * normalVector - dLightVector);
	float spec = pow(saturate(dot(reflect, viewVector)), shininess);
	diffuseCol = saturate(dot(normalVector, dLightVector))*diffuseColor*dLDiffuseColor;
	if(SpecularEnabled)
	specularCol = bump*spec*specularColor*dLSpecularColor;
  }*/

     float dist = acos(dot(normalize(input.wPos-pLightPos[0]),normalize(pLightDir[0])));
		float3 pLightDir = pLightPos[0]-input.wPos;
		float3 pLightVector = normalize(mul(input.tangentMatrix, pLightDir));
		diffuseCol += saturate(dot(normalVector, pLightVector))*
					  saturate(1.0f-((dist-pLightNear[0])/(pLightFar[0]-pLightNear[0])))*
					  diffuseColor*float4(pLightPower[0],pLightPower[0],pLightPower[0],1);
}
  else
  {
	diffuseCol = float4(diffuseColor.xyz,1);
  }

  return float4((diffuseTex * saturate(ambientColor + diffuseCol)).xyz,diffuseTex.w*wAlpha*input.alpha);
}

technique maintechniquet {
	pass pass0 {
		VertexShader = compile vs_2_0 EngineVertexShadert();
		PixelShader  = compile ps_2_0 EnginePixelShadert();
	}
}