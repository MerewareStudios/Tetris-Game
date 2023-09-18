Shader "Custom/GrabDistort"
{
    Properties
    {
//        _MaskTexture ("Mask texture", 2D) = "white" {}
        [Normal]_DistortionGuide("Distortion guide", 2D) = "bump" {}
        _Power("Power", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "LightMode" = "UseColorTexture"}
        Cull Off
        ZWrite Off
        LOD 100
 
        GrabPass
        {
            "_GrabbedTexture"
        }
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 distortionUV : TEXCOORD1;
                float4 grabPassUV : TEXCOORD2;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
 
            float _Power;
            sampler2D _DistortionGuide;
            float4 _DistortionGuide_ST;
            // sampler2D _MaskTexture;
            // float4 _MaskTexture_ST;
            sampler2D _GrabbedTexture;
 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // o.uv = TRANSFORM_TEX(v.uv, _MaskTexture);
                o.uv = v.uv;
                o.distortionUV = TRANSFORM_TEX(v.uv, _DistortionGuide);
                o.grabPassUV = ComputeGrabScreenPos(o.vertex);
                o.color = v.color;
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                // fixed mask = tex2D(_MaskTexture, i.uv).x;
                float2 distortion = UnpackNormal(tex2D(_DistortionGuide, i.distortionUV)).xy;
                // distortion *= _DistortionAmount * mask * i.color.a;
                distortion *= _Power;
                i.grabPassUV.xy += distortion * i.grabPassUV.z;
                fixed4 col = tex2Dproj(_GrabbedTexture, i.grabPassUV);
                return col;
            }
            ENDCG
        }
    }
}