Shader "Custom/Desolve_Shader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        [HDR] _Light_Band_Color("Light Band Color",Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _Perlin_Noise ("Perlin (RGB)", 2D) = "white" {}
        _Threshold("Vanishing",Range(0,1))=0
        _Line_Size("Light Band Threshold",Range(0,1))=0
    }
    SubShader
    {
        //Tags { "RenderType"="Opaque" }
        Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue"="Transparent" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Perlin_Noise;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Perlin_Noise;
        };

        half _Glossiness;
        half _Metallic;
        half _Threshold;
        half _Line_Size;
        fixed4 _Color;
        fixed4 _Light_Band_Color;

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
            o.Alpha = c.a;

            if(_Threshold - _Line_Size < tex2D (_Perlin_Noise, IN.uv_Perlin_Noise).r){
                o.Albedo = _Light_Band_Color.rgb;
                o.Emission = _Light_Band_Color.rgb;
            }
                

            if(_Threshold < tex2D (_Perlin_Noise, IN.uv_Perlin_Noise).r){
                o.Albedo = (0,0,0);
                o.Alpha = 0;
            }
                

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
