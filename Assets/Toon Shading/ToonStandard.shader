Shader "Toon/ToonStandard"
{
	Properties
	{
		//DIFFUSE
		_MainTex ("Albedo", 2D) = "white" {}
		_Color("Tint", Color) = (0.5,0.5,0.5,1.0)
		
		//TOON COLORS RAMP
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
		_HColor("Highlight Color", Color) = (0.6,0.6,0.6,1.0)
		_SColor("Shadow Color", Color) = (0.3,0.3,0.3,1.0)
		_Saturation("Saturation", Range(1,10)) = 1

		//OTHER PROPERTIES
		_NormalMap("Normal Map", 2D) = "bump" {}
		_NormalScale("Normal Strength", Float) = 1
		//_EmissionMap("Emission", 2D) = "black" {}
		//_Emission("Emission", Color) = (0, 0, 0)
		_OcclusionMap("Occlusion", 2D) = "white" {}
		_OcclusionStrength("Occlusion Strength", Range(0, 1)) = 1
		
		//RIM LIGHT
		_RimColor ("Rim Color", Color) = (0.8,0.8,0.8,0.6)
		_RimMin ("Rim Min", Range(0,1)) = 0.5
		_RimMax ("Rim Max", Range(0,1)) = 1.0
		
		_SpecAmount ("Specular Amount", Range(0,1)) = 0
		_Roughness("Specular Amount", Range(0,1)) = 0
	}
	
	SubShader
	{
		//Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
		//Blend SrcAlpha OneMinusSrcAlpha
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		
		#pragma surface surf ToonyColorsCustom
		#pragma target 2.0
		#pragma glsl
		
		
		//================================================================
		// VARIABLES
		
		fixed4 _Color;
		sampler2D _MainTex;
		sampler2D _NormalMap;
		//sampler2D _EmissionMap;
		//float3 _Emission;
		sampler2D _OcclusionMap;
		float _OcclusionStrength;
		
		fixed4 _RimColor;
		fixed _RimMin;
		fixed _RimMax;
		float4 _RimDir;
		
		struct Input
		{
			half2 uv_MainTex;
			half2 uv_NormalMap;
			//half2 uv_EmissionMap;
			half2 uv_OcclusionMap;
			float3 viewDir;
		};
		
		//================================================================
		// CUSTOM LIGHTING
		
		//Lighting-related variables
		fixed4 _HColor;
		fixed4 _SColor;
		fixed _Saturation;
		sampler2D _Ramp;
		fixed _SpecAmount;
		fixed _Roughness;
		
		//Custom SurfaceOutput
		struct SurfaceOutputCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			//float Occlusion;
			half Specular;
			fixed Alpha;
		};

		//GET FUNCTIONS
		float GetOcclusion(half2 uv) {
			#if defined(_OCCLUSION_MAP)
				return lerp(1, tex2D(_OcclusionMap, uv.xy).g, _OcclusionStrength);
			#else
				return 1;
			#endif
		}
		
		inline half4 LightingToonyColorsCustom (SurfaceOutputCustom s, half3 lightDir, half3 viewDir, half atten)
		{
			/*float4 hdrReflection = 1.0;
			float3 reflectedDir = reflect(-viewDir, s.Normal);//halfasview
			float4 reflection = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, 0);
			//UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflectedDir);
			hdrReflection.rgb = DecodeHDR(reflection, unity_SpecCube0_HDR);
			hdrReflection.a = 1.0;
			hdrReflection = lerp(1, hdrReflection, _Roughness);*/

			s.Normal = normalize(s.Normal);
			fixed ndl = max(0, dot(s.Normal, lightDir)*0.5 + 0.5);
			
			fixed3 ramp = tex2D(_Ramp, fixed2(ndl,ndl));
			#if !(POINT) && !(SPOT)
				ramp *= atten;
			#endif
			_SColor = lerp(_HColor, _SColor, _SColor.a);	//Shadows intensity through alpha
			ramp = lerp(_SColor.rgb,_HColor.rgb,ramp);
			fixed4 c;
			c.rgb = s.Albedo * pow(_LightColor0.rgb, _Saturation) * ramp;
			c.a = s.Alpha;
			#if (POINT || SPOT)
				c.rgb *= atten;
			#endif

			return c;
		}
		
		//================================================================
		// SURFACE FUNCTION
		
		void surf (Input IN, inout SurfaceOutputCustom o)
		{
			fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 aoTex = tex2D(_OcclusionMap, IN.uv_OcclusionMap);
			//fixed4 a = lerp(lerp(1, _SColor, _OcclusionStrength), 1, aoTex);
			
			o.Albedo = mainTex.rgb * _Color.rgb * 0.7 * lerp(lerp(1, _SColor, _OcclusionStrength), 1, aoTex);//_SColor * tex2D(_OcclusionMap, IN.uv_OcclusionMap);
			o.Alpha = mainTex.a * _Color.a;
			o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));// *_NormalScale;
			
			//Rim
			float3 viewDir = normalize(IN.viewDir);
			half rim = 1.0f - saturate( dot(viewDir, o.Normal) );
			rim = smoothstep(_RimMin, _RimMax, rim);
			o.Emission += (_RimColor.rgb * rim) * _RimColor.a * aoTex;
			
			//Specular
			half inner = saturate(dot(viewDir, o.Normal));
			inner = smoothstep(1 - _Roughness, 1, inner);
			o.Emission += (_HColor.rgb * inner) * _SpecAmount * aoTex;
		}
		
		ENDCG
	}
	
	Fallback "Diffuse"
	CustomEditor "ToonShadingGUI"
}
