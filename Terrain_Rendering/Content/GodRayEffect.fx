#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

matrix WorldViewProjection;

float3 EyePosition;
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 LightDirection;
float3 transformedLightDirection;
float3 projsunpos;
int Screenwidth, Screenheight;
float Aspectratio;

matrix invviewmatrix;

float2 tan = float2(10.08228f, 5.671284f);

Texture2D SpriteTexture, obstaclemap, colormap;
float4 orange = float4(1, 0.6f, 0.05f, 1);
float4 blue = float4(0.25f, 0.52f, 1, 1);

// The output from the vertex shader || input for the pixel shader
sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};
sampler2D obstaclesamp = sampler_state
{
	Texture = <obstaclemap>;
};
sampler2D colorsamp = sampler_state
{
	Texture = <colormap>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float fade(float t)
{
	return (t * t * t * (t * (t * 6 - 15) + 10));         // 6t^5 - 15t^4 + 10t^3
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR
{
	float4 OUT;
	
	// CONSTANTS
	float decay = 0.945715;
	float exposure = 0.92;
	float density = 1.29756;
	float weight = 0.90767;

	int NUM_SAMPLES = 100;

	float2 screen = float2(input.Position.x / (float)Screenwidth, input.Position.y / (float)Screenheight) * 2 - float2(1, 1);
	//float2 screennorm = (((input.Position.xy / 1080.0f) * 5.67f) *2) - float2(1, 1);
	float3 projecteddirection = normalize(float3(0.839f * screen.x * Aspectratio, 0.839f * screen.y, 1.0f));
	float3 trans = float3(-projecteddirection.x, projecteddirection.y, projecteddirection.z);
	float3 direction = mul(trans, invviewmatrix);
	direction *= -1;




	float2 sunpos = float2(projsunpos.x / (float)Screenwidth, projsunpos.y / (float)Screenheight);

	float sundist = length(float2(0.5f, 0.5f) - sunpos);
	float3 currentdir = projecteddirection;
	float factor = 0;
	float illuminationDecay = 1.0f;
	
	float4 color = float4(0, 0, 0, 0);
	float4 originalcolor = color;
	float4 sample2;

	float verdunkelung = ((-LightDirection.y) + 1) / 2.0f;
	float airdensity = (1 - direction.y); //Density of air to the pixel of the sky
	float luftpow = (1 / pow(pow(airdensity, 2), 4));
	float blauint = (-LightDirection.y) + (1 - airdensity) * 0.05f;
	blauint = ((blauint / 1.4f) + 0.4f) * 0.5f;

	for (int i = 0; i < NUM_SAMPLES; i++)
	{
		factor += (1 / (float)NUM_SAMPLES) * density;
		currentdir = normalize(transformedLightDirection * factor + projecteddirection * (1 - factor));
		float2 newscreenpos = float2(currentdir.x / (currentdir.z * 0.839 * Aspectratio), currentdir.y / (currentdir.z * 0.839));

		float4 obstacletex = tex2D(obstaclesamp, (newscreenpos + float2(1, 1)) * 0.5f);
		if (obstacletex.r > 0.5f)
			sample2 = float4(0, 0, 0, 1);
		else
		{
			float sundot = (dot(transformedLightDirection, currentdir) + 1) / 2.0f;
			float sonne = pow(sundot + 0.00020f, 3200);
			float orangeint = pow(sundot * (1 + 0.002f * pow(airdensity, 2)), luftpow) * verdunkelung * 1.5f * 0.04f;
			float clamporangeint = clamp(orangeint, 0, 0.75f);
			sample2 = (float4(clamp(sonne, 0, 0.5f), clamp(sonne, 0, 0.5f), clamp(sonne, 0, 0.5f), 0) + (orange * clamporangeint + blue * clamp(blauint * clamporangeint, 0, 0.75f)) * (pow(1 - abs(LightDirection.y), 2.5f))) * 0.4f;
		}
		
		sample2 *= illuminationDecay * weight;
		color += sample2;
		illuminationDecay *= decay;
	}
	if (color.x + color.y + color.z < originalcolor.x + originalcolor.y + originalcolor.z)
		color = originalcolor;
	float4 realColor = tex2D(colorsamp, input.TextureCoordinates.xy);

	OUT = float4(float3(color.r, color.g, color.b) * exposure, 1) + realColor;// *0.00001f + float4((newscreenpos2.x + 1) * 0.5f - 0.9f, 0, 0, 1);
	float dot2 = dot(transformedLightDirection, projecteddirection);
	if (dot2 < 0.0f)
	{
		OUT = realColor;
	}
	else if (dot2 < 0.625f)
	{
		OUT = (realColor + float4(float3(color.r, color.g, color.b) * exposure, 1) * fade(dot2 * 1.6f));
	}

	OUT.a = 1;

	return OUT;

	/*float2 sunpos = float2(projsunpos.x / 1920.0f, projsunpos.y / 1080.0f);

	float sundist = length(float2(0.5f, 0.5f) - sunpos);

	float2 tc = input.TextureCoordinates;
	float2 deltaTexCoord = (tc - sunpos);
	deltaTexCoord *= (1.0f / (float)NUM_SAMPLES) * density;
	float illuminationDecay = 1.0f;

	float4 color = tex2D(obstaclesamp, tc) * 0.4f;
	float4 originalcolor = color;

	for (int i = 0; i < NUM_SAMPLES; i++)
	{
		tc -= deltaTexCoord;
		float4 sample2 = tex2D(obstaclesamp, tc) * 0.4f;
		sample2 *= illuminationDecay * weight;
		color += sample2;
		illuminationDecay *= decay;
	}
	if (color.x + color.y + color.z < originalcolor.x + originalcolor.y + originalcolor.z)
		color = originalcolor;
	float4 realColor = tex2D(colorsamp, input.TextureCoordinates.xy);

	if (sundist > 0.55f)
	{
		color = color * fade(clamp(1 - (sundist - 0.55f) * 2.0f, 0, 1));
	}
	if (projsunpos.z < 1)
		color *= 0;

	return ((float4((float3(color.r, color.g, color.b) * exposure), 1)) + (realColor*(1.0f)));*/


}

technique BasicColorDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
};