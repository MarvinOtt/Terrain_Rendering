#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

matrix WorldViewProjection;
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 EyePosition;
float3 LightDirection;
Texture2D watertex;
Texture2D watertex2;
Texture2D terrainheights;
int Screenwidth;
int Screenheight;
int worldsize;
float waveheight;
float waveverschiebung, waveverschiebung2;
float viewdeepness;
float wellenbreite, wellenbreite2;
const int texabm = 2048;
float deepestheight;
float4 lightcolor = float4(1, 1, 1, 1);
float3 leftup, leftdown, rightup, rightdown;

bool IsReflections, IsTerraindistortion, IsWaves;

static const float PI = 3.14159265f;
static const float PIhalf = 1.5707963f;

float4 oceanblue = float4(0.04098f, 0.12725f, 0.3049f, 1.0f) * 0.6f; // Color of deep ocean water from above
float4 sunorange = float4(1, 0.6f, 0.1f, 1); // Color of the sun at evening

int wavemeshsizeX;
int wavemeshsizeY;

SamplerState heightmapsamp
{
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

//
// SAMPLER
//
Texture2D normalmap;
sampler2D nmsamp = sampler_state {
	Texture = (normalmap);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

Texture2D maptex;
sampler2D mapsamp = sampler_state {
	Texture = (maptex);
	MAGFILTER = NONE;
	MINFILTER = NONE;
	MIPFILTER = NONE;
	AddressU = Clamp;
	AddressV = Clamp;
};

Texture2D depthtex;
sampler2D depthsamp = sampler_state {
	Texture = (depthtex);
	MAGFILTER = NONE;
	MINFILTER = NONE;
	MIPFILTER = NONE;
	AddressU = Clamp;
	AddressV = Clamp;
};

// The input for the VertexShader
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float4 Color : COLOR;
};

// The output from the vertex shader || input for the pixel shader
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Position2 : POSITION1;
	float3 View : TEXCOORD1;
	float4 Color : COLOR0;
	float terrainheight : BLENDWEIGHT0;
};

float fade(float t)
{
	return (t * t * t * (t * (t * 6 - 15) + 10));         // 6t^5 - 15t^4 + 10t^3
}

// Adds two normals correctly
inline float3 addnormal(float3 n1, float3 n2)
{
	float xcos1 = PIhalf - acos(n1.x);
	float xcos2 = PIhalf - acos(n2.x);
	float newx = sin(xcos1 + xcos2);

	float zcos1 = PIhalf - acos(n1.z);
	float zcos2 = PIhalf - acos(n2.z);
	float newz = sin(zcos1 + zcos2);

	float newy = sqrt((1 - newx*newx) - newz*newz);
	return normalize(float3(newx, newy, newz));
}
inline float3 multiply_normal(float3 normal, float value)
{
	float3 OUT = normal;
	OUT.xz *= value;
	return normalize(OUT);
}

// Returns height at position
float getheightatpos(float xi, float yi)
{
	xi = xi / wellenbreite;
	yi = yi / wellenbreite;
	if (xi < 0)
		xi = texabm - ((-xi) % texabm);
	if (yi < 0)
		yi = texabm - ((-yi) % texabm);
	xi = abs(xi % texabm);
	yi = abs(yi % texabm);

	float y = (yi + waveverschiebung);
	float x = xi;
	float y2 = yi;
	float x2 = (xi + waveverschiebung * 1.2324f);
	float y3 = abs(yi - waveverschiebung * 0.65274f);
	float x3 = abs(xi - waveverschiebung * 0.70328f);

	float height = 0;
	height += watertex.SampleLevel(heightmapsamp, float2(x, y) / (float)texabm, 0).r;
	height += watertex.SampleLevel(heightmapsamp, float2(x2, y2) / (float)texabm, 0).r;
	height += watertex.SampleLevel(heightmapsamp, float2(x3, y3) / (float)texabm, 0).r;

	return (height * 0.33333f) - 0.5f;
}

