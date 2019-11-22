Shader "Unlit/CenterErosionShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Fill ("Fill", Range (0, 1)) = .5
		_Scale ("Scale", float) = 1
		_Height ("Height", float) = 4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

		Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
				float3 localPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
				
				fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _TintColor;
			
			float _Fill;
			float _Scale;
			float _Height;

            v2f vert (appdata v)
            {
                v2f o;
				o.localPos = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
				o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = fixed4(1,1,1,1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

				//clip((-length(i.localPos) - (_Scale + 0.001f)) + i.color.a * (_Scale * _Height));
				//clip((length(i.localPos) - (_Scale)) + i.color.a * (_Scale * _Height));

				clip((tex2D(_MainTex, i.uv) - (_Scale)) + i.color.a * (_Scale * _Height));

                return col;
            }
            ENDCG
        }
    }
}
