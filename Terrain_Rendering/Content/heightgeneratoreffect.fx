#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

texture2D SpriteTexture;
float4x4 Matrix;
float2 rendercoos;

float p[255];

float3 grad3[12];
float2 texsize;
int worldsize;
float pointvalue00;
float pointvalue10;
float pointvalue01;
float pointvalue11;
sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

float fade(float t)
{
	// Fade function as defined by Ken Perlin.  This eases coordinate values
	// so that they will "ease" towards integral values.  This ends up smoothing
	// the final output.
	return (t * t * t * (t * (t * 6 - 15) + 10));         // 6t^5 - 15t^4 + 10t^3
}
float fade2(float t)
{
	// Fade function as defined by Ken Perlin.  This eases coordinate values
	// so that they will "ease" towards integral values.  This ends up smoothing
	// the final output.
	return (t * t * (t * (t * 6 - 15) + 10));         // 6t^5 - 15t^4 + 10t^3
}

float dot3(float3 g, float x, float y) {
	return g.x * x + g.y * y;
}

int fastfloor(float x) {
	return x>0 ? (int)x : (int)x - 1;
}

float simplexnoise(float xin, float yin) {
	float n0, n1, n2; // Noise contributions from the three corners
					   // Skew the input space to determine which simplex cell we're in
	float F2 = 0.3660254f;//0.5f*(sqrt(3.0f) - 1.0f)
	float s = (xin + yin)*F2; // Hairy factor for 2D
	int i = fastfloor(xin + s);
	int j = fastfloor(yin + s);
	float G2 = 0.211324f; // (3.0f - sqrt(3.0f)) / 6.0f
	float t = (i + j)*G2;
	float X0 = i - t; // Unskew the cell origin back to (x,y) space
	float Y0 = j - t;
	float x0 = xin - X0; // The x,y distances from the cell origin
	float y0 = yin - Y0;
	// For the 2D case, the simplex shape is an equilateral triangle.
	// Determine which simplex we are in.
	int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
	if (x0>y0) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
	else { i1 = 0; j1 = 1; } // upper triangle, YX order: (0,0)->(0,1)->(1,1)
							 // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
							 // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
							 // c = (3-sqrt(3))/6
	float x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
	float y1 = y0 - j1 + G2;
	float G2mul = 0.4226497f;
	float x2 = x0 - 1.0f + G2mul; // Offsets for last corner in (x,y) unskewed coords
	float y2 = y0 - 1.0f + G2mul;
	// Work out the hashed gradient indices of the three simplex corners
	int ii = i % 255;
	int jj = j % 255;
	int gi0 = p[(ii + p[jj]) % 255] % 12;
	int gi1 = p[(ii + i1 + p[(jj + j1)%255])%255] % 12;
	int gi2 = p[(ii + 1 + p[(jj + 1)%255])%255] % 12;
	// Calculate the contribution from the three corners
	float t0 = 0.5f - x0*x0 - y0*y0;
	if (t0<0.0f) n0 = 0.0f;
	else {
		t0 *= t0;
		n0 = t0 * t0 * dot3(grad3[gi0], x0, y0); // (x,y) of grad3 used for 2D gradient
	}
	float t1 = 0.5f - x1*x1 - y1*y1;
	if (t1<0.0f) n1 = 0.0f;
	else {
		t1 *= t1;
		n1 = t1 * t1 * dot3(grad3[gi1], x1, y1);
	}
	float t2 = 0.5f - x2*x2 - y2*y2;
	if (t2<0.0f) n2 = 0.0f;
	else {
		t2 *= t2;
		n2 = t2 * t2 * dot3(grad3[gi2], x2, y2);
	}
	// Add contributions from each corner to get the final noise value.
	// The result is scaled to return values in the interval [-1,1].
	float OUT = 70.0f * (n0 + n1 + n2);
	//OUT = 1.0f - abs(OUT);
	//OUT = OUT * 2.0f - 1.0f;
	return OUT;
}

