

float4x4 _FakeLightMatrix;
sampler2D _FakeLightColor;

void SampleFakeLightning_half(float3 worldPos, out half3 emission, out half occlusion)
{
    float4 fakeLightCoord = mul(_FakeLightMatrix, float4(worldPos, 1));
    fakeLightCoord.xyz /= fakeLightCoord.w;

    half4 fakeLightColor = tex2D(_FakeLightColor, fakeLightCoord.xy);
    //half fakeLightColorMax = max(fakeLightColor.r, max(fakeLightColor.g, fakeLightColor.b));

    occlusion = saturate(smoothstep(0, 0.5, fakeLightColor * 0.5)) * 2;
    //emission = saturate(fakeLightColor - 0.5) * 2;
    emission = saturate(smoothstep(0.5, 1, fakeLightColor)) * 2;
}