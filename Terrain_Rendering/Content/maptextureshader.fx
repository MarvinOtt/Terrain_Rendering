#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;
static const float PI = 3.14159265f;
float viewdeepness, surfacetransparancy;
float3 EyePosition;
// Light related
float4 AmbientColor;
float AmbientIntensity;
float4 Color;
float3 LightDirection;
float4 DiffuseColor;
float DiffuseIntensity;
bool reflectionrendering;
float4 SpecularColor;
float SpecularIntensity;

float wellenbreite;
float waveverschiebung;
float wellenhohe;
int texabm = 2048;

//Textures
static const int maxtexanz = 4;
int texanz = 4;

Texture2D watertex;

texture Texture1;
sampler2D textureSampler1 = sampler_state {
	Texture = (Texture1);
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};
texture Texture2;
sampler2D textureSampler2 = sampler_state {
	Texture = (Texture2);
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};
texture Texture3;
sampler2D textureSampler3 = sampler_state {
	Texture = (Texture3);
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};
texture Texture4;
sampler2D textureSampler4 = sampler_state {
	Texture = (Texture4);
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};
texture sandnormaltex;
sampler2D sandnormalsamp = sampler_state {
	Texture = (sandnormaltex);
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};
texture wetsand_tex;
sampler2D wetsand_texsamp = sampler_state {
	Texture = (wetsand_tex);
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};
texture wetsand_normal;
sampler2D wetsand_normalsamp = sampler_state {
	Texture = (wetsand_normal);
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};
texture wetsand_specular;
sampler2D wetsand_specularsamp = sampler_state {
	Texture = (wetsand_specular);
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

// The input for the VertexShader
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Texweights : COLOR0;
};

// The output from the vertex shader, used for later processing
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 worldpos : POSITION1;
	float3 Normal : TEXCOORD0;
	float3 View : TEXCOORD1;
	float4 Texweights : COLOR0;
};

float fade(float t)
{
	return (t * t * t * (t * (t * 6 - 15) + 10));         // 6t^5 - 15t^4 + 10t^3
}

// The VertexShader
VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float3 Normal : NORMAL)
{
	VertexShaderOutput OUT;
	float4 worldPosition = mul(input.Position, World);
	OUT.worldpos = worldPosition;

	if (reflectionrendering == true)
		if (worldPosition.y < 0)
			worldPosition.y = 0;
	

	float4 viewPosition = mul(worldPosition, View);
	OUT.Position = mul(viewPosition, Projection);
	float3 normal = normalize(mul(Normal, World));
	OUT.Normal = normal;
	OUT.View = normalize(EyePosition - worldPosition);
	OUT.Texweights = input.Texweights;
	return OUT;
}

struct PixelShaderOutput
{
	float4 Col1 : COLOR0;
	float4 Col2 : COLOR1;
	float4 Col3 : COLOR2;
};

float Dither(float3 uv)
{
	float4 magic = { 0.06711056, 0.00583715, 0.739567264, 52.9829189 };
	return frac(magic.w * frac(dot(uv, magic.xyz)));
}

float getheightatpos(float xi, float yi)
{
	xi = xi / wellenbreite;
	yi = yi / wellenbreite;
	xi = abs(xi);
	yi = abs(yi);
	float y = (yi + waveverschiebung);
	float x = xi;
	float y2 = yi;
	float x2 = (xi + waveverschiebung*1.2324f);
	float y3 = abs(yi - waveverschiebung / 1.532f);
	float x3 = abs(xi - waveverschiebung / 1.4219f);
	uint intx = (uint)x;
	uint inty = (uint)y;
	uint intx2 = (uint)x2;
	uint inty2 = (uint)y2;
	uint intx3 = (uint)x3;
	uint inty3 = (uint)y3;

	uint2 pos00 = { intx % texabm, inty % texabm };
	uint2 pos10 = { (intx + 1) % texabm, inty % texabm };
	uint2 pos01 = { intx % texabm, (inty + 1) % texabm };
	uint2 pos11 = { (intx + 1) % texabm, (inty + 1) % texabm };

	uint2 pos002 = { intx2 % texabm, inty2 % texabm };
	uint2 pos102 = { (intx2 + 1) % texabm, inty2 % texabm };
	uint2 pos012 = { intx2 % texabm, (inty2 + 1) % texabm };
	uint2 pos112 = { (intx2 + 1) % texabm, (inty2 + 1) % texabm };

	uint2 pos003 = { intx3 % texabm, inty3 % texabm };
	uint2 pos103 = { (intx3 + 1) % texabm, inty3 % texabm };
	uint2 pos013 = { intx3 % texabm, (inty3 + 1) % texabm };
	uint2 pos113 = { (intx3 + 1) % texabm, (inty3 + 1) % texabm };

	float xanteil = x - ((int)x);
	float yanteil = y - ((int)y);
	float xanteil2 = x2 - ((int)x2);
	float yanteil2 = y2 - ((int)y2);
	float xanteil3 = x3 - ((int)x3);
	float yanteil3 = y3 - ((int)y3);
	float height = 0;
	height += (1 - xanteil) * watertex[pos00].r * (1 - yanteil);
	height += (xanteil)* watertex[pos10].r * (1 - yanteil);
	height += (1 - xanteil) * watertex[pos01].r * (yanteil);
	height += (xanteil)* watertex[pos11].r * (yanteil);

	height += (1 - xanteil2) * watertex[pos002].r * (1 - yanteil2);
	height += (xanteil2)* watertex[pos102].r * (1 - yanteil2);
	height += (1 - xanteil2) * watertex[pos012].r * (yanteil2);
	height += (xanteil2)* watertex[pos112].r * (yanteil2);

	height += (1 - xanteil3) * watertex[pos003].r * (1 - yanteil3);
	height += (xanteil3)* watertex[pos103].r * (1 - yanteil3);
	height += (1 - xanteil3) * watertex[pos013].r * (yanteil3);
	height += (xanteil3)* watertex[pos113].r * (yanteil3);

	return (height / 3.0f) - 0.5f;
}

