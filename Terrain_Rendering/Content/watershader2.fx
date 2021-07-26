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
float3 EyePosition;
float3 LightDirection;
Texture2D watertex;
Texture2D watertex2;
int Screenwidth;
int Screenheight;
float wellenhohe;
float waveverschiebung, waveverschiebung2;
float viewdeepness;
float wellenbreite, wellenbreite2;
int texabm = 2048;
float deepestheight;
float4 lightcolor = float4(1,1,1,1);
float3 leftup, leftdown, rightup, rightdown;

bool IsReflections, IsTerraindistortion, IsWaves;

static const float PI = 3.14159265f;
static const float PIhalf = 1.5707963f;

int wavemeshsizeX;
int wavemeshsizeY;
// anisotropic
Texture2D normalmap;
sampler2D nmsamp = sampler_state {
	Texture = (normalmap);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

Texture2D normalmap2;
sampler2D nmsamp2 = sampler_state {
	Texture = (normalmap2);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

Texture2D reflectionmap;
sampler2D refsamp = sampler_state {
	Texture = (reflectionmap);
	MAGFILTER = NONE;
	MINFILTER = NONE;
	MIPFILTER = NONE;
	AddressU = Clamp;
	AddressV = Clamp;
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

Texture2D reflectiondepthmap;
sampler2D refdepthsamp = sampler_state {
	Texture = (reflectiondepthmap);
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
};

float fade(float t)
{
	return (t * t * t * (t * (t * 6 - 15) + 10));         // 6t^5 - 15t^4 + 10t^3
}
// Returns the color of the sky
float4 colorofskyfromdirection(float3 direction)
{
	float airdensity = (1 - direction.y); //Density of air to the pixel of the sky
	float blauint = (-LightDirection.y) + (1 - airdensity) * 0.05f;
	float sundot = (dot(-LightDirection, direction) + 1) / 2.0f;

	blauint = ((blauint / 1.4f) + 0.4f)*0.5f;

	// Makes the sky dark when the sun goes down
	float verdunkelung = ((-LightDirection.y) + 1) / 2.0f;

	float sonne = pow(sundot + 0.00075f, 100000) * 1000;

	float luftpow = (1 / pow(airdensity, 8));
	float orangeint = pow(sundot * (1 + 0.002f * pow(airdensity, 2)), luftpow) * verdunkelung * 1.5f;// +pow(sundot + 0.0005f, 500) * 0.25f;

	float4 orange = float4(1, 0.6f, 0.05f, 1); // Color of the sun
	float4 blue = float4(0.25f, 0.52f, 1, 1); // Color of the sky at day

	float orangefactor2 = 1;
	if (direction.y < 0.8f)
	{
		orangefactor2 = saturate(1 - fade(-(direction.y - 0.8f)) * 0.85f);
	}

	float4 output = /*orange * orangeint * 1.0f * orangefactor2*/ +blue * (1 - pow(1 - blauint, 2)); // Combining the blue and orange Color
	return output * 0.75f;
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
	float y3 = abs(yi - waveverschiebung/1.532f);
	float x3 = abs(xi - waveverschiebung/1.4219f);
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
float3 getnormalatpostex(float x, float y)
{
	float nx, ny;
	if (x / wellenbreite > 0)
		nx = ((x / wellenbreite) % texabm);
	else
		nx = (texabm - (abs(x / wellenbreite) % texabm));


	if (y / wellenbreite + waveverschiebung > 0)
		ny = ((y / wellenbreite + waveverschiebung) % texabm);
	else
		ny = (texabm - (abs(y / wellenbreite + waveverschiebung) % texabm));

	float4 normal1 = tex2D(nmsamp, (float2(nx, ny) / (float)texabm));





	if (x / wellenbreite + waveverschiebung*1.2324f > 0)
		nx = ((x / wellenbreite + waveverschiebung*1.2324f) % texabm);
	else
		nx = (texabm - (abs(x / wellenbreite + waveverschiebung*1.2324f) % texabm));


	if (y / wellenbreite > 0)
		ny = ((y / wellenbreite) % texabm);
	else
		ny = (texabm - (abs(y / wellenbreite) % texabm));

	float4 normal2 = tex2D(nmsamp, (float2(nx, ny) / (float)texabm));






	if (x / wellenbreite - waveverschiebung / 1.4219f > 0)
		nx = ((x / wellenbreite - waveverschiebung / 1.4219f) % texabm);
	else
		nx = (texabm - (abs(x / wellenbreite - waveverschiebung / 1.4219f) % texabm));


	if (y / wellenbreite - waveverschiebung / 1.532f > 0)
		ny = ((y / wellenbreite - waveverschiebung / 1.532f) % texabm);
	else
		ny = (texabm - (abs(y / wellenbreite - waveverschiebung / 1.532f) % texabm));

	float4 normal3 = tex2D(nmsamp, (float2(nx, ny) / (float)texabm));

	//uint2 cooo = { nx * 800, ny * 800 };
	//float3 normal1 = normalize(normalmap[cooo]);
	return float3(normal1.x * 2 - 1, normal1.y * 2 - 1, normal1.z * 2 - 1) + float3(normal2.x * 2 - 1, normal2.y * 2 - 1, normal2.z * 2 - 1) + float3(normal3.x * 2 - 1, normal3.y * 2 - 1, normal3.z * 2 - 1);
}

float3 getnormalatpostex_fine(float x, float y)
{
	float nx, ny;
	if (x / wellenbreite2 > 0)
		nx = ((x / wellenbreite2) % texabm);
	else
		nx = (texabm - (abs(x / wellenbreite2) % texabm));


	if (y / wellenbreite2 + waveverschiebung2 > 0)
		ny = ((y / wellenbreite2 + waveverschiebung2) % texabm);
	else
		ny = (texabm - (abs(y / wellenbreite2 + waveverschiebung2) % texabm));

	float4 normal1 = tex2D(nmsamp2, (float2(nx, ny) / (float)texabm));





	if (x / wellenbreite2 + waveverschiebung2*1.2324f > 0)
		nx = ((x / wellenbreite2 + waveverschiebung2*1.2324f) % texabm);
	else
		nx = (texabm - (abs(x / wellenbreite2 + waveverschiebung2*1.2324f) % texabm));


	if (y / wellenbreite2 > 0)
		ny = ((y / wellenbreite2) % texabm);
	else
		ny = (texabm - (abs(y / wellenbreite2) % texabm));

	float4 normal2 = tex2D(nmsamp2, (float2(nx, ny) / (float)texabm));






	if (x / wellenbreite2 - waveverschiebung2 / 1.4219f > 0)
		nx = ((x / wellenbreite2 - waveverschiebung2 / 1.4219f) % texabm);
	else
		nx = (texabm - (abs(x / wellenbreite2 - waveverschiebung2 / 1.4219f) % texabm));


	if (y / wellenbreite2 - waveverschiebung2 / 1.532f > 0)
		ny = ((y / wellenbreite2 - waveverschiebung2 / 1.532f) % texabm);
	else
		ny = (texabm - (abs(y / wellenbreite2 - waveverschiebung2 / 1.532f) % texabm));

	float4 normal3 = tex2D(nmsamp2, (float2(nx, ny) / (float)texabm));

	//uint2 cooo = { nx * 800, ny * 800 };
	//float3 normal1 = normalize(normalmap[cooo]);
	return float3(normal1.x * 2 - 1, normal1.y * 2 - 1, normal1.z * 2 - 1) + float3(normal2.x * 2 - 1, normal2.y * 2 - 1, normal2.z * 2 - 1) + float3(normal3.x * 2 - 1, normal3.y * 2 - 1, normal3.z * 2 - 1);
}

float3 finishednormalatpos(float x, float y, float finestrength, float normalstrength)
{
	float3 OUT;
	//OUT = normalize(getnormalatpostex(x, y) + getnormalatpostex_fine(x, y));
	float3 n1, n2;
	float3 nfine = getnormalatpostex_fine(x, y);
	float3 nnormal = getnormalatpostex(x, y);
	n1 = normalize(float3(nnormal.x * normalstrength, nnormal.y, nnormal.z * normalstrength));
	n2 = normalize(float3(nfine.x * finestrength, nfine.y, nfine.z * finestrength));

	float xcos1 = PIhalf - acos(n1.x);
	float xcos2 = PIhalf - acos(n2.x);
	float newx = sin(xcos1 + xcos2);

	float zcos1 = PIhalf - acos(n1.z);
	float zcos2 = PIhalf - acos(n2.z);
	float newz = sin(zcos1 + zcos2);

	float newy = sqrt((1 - newx*newx) - newz*newz);
	OUT = float3(newx, newy, newz);
	return OUT;
}

// The VertexShader.
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
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
	if ((direction.y >= 0 || length(koo) > 50000.0f) && EyePosition.y > 0)
	{
		float xzlength = 50000.0f;
		koo = float3(direction.x * xzlength, 0, direction.z * xzlength);
	}
	else if (direction.y < 0 && EyePosition.y < 0)
	{
		float xzlength = 50000.0f;
		koo = float3(direction.x * xzlength, 1000, direction.z * xzlength);
	}

	input.Position = float4(koo, 1);
	float dist = length(EyePosition - mul(input.Position, World).xyz);
	if (IsWaves && dist < 800)
	{
		float heightdif = getheightatpos(mul(input.Position, World).x, mul(input.Position, World).z) * wellenhohe;
		if(dist > 500)
			input.Position.y += heightdif * ((800 - dist) / 300.0f);
		else
			input.Position.y += heightdif;
	}

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.Position2 = worldPosition;
	output.Color = input.Color;
	output.View = normalize(EyePosition - worldPosition.xyz);
	return output;
}


struct PixelOut
{
	float4 Col1 : COLOR0;
	float4 Col2 : COLOR1;
};

// The Pixel Shader
PixelOut PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	PixelOut OUT;
	
	float Camera2worldposlength = length(EyePosition - input.Position2.xyz);
	
	float lightstaerke = clamp((1 - pow(1 + LightDirection.y, 2)), 0, 1);
	
	//float3 normalcol1 = getnormalatpostex(input.Position2.x, input.Position2.z);
	float refdistorbtion = Screenwidth / 15.0f;
	float terraindistorbtion = Screenwidth / 10.0f;
	if (Camera2worldposlength > 20)
	{
		refdistorbtion *= fade(1 / ((Camera2worldposlength - 20) / 2000.0f + 1));
	}

	// T E R R A I N    U N D E R    W A T E R
	float normalheight = tex2D(depthsamp, float2(input.Position.x / ((float)Screenwidth), input.Position.y / ((float)Screenheight))).r; // Height of terrain
	if (normalheight == 0)
		normalheight = -deepestheight;

	float3 wavenormal = finishednormalatpos(input.Position2.x, input.Position2.z, 1.0f, 0.5f);
	float3 normal2 = finishednormalatpos(input.Position2.x, input.Position2.z, 1, 1);
	float3 reflect2 = normalize(reflect(LightDirection, normal2));
	float dotproduct = pow(saturate(dot(reflect2, input.View)), 100);
	float fresnelterm = clamp(saturate(dot(normalize(EyePosition - input.Position2), normal2)), 0, 1);

	float3 dir = normalize(input.Position2 - EyePosition);
	float depth;
	float4 terraincol = float4(0, 0, 0, 0);
	if (IsTerraindistortion)
	{
		float posx = input.Position.x + (wavenormal.z * (1 + (400 / (400 + Camera2worldposlength))) * terraindistorbtion * (clamp(abs(normalheight) / viewdeepness, 0, 1)));
		float posy = input.Position.y + (wavenormal.x * (1 + (400 / (400 + Camera2worldposlength))) * terraindistorbtion * (clamp(abs(normalheight) / viewdeepness, 0, 1)));
		float height = tex2D(depthsamp, float2(posx / ((float)Screenwidth), posy / ((float)Screenheight))).r; // Height of terrain
		depth = abs((height - input.Position2.y) / dir.y);
		terraincol = tex2D(mapsamp, float2(posx / ((float)Screenwidth), posy / ((float)Screenheight))); // Terrain Color
		if (height == 0)
			depth = deepestheight;
	}
	else
	{
		depth = abs((normalheight - input.Position2.y) / dir.y);
		terraincol = tex2D(mapsamp, float2(input.Position.x / ((float)Screenwidth), input.Position.y / ((float)Screenheight))); // Terrain Color
		if (normalheight == 0)
			depth = deepestheight;
	}

	// R E F L E C T I O N S
	float4 final_reflections = float4(0, 0, 0, 0);
	float refstrength = 4.0f;
	if (IsReflections)
	{
		float refheight = tex2D(refdepthsamp, float2(input.Position.x / ((float)Screenwidth), (Screenheight - input.Position.y) / ((float)Screenheight))).r;
		float x = input.Position.x + (normal2.z*refdistorbtion * clamp(abs(refheight / 100.0f), 0, 1));
		float y = input.Position.y + (normal2.x*refdistorbtion * clamp(abs(refheight / 100.0f), 0, 1));
		float4 refcol = saturate(tex2D(refsamp, float2(x / ((float)Screenwidth), (Screenheight - y) / ((float)Screenheight)))); // Reflection Color
		final_reflections = refcol * pow(1 - fresnelterm, 4) * 0.25f * clamp(depth / (viewdeepness), 0, 1);
		//if (refcol.b < 0.2f)	// No reflections if someting is in the way
			//refstrength = 0;
	}
	fresnelterm = clamp(fresnelterm, 0, 1);
	float foamstrength = clamp(1.5f / (1 + abs(normalheight - input.Position2.y) / 4.0f) - 1, 0, 1);
	float factor = 1 - (1 / (1 + depth * 0.3));
	float terraincolor_deepnessstrength = clamp((deepestheight - depth) / ((1 + depth) * deepestheight), 0, 1);

	float4 watercolor = (fresnelterm / 1.25f + 0.2f) * input.Color * (clamp(depth / viewdeepness, 0, 1)) * pow(lightstaerke, 1.3f) * 0.5f; // Color of water
	float4 FOAM = float4(1, 1, 1, 1) * foamstrength * (0.025 + clamp(-LightDirection.y + 0.4f, 0, 1) / 2.5f);
	float4 terraincolor = (fresnelterm / 4.0f + 0.75f) * (terraincol * (1 - pow(1 - terraincolor_deepnessstrength, 2))); // Terrain color under water
	float4 final_lightcolors = pow(dotproduct, 1.0f) * lightcolor * (clamp(-LightDirection.y + 0.35f, 0, 1) / 1.0f) * refstrength; // Water light reflections

	//  Combining reflections, terrain, light and watercolor
	//float4 finalcolor = terraincol;
	float4 finalcolor = ((watercolor + terraincolor + final_reflections) * (1 - clamp(dotproduct, 0, 1)) + final_lightcolors) * fade(clamp(depth * 4, 0, 0.5f) / 0.5f);
	OUT.Col1 = finalcolor + terraincol * fade(1 - clamp(depth * 4, 0, 0.5f) / 0.5f);

	// Bloom Effect
	float brightness = final_lightcolors.r + final_lightcolors.g + final_lightcolors.b;
	brightness = pow(clamp(brightness / 1.0f, 0, 1), 2) * 1.0f * (1 / (1 + Camera2worldposlength / 1000.0f));
	brightness += 0.0001f;
	OUT.Col2 = finalcolor * brightness;
	OUT.Col1.a = 1.0f;
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
