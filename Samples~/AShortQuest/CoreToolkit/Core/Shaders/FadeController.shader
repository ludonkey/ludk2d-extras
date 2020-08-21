Shader "LuDK/UI/FadeController"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        [Toggle(LUDK_UI_FADE_APPEARING_BLACK_TO_WHITE)] _AppearingBlackToWhite("Appearing Black To White", Float) = 0
        _TransitionTex("Transition Texture", 2D) = "white" {}
        _BgColor("Background Color", Color) = (1,1,1,1)
        _DisplayFactor("Display Factor", Range(0, 1)) = 1        
        _TransitionSmoothness("Transition Smoothness", Range(0, 1)) = 1
        [Toggle(LUDK_UI_FADE_REMAP_MIN_MAX)] _RemapMinMax("Remap Black/White Range", Float) = 0
        _MaxWhite("Max White", Range(0, 1)) = 1
        _MaxDark("Max Dark", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature LUDK_UI_FADE_APPEARING_BLACK_TO_WHITE
            #pragma shader_feature LUDK_UI_FADE_REMAP_MIN_MAX

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
            sampler2D _TransitionTex;
            float4 _BgColor;
            float _MaxWhite;
            float _MaxDark;
            float _DisplayFactor;
            float _TransitionSmoothness;

            float invLerp(float from, float to, float value) {
                return saturate((value - from) / (to - from));
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 transitionColor = tex2D(_TransitionTex, i.uv);
                float currentValue = transitionColor.b;
                #ifdef LUDK_UI_FADE_REMAP_MIN_MAX
                currentValue = invLerp(1 - _MaxDark, _MaxWhite, currentValue);
                #endif                
                #ifdef LUDK_UI_FADE_APPEARING_BLACK_TO_WHITE
                currentValue = 1 - currentValue;
                #endif
                float delta = currentValue - 1 + _DisplayFactor;
                float4 imgColor = tex2D(_MainTex, i.uv);
               
                float alphaFactor = delta >= 0 && _DisplayFactor  > 0 ? 1 : (_DisplayFactor > 0 && delta >= (-0.01 * _TransitionSmoothness) ? (0.01 + delta) * 100 : 0);
                               
                return float4(imgColor.r * _BgColor.r, imgColor.g * _BgColor.g, imgColor.b * _BgColor.b, imgColor.a * alphaFactor * _BgColor.a);
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
