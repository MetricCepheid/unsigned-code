float4x4 World;
float4x4 View;
float4x4 Projection;

float3 LightDirection = float3(0,4,2.5);

float4 Ambient = float4(0,0,0,1);
float4 Diffuse = float4(1,1,1,1);
float4 Specular = float4(1,1,1,1);

float SpecularPower = 1.0f;

float Alpha = 1.0f;

float3 EyePos;

float CELL_LEVELS = 4.0f;

texture2D normalTex;
sampler2D NormalMapSampler = sampler_state
{
    Texture = <normalTex>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = wrap;
    AddressV = wrap;
};

texture2D diffuseTex;
sampler2D DiffuseTextureSampler = sampler_state
{
    Texture = <diffuseTex>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = wrap;
    AddressV = wrap;
};

texture2D specularTex;
sampler2D SpecularTextureSampler = sampler_state
{
    Texture = <specularTex>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = wrap;
    AddressV = wrap;
};

struct VS_INPUT
{
    float3 position            : POSITION0;
    float2 texCoord            : TEXCOORD0;
    float3 normal            : NORMAL0;    
    float3 binormal            : BINORMAL0;
    float3 tangent            : TANGENT0;
};

// output from the vertex shader, and input to the pixel shader.
// lightDirection and viewDirection are in world space.
// NOTE: even though the tangentToWorld matrix is only marked 
// with TEXCOORD3, it will actually take TEXCOORD3, 4, and 5.
struct VS_OUTPUT
{
    float4 position            : POSITION0;
    float2 texCoord            : TEXCOORD0;
    float3 worldPosition       : TEXCOORD2;
    float3x3 tangentToWorld    : TEXCOORD3;
};

VS_OUTPUT VertexShader( VS_INPUT input )
{
    VS_OUTPUT output;
    
    // transform the position into projection space
    float4 worldSpacePos = mul(float4(input.position,1), World);
    output.position = mul(worldSpacePos, View);
    output.position = mul(output.position, Projection);
    
    output.worldPosition = worldSpacePos;
    
    // calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors.  the pixel shader will normalize these
    // in case the world matrix has scaling.
    output.tangentToWorld[0] = mul(input.tangent, (float3x3)World);
    output.tangentToWorld[1] = mul(input.binormal, (float3x3)World);
    output.tangentToWorld[2] = mul(input.normal, (float3x3)World);
    
    // pass the texture coordinate through without additional processing
    output.texCoord = input.texCoord;
    
    return output;
}

float4 PixelShader( VS_OUTPUT input ) : COLOR0
{
    // look up the normal from the normal map, and transform from tangent space
    // into world space using the matrix created above.  normalize the result
    // in case the matrix contains scaling.
    float3 normalFromMap = tex2D(NormalMapSampler, input.texCoord)*2-1;
    normalFromMap = mul(normalFromMap, input.tangentToWorld);
    normalFromMap = normalize(normalFromMap);
    
    // clean up our inputs a bit
    float3 viewDirection = normalize(input.worldPosition-EyePos);
    float3 lightDirection = LightDirection;    
    
    // use the normal we looked up to do phong diffuse style lighting.    
    float nDotL = max(dot(normalFromMap, lightDirection), 0);
    float4 diffuse = saturate(Diffuse * nDotL);
    
    // use phong to calculate specular highlights: reflect the incoming light
    // vector off the normal, and use a dot product to see how "similar"
    // the reflected vector is to the view vector.    
    float3 reflectedLight = reflect(lightDirection, normalFromMap);
    float rDotV = max(dot(reflectedLight, viewDirection), 0);
    float3 specInfo = tex2D(SpecularTextureSampler,input.texCoord);
    float4 specular = specInfo.r * Specular * pow(rDotV, specInfo.g*SpecularPower);
    
    float4 diffuseTexture = tex2D(DiffuseTextureSampler, input.texCoord);
    
    // return the combined result.
    return float4(((saturate(diffuse + Ambient) * diffuseTexture) + (specular)).rgb,diffuseTexture.a*Alpha);
}



float4 PixelShader_NO_LT( VS_OUTPUT input ) : COLOR0
{    
    float4 diffuseTexture = tex2D(DiffuseTextureSampler, input.texCoord);
    
    // return the combined result
    return float4((Ambient * diffuseTexture).rgb,diffuseTexture.a*Alpha);
}

float4 PixelShader_NO_NM( VS_OUTPUT input ) : COLOR0
{
    float3 normalFromMap = float3(0,0,1);
    normalFromMap = mul(normalFromMap, input.tangentToWorld);
    normalFromMap = normalize(normalFromMap);
    
    // clean up our inputs a bit
    float3 viewDirection = normalize(input.worldPosition-EyePos);
    float3 lightDirection = LightDirection;    
    
    // use the normal we looked up to do phong diffuse style lighting.    
    float nDotL = max(dot(normalFromMap, lightDirection), 0);
    float4 diffuse = saturate(Diffuse * nDotL);
    
    // use phong to calculate specular highlights: reflect the incoming light
    // vector off the normal, and use a dot product to see how "similar"
    // the reflected vector is to the view vector.    
    float3 reflectedLight = reflect(lightDirection, normalFromMap);
    float rDotV = max(dot(reflectedLight, viewDirection), 0);
    float3 specInfo = tex2D(SpecularTextureSampler,input.texCoord);
    float4 specular = specInfo.r * Specular * pow(rDotV, specInfo.g*SpecularPower);
    
    float4 diffuseTexture = tex2D(DiffuseTextureSampler, input.texCoord);
    
    // return the combined result.
    return float4(((saturate(diffuse + Ambient) * diffuseTexture) + specular).rgb,diffuseTexture.a*Alpha);
}

