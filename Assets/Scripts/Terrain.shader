﻿Shader "Custom/Terrain"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
//#pragma exclude_renderers d3d11 gles
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		const static int maxColorCount = 8;

		int baseColorCount;
		float3 baseColors[maxColorCount];
		float baseStartHeights[maxColorCount];

		float minHeight;
		float maxHeight;

		struct Input
		{
			float3 worldPos;
		};

		float inverseLerp(float a, float b, float value)
		{
			return saturate((value - a) / (b - a));
		}

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
			for (int i = 0; i < baseColorCount; i++)
			{
				float drawStrength = saturate(sign(heightPercent - baseStartHeights[i]));
				o.Albedo = o.Albedo * ( 1 - drawStrength) + baseColors[i] * drawStrength;
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}