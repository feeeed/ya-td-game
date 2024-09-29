Shader "Hidden/GaussBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };

    v2f vert (appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
    }

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    half2 _BlurDirection;

    half4 frag (v2f i) : SV_Target
    {
        float texelSize = _MainTex_TexelSize.y;
        float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);

        half4 c0 = tex2D(_MainTex, uv - _BlurDirection * texelSize * 3.23076923 );
        half4 c1 = tex2D(_MainTex, uv - _BlurDirection * texelSize * 1.38461538 );
        half4 c2 = tex2D(_MainTex, uv);
        half4 c3 = tex2D(_MainTex, uv + _BlurDirection * texelSize * 1.38461538 );
        half4 c4 = tex2D(_MainTex, uv + _BlurDirection * texelSize * 3.23076923 );

        //if (c0.a <= 0.5) c0.rgb = c2.rgb;
        //if (c1.a <= 0.5) c1.rgb = c2.rgb;
        //if (c3.a <= 0.5) c3.rgb = c2.rgb;
        //if (c4.a <= 0.5) c4.rgb = c2.rgb;

        //return (c0 + c1 + c2 + c3 + c4) * 0.20005; 
 
        
        return (c0 * 0.07027027 + c1 * 0.31621622
             + c2 * 0.22702703
             + c3 * 0.31621622 + c4 * 0.07027027) * 1.00005;
    }
    ENDCG

    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Tags { "RenderTexture"="True" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