// The Pixel Shader
PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) : COLOR
{
	PixelShaderOutput OUT;
	float3 normal = input.Normal;
	
	// Getting Texture Color
	float4 texcols[maxtexanz];
	float staerken[maxtexanz];
	staerken[0] = input.Texweights.r;
	staerken[1] = input.Texweights.g;
	staerken[2] = input.Texweights.b;
	staerken[3] = input.Texweights.a;

	float z = input.worldpos.z;
	float x = input.worldpos.x;
	//z -= ((int)(z / 1000.0f)) * 1000.0f;
	//x -= ((int)(x / 1000.0f)) * 1000.0f;

	float2 texcoo = float2((x) / 3.5f, (z) / 3.5f);

	texcols[0] = tex2D(textureSampler1, texcoo);
	texcols[1] = tex2D(textureSampler2, texcoo);
	texcols[2] = tex2D(textureSampler3, texcoo);
	texcols[3] = tex2D(textureSampler4, texcoo);

	float4 texcolors = float4(0, 0, 0, 0);
	float gesstaerke = 0;
	gesstaerke += staerken[0];
	gesstaerke += staerken[1];
	gesstaerke += staerken[2];
	gesstaerke += staerken[3];

	staerken[0] = staerken[0] / gesstaerke;
	staerken[1] = staerken[1] / gesstaerke;
	staerken[2] = staerken[2] / gesstaerke;
	staerken[3] = staerken[3] / gesstaerke;
	float wetsand_strength = fade(1 - (clamp(input.worldpos.y * 500, 45, 1000) - 45.0f) / 955.0f);
	if (staerken[0] > 0.01f)
	{
		float3 n;
		float4 tex = (tex2D(sandnormalsamp, texcoo));
		n.x = tex.r - 0.5f;
		n.y = tex.b - 0.5f;
		n.z = -tex.g + 0.5f;
		normal = normalize((n * 2 - float3(0, 1, 0)) * staerken[0] * (1 - wetsand_strength) + normal);
	}
	float4 wetsand_color;
	if (wetsand_strength > 0.01f)
	{
		float ssss = fade((clamp(input.worldpos.y * 50, 25, 200) - 25.0f) / 175.0f) * 0.75f;
		float3 n;
		float4 tex = (tex2D(wetsand_normalsamp, texcoo));
		n.x = (tex.r - 0.5f);
		n.y = tex.b - 0.5f;
		n.z = (-tex.g + 0.5f);
		normal = normalize((n * 2 - float3(0, 1, 0)) * wetsand_strength * (ssss + 0.04f) + normal);
		wetsand_color = tex2D(wetsand_texsamp, texcoo);
		texcolors += wetsand_strength * wetsand_color * staerken[0];
		SpecularIntensity += (tex2D(wetsand_specularsamp, texcoo)) * wetsand_strength * 2.5f * (1 - ssss) * clamp(-LightDirection.y + 0.35f, 0, 1);
		SpecularIntensity *= (1 - clamp((-input.worldpos.y), 0, 1));
	}
	//texcols[0] *= 1 - fade(1 - (clamp(input.worldpos.y, 45, 400) - 45.0f) / 355.0f) * 0.45f;

	texcolors += staerken[0] * texcols[0] * (1 - wetsand_strength);
	texcolors += staerken[1] * texcols[1];
	texcolors += staerken[2] * texcols[2];
	texcolors += staerken[3] * texcols[3];

	//SpecularIntensity += fade(1 - (clamp(input.worldpos.y, 45, 400) - 45.0f) / 355.0f) * 0.5f;
	
	
	float4 diffuse = dot(-LightDirection, normal);
	float3 reflect2 = normalize(reflect(-LightDirection, normal));

	float reflectstrength = pow(saturate(dot(-reflect2, normalize(EyePosition - input.worldpos))*0.96),10);
	//SpecularColor = float4(1, 1, 1, 1);

	// Calculating Light Color and Strength
	float4 lightcolors = ((AmbientColor * AmbientIntensity + DiffuseColor * DiffuseIntensity * clamp(diffuse, 0, 1)) + SpecularColor * SpecularIntensity * pow(clamp(reflectstrength, 0, 1), 1.25f));

	// Combing Light and Texture Colors
	float4 output = saturate((texcolors * lightcolors));

	// Dithering
	float noise = lerp(-0.5, 0.5, Dither(input.worldpos.xyz));
	output.xyz = output + noise / 511.0f;
	output.a = 1.0f;
	OUT.Col1 = output;
	OUT.Col2 = float4(input.worldpos.y, 0, 0, 1);
	OUT.Col3 = output;


	return OUT;
}

technique Rendering
{
	pass Pass_1
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}
