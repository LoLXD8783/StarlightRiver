sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float power;
float speed;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float2 offset;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
	float2 off = float2(0, sin(uTime + (coords.x + offset.x) * speed) * cos((coords.x + offset.x) * 0.37 * speed - uTime) * sin((coords.x + offset.x) * 0.21 * speed - uTime) * power);
	float4 color = tex2D(uImage0, coords + off);
	float map = tex2D(samplerTex, coords + off).r;
	float map2 = map * map * map;
	float bright = min((color.r + color.g + color.b) * 2.5, 0.005);

	return float4(color.xyz * map * 3.0, color.a * map);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}