float speedperlin2D(float x, float y)
{

	uint xcoo, ycoo;
	xcoo = (int)x;
	ycoo = (int)y;
	pointvalue00 = p[((p[(((xcoo) * 734) % 256)] * 0.5f + p[(((ycoo) * 346) % 256)] * 0.5f))] * 0.00390625f;
	pointvalue01 = p[((p[(((xcoo) * 734) % 256)] * 0.5f + p[(((ycoo + 1) * 346) % 256)] * 0.5f))] * 0.00390625f;
	pointvalue10 = p[((p[(((xcoo + 1) * 734) % 256)] * 0.5f + p[(((ycoo) * 346) % 256)] * 0.5f))] * 0.00390625f;
	pointvalue11 = p[((p[(((xcoo + 1) * 734) % 256)] * 0.5f + p[(((ycoo + 1) * 346) % 256)] * 0.5f))] * 0.00390625f;
	float hohe = 0;
	hohe += fade(1 - (x - xcoo)) * pointvalue00 * fade(1 - (y - ycoo));
	hohe += fade(x - xcoo) * pointvalue10 * fade(1 - (y - ycoo));
	hohe += fade(1 - (x - xcoo)) * pointvalue01 * fade(y - ycoo);
	hohe += fade(x - xcoo) * pointvalue11 * fade(y - ycoo);
	return (hohe - 0.5f) * 2.0f;
}

float Octavesimplexnoise(float x, float y, int octaves, float persistence)
{
	persistence *= 1;
	float total = 0.0f;
	float frequency = 0.5f;
	float amplitude = 1.0f;
	float maxValue = 0.0f;
	for (int i = 0; i < octaves; i++)
	{
		float OUT = simplexnoise(x * frequency + 100.0f, y * frequency + 100.0f);
		total += OUT * amplitude;
		maxValue += amplitude;
		amplitude *= persistence;
		frequency *= 2.0f;
	}
	return total / maxValue;
}

float Octavesimplexnoise2(float x, float y, int octaves, float persistence)
{
	persistence *= 1;
	float total = 0.0f;
	float frequency = 0.5f;
	float amplitude = 1.0f;
	float maxValue = 0.0f;
	for (int i = 0; i < octaves; i++)
	{
		float OUT = simplexnoise(x * frequency + 100.0f, y * frequency + 100.0f);
		if (i > 0)
		{
			OUT *= 0.1f + fade(pow(1 - fade(abs(total)), 3.0f)) * 0.9f;// *(1 - abs(total)) * (1 - abs(total));
		}
		total += OUT * amplitude;
		maxValue += amplitude;
		amplitude *= persistence;
		frequency *= 2.4f;
	}
	return total / maxValue;
}


/*float rigidnoise(float x, float y)
{
	uint xcoo, ycoo;
	xcoo = (int)x;
	ycoo = (int)y;
	pointvalue00 = p[((p[(((xcoo) * 734) % 256)] * 0.5f + p[(((ycoo) * 346) % 256)] * 0.5f))] * 0.00390625f;
	pointvalue01 = p[((p[(((xcoo) * 734) % 256)] * 0.5f + p[(((ycoo + 1) * 346) % 256)] * 0.5f))] * 0.00390625f;
	pointvalue10 = p[((p[(((xcoo + 1) * 734) % 256)] * 0.5f + p[(((ycoo) * 346) % 256)] * 0.5f))] * 0.00390625f;
	pointvalue11 = p[((p[(((xcoo + 1) * 734) % 256)] * 0.5f + p[(((ycoo + 1) * 346) % 256)] * 0.5f))] * 0.00390625f;
	float hohe = 0;
	hohe += fade(1 - (x - xcoo)) * pointvalue00 * fade(1 - (y - ycoo));
	hohe += fade(x - xcoo) * pointvalue10 * fade(1 - (y - ycoo));
	hohe += fade(1 - (x - xcoo)) * pointvalue01 * fade(y - ycoo);
	hohe += fade(x - xcoo) * pointvalue11 * fade(y - ycoo);
	float OUT = (hohe - 0.5f);
	OUT = 0.5 - abs(OUT);
	return (OUT * 2.0f);
}*/


float Octaverigidnoise(float x, float y, int octaves, float persistence)
{
	persistence *= 1;
	float total = 0;
	float frequency = 0.5;
	float amplitude = 1;
	float maxValue = 0;
	for (int i = 0; i < octaves; i++)
	{
		float OUT = Octavesimplexnoise2(x * frequency + p[octaves + i], y * frequency + p[octaves + i + 1], 10, 0.385f);
		OUT = 1.0f - abs(OUT);
		OUT = OUT * 2.0f - 1.0f;
		OUT += 0.02f;
		if (OUT > 1.0f)
		{
			OUT = 1 + 0.01f * pow(fade((OUT - 1.0f) * 50.0f), 2.0f) * 1.5f + (OUT - 1.0f) * (1 - fade((OUT - 1.0f) * 50.0f));
		}
		if (i > 0)
		{
			OUT *= abs(total);
		}
		total += OUT * amplitude;
		maxValue += amplitude;
		amplitude *= persistence;
		frequency *= 2.0f;
	}
	return total / maxValue;
}

