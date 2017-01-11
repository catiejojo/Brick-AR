﻿Shader "CatieJo/Dynamic Mesh Shader" 
{ 
	 SubShader 
	 {
        Pass
        {
 			ZWrite On

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            uniform sampler2D _CameraDepthTexture; //the depth texture
 
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 projPos : TEXCOORD1; //Screen position of pos
            };

            v2f vert(float4 pos : POSITION)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, pos);
                o.projPos = ComputeScreenPos(o.pos);
                return o;
            }
 
            half4 frag(v2f i) : COLOR
            {
                //Grab the depth value from the depth texture
                //Linear01Depth restricts this value to [0, 1]
                float depth = Linear01Depth (tex2Dproj(_CameraDepthTexture,
                                                             UNITY_PROJ_COORD(i.projPos)).r);
            	half4 c;
                c.r = depth;
                c.g = depth;
                c.b = depth;
                c.a = 1;
 
                return c;
            }

            ENDCG
        }		 
	 }
	 Fallback "Transparent/Cutout/VertexLit"
 }