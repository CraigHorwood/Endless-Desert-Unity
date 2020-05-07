Shader "Custom/ChunkShader" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_RoughnessMap ("Roughness Map", 2D) = "black" {}
		_HeightMap ("Height Map", 2D) = "black" {}
		_HeightNormalMap ("Height Normal Map", 2D) = "bump" {}
		_HoleFlags ("Hole Flags", 2D) = "black" {}
		_HoleTexture ("Hole Texture", 2D) = "white" {}
		_SelectedCell ("Selected Cell", Int) = -1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM

		#pragma surface surf Lambert fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _HoleFlags;
		sampler2D _RoughnessMap;
		sampler2D _HeightMap;
		sampler2D _HeightNormalMap;
		sampler2D _HoleTexture;
		int _SelectedCell;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_HoleFlags;
			float2 uv_HoleTexture;
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			v.vertex.y += tex2Dlod(_RoughnessMap, fixed4(v.texcoord.xy, 0.0, 0.0)).r * 0.125;
			v.vertex.y += tex2Dlod(_HeightMap, fixed4(v.texcoord.xy, 0.0, 0.0)).r * 2.0 - 1.0;
			v.normal.xyz = tex2Dlod(_HeightNormalMap, fixed4(v.texcoord.xy, 0.0, 0.0)).rgb;
		}

		// properties

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			bool isSelectedCell = false;
			if (_SelectedCell >= 0)
			{
				int xSelectedCell = _SelectedCell & 7;
				int zSelectedCell = _SelectedCell >> 3;
				int xCell = floor(IN.uv_MainTex.x * 8.0);
				int zCell = floor(IN.uv_MainTex.y * 8.0);
				isSelectedCell = xSelectedCell == xCell && zSelectedCell == zCell;
			}
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			if (isSelectedCell)
			{
				float2 pix = floor(frac(IN.uv_MainTex * 8.0) * 64.0);
				if (pix.x == 0.0 || pix.y == 0.0 || pix.x == 63.0 || pix.y == 63.0) c = fixed4(0.0, 0.0, 0.0, 1.0);
			}
			fixed holeFlag = tex2D(_HoleFlags, IN.uv_HoleFlags).r;
			if (holeFlag == 1.0)
			{
				fixed4 holeC = tex2D(_HoleTexture, IN.uv_HoleTexture * 8.0);
				o.Albedo = c.rgb * (1.0 - holeC.a) + holeC.rgb * holeC.a;
			}
			else o.Albedo = c.rgb;
			fixed4 n = tex2D(_BumpMap, IN.uv_BumpMap);
			o.Normal = UnpackNormal(n);

			o.Specular = 0.0;
			o.Gloss = 0.0;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
