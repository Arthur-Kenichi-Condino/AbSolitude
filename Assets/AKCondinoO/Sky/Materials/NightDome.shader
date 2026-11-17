// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
Shader "Unlit/Sky Night Dome major0_minor0_release0_build10" {
	Properties{
					  _Alpha                                                                 (    "[_Alpha] Alpha",Range(0,1))=1
					  _BaseColor                                                             ("[_BaseColor] Base Color",Color)=(0.60,0.68,0.69,1)
					  _BaseTexture                                                           (      "[_BaseTexture] Stars",2D)="black"
					  _TimeOfDay                                                            ("[_TimeOfDay] Time Of Day",Float)=0
					  _Position                     (                                           "[_Position] Position",Vector)=(0,0,0,0)
					  _VerticalOpacityThreshold     (          "[_VerticalOpacityThreshold] Vertical Opacity Threshold",Float)=50
					  _VerticalTransparencyThreshold("[_VerticalTransparencyThreshold] Vertical Transparency Threshold",Float)=-100
	}
    SubShader{
		Tags{"Queue"="Transparent" "RenderType"="Transparent"}
		ZWrite Off
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off
        //  A single pass in our subshader
        Pass{
			CGPROGRAM
			#pragma   vertex   vertexProcess
			#pragma fragment fragmentProcess
			//  Adding fog...
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			    float _Alpha;
			   float4 _BaseColor;
	uniform sampler2D _BaseTexture;
	uniform float4    _BaseTexture_ST;
			    float _TimeOfDay;
			   float4 _Position;
			     half _VerticalOpacityThreshold;
			     half _VerticalTransparencyThreshold;
			struct input{
				float2     uv:TEXCOORD0;
				float4 vertex:POSITION;
			};
			struct output{
				float4 position:SV_POSITION0;
				float2       uv:TEXCOORD0;
				float4   vertex:POSITION1;
			};
			output vertexProcess(input i){
				output o;
				//  Convert the vertex to world space: 
				o.vertex  =mul(unity_ObjectToWorld,i.vertex); 
				o.position=   UnityObjectToClipPos(i.vertex);
				o.uv=i.uv.xy*_BaseTexture_ST.xy+_BaseTexture_ST.zw;
				return o;
			}
			fixed4 fragmentProcess(output i):SV_Target{
				fixed4 color=tex2D(_BaseTexture,i.uv);
				//  Calculate alpha according to the world Y coordinate:
				color.w=1-saturate((i.vertex.y-_VerticalOpacityThreshold)/(_VerticalTransparencyThreshold-_VerticalOpacityThreshold));
				color.w=color.w*_Alpha;
			return color;}
			ENDCG
        }
    }
	FallBack "Diffuse"
}