float4 PixelShader_NO_SP( VS_OUTPUT input ) : COLOR0
{
    // look up the normal from the normal map, and transform from tangent space
    // into world space using the matrix created above.  normalize the result
    // in case the matrix contains scaling.
    float3 normalFromMap = tex2D(NormalMapSampler, input.texCoord)*2-1;
    normalFromMap = mul(normalFromMap, input.tangentToWorld);
    normalFromMap = normalize(normalFromMap);
    
    // clean up our inputs a bit
    float3 viewDirection = normalize(input.worldPosition-EyePos);
    float3 lightDirection = LightDirection;    
    
    // use the normal we looked up to do phong diffuse style lighting.    
    float nDotL = max(dot(normalFromMap, lightDirection), 0);
    float4 diffuse = saturate(Diffuse * nDotL);
    
    float4 diffuseTexture = tex2D(DiffuseTextureSampler, input.texCoord);
    
    // return the combined result.
    return float4(((saturate(diffuse + Ambient) * diffuseTexture)).rgb,diffuseTexture.a*Alpha);
}

float4 PixelShader_NO_NMSP( VS_OUTPUT input ) : COLOR0
{
    float3 normalFromMap = float3(0,0,1);
    normalFromMap = mul(normalFromMap, input.tangentToWorld);
    normalFromMap = normalize(normalFromMap);
    
    // clean up our inputs a bit
    float3 lightDirection = LightDirection;    
    
    // use the normal we looked up to do phong diffuse style lighting.    
    float nDotL = max(dot(normalFromMap, lightDirection), 0);
    float4 diffuse = saturate(Diffuse * nDotL);
    
    float4 diffuseTexture = tex2D(DiffuseTextureSampler, input.texCoord);
    
    // return the combined result.
    return float4(((saturate(diffuse + Ambient) * diffuseTexture)).rgb,diffuseTexture.a*Alpha);
}



float4 PixelShader_CARTOON( VS_OUTPUT input ) : COLOR0
{
    float3 normalFromMap = tex2D(NormalMapSampler, input.texCoord)*2-1;
    normalFromMap = mul(normalFromMap, input.tangentToWorld);
    normalFromMap = normalize(normalFromMap);
    
    // clean up our inputs a bit
    float3 viewDirection = normalize(input.worldPosition-EyePos);
    float3 lightDirection = LightDirection;    
    
    // use the normal we looked up to do phong diffuse style lighting.    
    float nDotL = max(dot(normalFromMap, lightDirection), 0);
    
    // use phong to calculate specular highlights: reflect the incoming light
    // vector off the normal, and use a dot product to see how "similar"
    // the reflected vector is to the view vector.    
    float3 reflectedLight = reflect(lightDirection, normalFromMap);
    float rDotV = max(dot(reflectedLight, viewDirection), 0);
    float3 specInfo = tex2D(SpecularTextureSampler,input.texCoord);
    float specularV = specInfo.r * pow(rDotV, specInfo.g*SpecularPower);
    
    float totalValue = Ambient.r+nDotL+specularV;
    float aPC = Ambient.r/totalValue;
    float dPC = nDotL/totalValue;
    float sPC = specularV/totalValue;
    totalValue = (int)(totalValue*CELL_LEVELS)/CELL_LEVELS;
    
    float4 ambient = aPC*totalValue;
    float4 diffuse = saturate(Diffuse * (dPC*totalValue));
    float4 specular = Specular * (sPC*totalValue);
    
    float4 diffuseTexture = tex2D(DiffuseTextureSampler, input.texCoord);
    
    // return the combined result.
    return float4(((saturate(diffuse + ambient) * diffuseTexture) + specular).rgb,diffuseTexture.a*Alpha);
}

Technique maintechnique
{
    Pass Go
    {
        VertexShader = compile vs_1_1 VertexShader();
        PixelShader = compile ps_2_0 PixelShader();
    }
}

Technique maintechnique_NO_LT
{
    Pass Go
    {
        VertexShader = compile vs_1_1 VertexShader();
        PixelShader = compile ps_2_0 PixelShader_NO_LT();
    }
}

Technique maintechnique_NO_NM
{
    Pass Go
    {
        VertexShader = compile vs_1_1 VertexShader();
        PixelShader = compile ps_2_0 PixelShader_NO_NM();
    }
}

Technique maintechnique_NO_SP
{
    Pass Go
    {
        VertexShader = compile vs_1_1 VertexShader();
        PixelShader = compile ps_2_0 PixelShader_NO_SP();
    }
}

Technique maintechnique_NO_NMSP
{
    Pass Go
    {
        VertexShader = compile vs_1_1 VertexShader();
        PixelShader = compile ps_2_0 PixelShader_NO_NMSP();
    }
}

Technique maintechnique_CARTOON
{
    Pass Go
    {
        VertexShader = compile vs_1_1 VertexShader();
        PixelShader = compile ps_2_0 PixelShader_CARTOON();
    }
}