float speedOctavePerlin2D(float x, float y, int octaves, float persistence)
{
	persistence *= 1;
	float total = 0;
	float frequency = 0.5;
	float amplitude = 1;
	float maxValue = 0;
	for (int i = 0; i < octaves; i++)
	{
		float OUT = speedperlin2D(x * frequency + 100.0f, y * frequency + 100.0f) * amplitude;
		total += OUT;
		maxValue += amplitude;
		amplitude *= persistence;
		frequency *= 1.87578f;
	}
	return total / maxValue;
}
struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input, float2 coords: TEXCOORD0) : COLOR0
{
	float height = 0;
	float xcoo = coords.x*texsize.x + rendercoos.x;
	float ycoo = coords.y*texsize.y + rendercoos.y;

	float biomeheight, biomeheight2;
	float mountainheight, islandheight, flatlandheight;
	float finalheight;

	biomeheight = Octavesimplexnoise(xcoo * 0.00015f, ycoo * 0.00015f, 5, 0.45f);
	biomeheight2 = Octavesimplexnoise(xcoo * 0.00025f + 100.0f, ycoo * 0.00025f + 100.0f, 5, 0.45f);

	flatlandheight = Octavesimplexnoise(xcoo * 0.0002f, ycoo * 0.0002f, 10, 0.425f) * 0.05f;

	// Generating Heights for Islands
	islandheight = speedOctavePerlin2D(xcoo * 0.0075f, ycoo * 0.0075f, 12, 0.4f) * 0.03f - 0.01f;
	//height += Octaverigidnoise(xcoo * 0.001f, ycoo * 0.001f, 10, 0.5f) * 0.03f * 10.0f;
	//height = ((speedOctavePerlin2D(xcoo * 0.0025f, ycoo * 0.0025f, 12, 0.375f) + 1.0f) * 0.275f + 0.005f);// *(0.9375f + height / 0.12f);

	mountainheight = Octaverigidnoise(xcoo * 0.0004f, ycoo * 0.0004f, 1, 1.0f) * 0.28f + 0.025f;
	mountainheight += Octaverigidnoise(xcoo * 0.00090f + 100.0f, ycoo * 0.00090f + 100.0f, 2, 0.325f) * 0.065f;
	mountainheight += Octaverigidnoise(xcoo * 0.0005f - 100.0f, ycoo * 0.0005f - 100.0f, 2, 0.4f) * 0.25f * mountainheight;

	if (biomeheight2 < 0.25f && biomeheight2 > -0.25f)
	{
		float strength = fade((biomeheight2 + 0.25f) * 2.0f);
		islandheight = strength * flatlandheight + (1 - strength) * islandheight;
	}
	else if(biomeheight2 > 0.25f)
		islandheight = flatlandheight;



	if (biomeheight < -0.2f)
		finalheight = islandheight;
	else if (biomeheight < 0.2f)
	{
		float strength = fade((biomeheight + 0.2f) * 2.5f);
		finalheight = strength * mountainheight + (1 - strength) * islandheight;
	}
	else
		finalheight = mountainheight;

	//height += Octaverigidnoise(xcoo * 0.0022f + 10.0f, ycoo * 0.0022f + 10.0f, 8, 0.425f) * 0.31f * 0.025f;
	//height += speedOctavePerlin2D(xcoo * 0.0010f, ycoo * 0.001f, 10, 0.425f) * 0.4f + 0.1f;

	return float4(finalheight, 0, 0.5, 1);
	

	/*
	height += Octaverigidnoise(xcoo * 0.00010f, ycoo * 0.00010f, 1, 1.0f) * 0.20f;
	//height += Octaverigidnoise(xcoo * 0.00090f, ycoo * 0.00090f, 10, 0.325f) * 0.075f;
	//height += Octaverigidnoise(xcoo * 0.0005f, ycoo * 0.0005f, 2, 0.5f) * 0.5f * height;
	
	
	*/
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};