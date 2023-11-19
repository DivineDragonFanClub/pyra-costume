Shader "CustomRP/Chara/CharaStandard" {
Properties {
_BaseColor ("Color", Color) = (1,1,1,1)
_BaseMap ("Albedo", 2D) = "white" { }
_BumpMap ("Normal Map", 2D) = "bump" { }
_BumpScale ("BumpScale", Range(0.01, 2)) = 1
_MultiMap ("Multi Map", 2D) = "black" { }
_ToonRamp ("Toon Ramp", 2D) = "white" { }
_ToonRampMetal ("Toon Ramp Metal", 2D) = "white" { }
_ToonShadowColor ("Toon Shadow Color", Color) = (1,1,1,1)
_Makeup ("Makeup", Range(0, 1)) = 0
_OcclusionIntensity ("_OcclusionIntensity", Range(0, 1)) = 1
_EmissionMap ("Emission Map", 2D) = "white" { }
[HDR]_EmissionColor ("Emission Color", Color) = (0,0,0,1)
_OutlineColor ("OutlineColor", Color) = (0.5,0.5,0.5,1)
_OutlineScale ("OutlineScale", Range(0, 10)) = 5
_OutlineTexMipLevel ("OutlineTexMipLevel", Range(0, 12)) = 4
_OutlineOriginalColorRate ("OutlineOriginalColorRate", Range(0, 1)) = 0
_OutlineGameScale ("OutlineGameScale", Range(0, 1)) = 1
[Toggle] _S_Key_RimLight ("Use Rim Light", Float) = 1
_RimLightColorLight ("RimLightColorLight", Color) = (1,1,1,1)
_RimLightColorShadow ("RimLightColorShadow", Color) = (1,1,1,1)
_RimLightBlend ("RimLightBlend", Range(0, 1)) = 0
_RimLightScale ("RimLightScale", Range(0, 1)) = 0
[Toggle(_S_KEY_COLOR_CHANGE_MASK)] _S_Key_ColorChangeMask ("Color Change Mask", Float) = 0
_ColorChangeMask100 ("Mask 1.0", Color) = (1,1,1,1)
_ColorChangeMask075 ("Mask 0.75", Color) = (1,1,1,1)
_ColorChangeMask050 ("Mask 0.5", Color) = (1,1,1,1)
_ColorChangeMask025 ("Mask 0.25", Color) = (1,1,1,1)
_LightColorToWhite ("Light Color To White", Range(0, 1)) = 0
_LightShadowToWhite ("Light Shadow To White", Range(0, 1)) = 0
[Toggle(_KEY_DITHER_ALPHA)] _Key_DitherAlpha ("Dither Alpha", Float) = 0
_DitherAlphaValue ("Dither Alpha Value", Range(0, 1)) = 1
[Toggle(_S_KEY_BUMP_ATTENUATION)] _S_Key_BumpAttenuation ("", Float) = 0
_BumpCameraAttenuation ("BumpCameraAttenuation", Range(0, 1)) = 0.2
[Toggle(_KEY_ENGAGE)] _Key_Engage ("Engage", Float) = 0
_EngageEmissionColor ("Engage Emission Color", Color) = (0.314,0.314,0.47,1)
[Toggle(_S_KEY_MORPH_SKIN)] _S_Key_MorphSkin ("Morph (Skin)", Float) = 0
_MorphPatternMap ("Pattern Map", 2D) = "white" { }
_MorphEmissionMap ("Emission Map", 2D) = "white" { }
[Toggle(_S_KEY_MORPH_DRESS)] _S_Key_MorphDress ("Morph (Dress)", Float) = 0
_ToonRamp_Morph ("Toon Ramp Morph", 2D) = "white" { }
_ToonRampMetal_Morph ("Toon Ramp Metal Morph", 2D) = "white" { }
[Toggle(_S_KEY_STANDARD_COLOR)] _S_Key_StandardColor ("Standard Color", Float) = 0
[Toggle(_S_KEY_STANDARD_SKIN)] _S_Key_StandardSkin ("Standard Skin", Float) = 0
[Toggle(_DEV_KEY_TOON_SPECULAR_BY_LIGHT)] _Dev_KeyToonSpecularByLight ("(DEV) Metal Type", Float) = 0
[Toggle(_DEBUG_CUSTOM_OUTLINE_ONLY)] _DEBUG_CUSTOM_OUTLINE_ONLY ("Debug Outline Only", Float) = 0
[Toggle] _DisableOutline ("Disable Outline", Float) = 0
_Preset ("Preset", Float) = 0
}
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