// Returns normal of big waves
float3 getnormalatpos(float x, float y)
{
	float xi, yi;
	xi = x / wellenbreite;
	yi = y / wellenbreite;
	if (xi < 0)
		xi = texabm - ((-xi) % texabm);
	if (yi < 0)
		yi = texabm - ((-yi) % texabm);
	xi = xi % texabm;
	yi = yi % texabm;

	float nx, ny;

	nx = ((xi) % texabm);
	ny = ((yi + waveverschiebung) % texabm);
	float3 normal1 = tex2D(nmsamp, (float2(nx, ny) / (float)texabm)).xyz;

	nx = ((xi + waveverschiebung * 1.2324f) % texabm);
	ny = ((yi) % texabm);
	float3 normal2 = tex2D(nmsamp, (float2(nx, ny) / (float)texabm)).xyz;

	nx = ((x / wellenbreite - waveverschiebung * 0.70328f) % texabm);
	ny = ((y / wellenbreite - waveverschiebung * 0.65274f) % texabm);
	float3 normal3 = tex2D(nmsamp, (float2(nx, ny) / (float)texabm)).xyz;

	normal1 += normal1 - float3(1, 1, 1);
	normal2 += normal2 - float3(1, 1, 1);
	normal3 += normal3 - float3(1, 1, 1);
	return normalize(addnormal(addnormal(normal1, normal2), normal3));
	//return float3(normal1.x * 2 - 1, normal1.y * 2 - 1, normal1.z * 2 - 1) + float3(normal2.x * 2 - 1, normal2.y * 2 - 1, normal2.z * 2 - 1) + float3(normal3.x * 2 - 1, normal3.y * 2 - 1, normal3.z * 2 - 1);
}

float4 getblueskycolor(float3 direction)
{
	float4 blue = float4(0.6f, 0.77f, 1, 1);
	float airdensity = (1 - direction.y); //Density of air to the pixel of the sky
	float blauint = (-LightDirection.y) + (1 - airdensity) * 0.05f;
	blauint = ((blauint / 1.4f) + 0.4f) * 0.65f;
	return blue * (1 - pow(1 - blauint, 1.5f));
}

//
// VERTEX SHADER
//
VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
	VertexShaderOutput output;

	float xfactor = 1 - (input.Position.x / (wavemeshsizeX - 1));
	float yfactor = 1 - (input.Position.z / (wavemeshsizeY - 1));
	float3 leftdownfactor = xfactor * yfactor * leftdown;
	float3 leftupfactor = xfactor * (1 - yfactor) * leftup;
	float3 rightdownfactor = (1 - xfactor) * yfactor * rightdown;
	float3 rightupfactor = (1 - xfactor) * (1 - yfactor) * rightup;
	float3 direction = normalize(leftdownfactor + leftupfactor + rightdownfactor + rightupfactor);
	float xkoo = -direction.x*(EyePosition.y / direction.y);
	float ykoo = -direction.z*(EyePosition.y / direction.y);
	float3 koo = float3(xkoo + EyePosition.x, 0, ykoo + EyePosition.z);

	/*if (direction.y >= 0 || length(koo) > 50000)
	{
		koo = float3(EyePosition.x, -10000.0f, EyePosition.z);
	}*/
	if ((direction.y >= 0 || length(koo) > 50000.0f) && EyePosition.y > 0.0f)
	{
		float xzlength = 50000.0f;
		koo = float3(direction.x * xzlength + EyePosition.x, 0, direction.z * xzlength + EyePosition.z);
	}
	input.Position = float4(koo, 1);

	// Height Displacement
	input.Position.y += getheightatpos(input.Position.x, input.Position.z) * waveheight;

	float terrainheight = terrainheights.SampleLevel(heightmapsamp, float2(input.Position.x + worldsize * 0.5f, input.Position.z + worldsize * 0.5f) / (float)(worldsize), 0).r;

	float factor = 1;
	if (terrainheight > 0)
		factor = 0.45f;
	else if (terrainheight > -50)
		factor = 0.45f + fade(-terrainheight * 0.02f) * 0.55f;

	input.Position.y *= factor;
	input.Position.y += (1 - factor) * sin((terrainheight - waveverschiebung * 0.04f) * 1.6f) * 0.4f;


	//input.Position.y += terrainheight * 0.5f;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.Position2 = worldPosition;
	output.Color = input.Color;
	output.View = normalize(EyePosition - worldPosition.xyz);
	output.terrainheight = terrainheight;
	return output;
}

//
// PIXEL SHADER
//
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 OUT = float4(0, 0, 0, 0); // Final Output Color
	float normalfactor = 1;
	if (input.terrainheight > 0)
		normalfactor = 0.45f;
	else if (input.terrainheight > -50)
	{
		normalfactor = 0.45f + fade(-input.terrainheight * 0.02f) * 0.55f;
	}

	// Normal of water surface
	float3 normal = getnormalatpos(input.Position2.x, input.Position2.z);
	normal = multiply_normal(normal, normalfactor);
	// Direction from Camera to Surface
	float3 direction = normalize(input.Position2 - EyePosition);
	// Reflected direction from surface
	float3 reflecteddirection = normalize(reflect(direction, normal));

	float distance = length(EyePosition - input.Position2.xyz);


	float fresnel_term = saturate(dot(-direction, normal));
	float oneminusfresnel_term = 1 - fresnel_term;

	//
	// TERRAIN UNDER WATER
	//

	float terrainheight = tex2D(depthsamp, input.Position.xy / float2(Screenwidth, Screenheight)).r;
	if (terrainheight == 0)
		terrainheight = -deepestheight;
	float depth = abs((terrainheight - input.Position2.y) / direction.y);

	// Strengths for colors
	float reflectedsunlightstrength = pow(saturate(dot(-LightDirection, reflecteddirection)) + 0.005f, 7500.0f) * 450.0f;
	float reflectedaveragelightstrength = pow(saturate(dot(-LightDirection, reflecteddirection)) * saturate(dot(direction, reflecteddirection)), 7.5f) * 0.75f;
	float watercolorstrength = fade(saturate((-LightDirection.y + 0.15f) / 1.15f)) + 0.05f;
	float terraincolorstrength = clamp((deepestheight - depth) / ((2.5f + depth) * (deepestheight / 2.5f)), 0, 1);

	if (-LightDirection.y < 0.8f)
	{
		if (-LightDirection.y < -0.02f)
			reflectedsunlightstrength /= 1 + reflectedsunlightstrength * (-(-LightDirection.y + 0.02f) * 0.01f);
		reflectedsunlightstrength *= (1 - (fade(saturate((LightDirection.y + 0.8f) * 1.125f))));
		if (-LightDirection.y < 0.1f)
			reflectedaveragelightstrength *= (1 - (fade(saturate((LightDirection.y + 0.1f) * 1.2f))));
	}

	float4 watercolor = oceanblue * watercolorstrength * (1 - terraincolorstrength);
	float4 skycolor = getblueskycolor(reflecteddirection) * 0.4f;
	float4 reflectedsunlightcolor = (reflectedsunlightstrength + reflectedaveragelightstrength) * sunorange * oneminusfresnel_term;

	float terraindistorbtion = Screenwidth * 0.025f;
	float posx = input.Position.x + (normal.z * (1 + (400 / (400 + distance))) * terraindistorbtion * (clamp(abs(depth) / viewdeepness, 0, 1)));
	float posy = input.Position.y + (normal.x * (1 + (400 / (400 + distance))) * terraindistorbtion * (clamp(abs(depth) / viewdeepness, 0, 1)));
	float4 terraincolor = tex2D(mapsamp, float2(posx, posy) / float2(Screenwidth, Screenheight));

	float4 RefrectiveColor = (watercolor + terraincolor * terraincolorstrength);
	float4 ReflectiveColor = (skycolor + reflectedsunlightcolor) * oneminusfresnel_term * (0.4 + (200 / (200 + distance))*0.6f);

	//ReflectiveColor *= fade(clamp(depth * 4, 0, 0.5f) / 0.5f);
	
	OUT = saturate(ReflectiveColor + RefrectiveColor);
	OUT *= fade(clamp(depth * 2, 0, 0.5f) / 0.5f);
	OUT += (terraincolor) * fade(1 - clamp(depth * 2, 0, 0.5f) / 0.5f);
	OUT.a = 1;
	
	return OUT;// *0.0001f + float4(-reflecteddirection.y, reflecteddirection.y, 0, 1);
}

technique Rendering_Water
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
};