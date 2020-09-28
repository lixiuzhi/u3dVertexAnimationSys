// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "LXZ/VertexAnimation" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CurTime("_CurTime", Float) = 0 
		_Frame2Time("Frame2Time", Float) = 0.333
		_Frame3Time("Frame3Time", Float) = 0.666
		_Color ("MainColor", color) = (1,1,1,1)
	}
	SubShader {
	    Tags { "QUEUE"="Geometry"  "RenderType"="Opaque" }
	   
	    Pass {  
		//Blend  SrcAlpha OneMinusSrcAlpha		
	    CGPROGRAM
	    #pragma vertex vert
	    #pragma fragment frag
		#pragma multi_compile_instancing

		
	   #include "UnityCG.cginc"  
	   
	   
	   #pragma glsl_no_auto_normalization  
	     
	    sampler2D _MainTex;
 
		float4 _Color;
	    
	    struct appdata {
	        float4 vertex : POSITION;
			float3 vertex1 : NORMAL;
			float4 vertex2 : TANGENT;
	        float2 texcoord : TEXCOORD0;
		    float2 vertex3: TEXCOORD1; 
			UNITY_VERTEX_INPUT_INSTANCE_ID
	    }; 
	    
	    struct v2f {
	        float4 pos : POSITION;
	        float2 uv : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
	    };
	    
		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_DEFINE_INSTANCED_PROP(float,_CurTime) 
		UNITY_DEFINE_INSTANCED_PROP(float,_Frame2Time) 
		UNITY_DEFINE_INSTANCED_PROP(float,_Frame3Time)  
		UNITY_INSTANCING_BUFFER_END(Props)
	    
	    v2f vert(appdata v) {
	        v2f result; 
			UNITY_SETUP_INSTANCE_ID(v); //这里第三步
            UNITY_TRANSFER_INSTANCE_ID(v,result); //第三步

			float ct = UNITY_ACCESS_INSTANCED_PROP(Props,_CurTime);
			float f2t = UNITY_ACCESS_INSTANCED_PROP(Props,_Frame2Time);
			float f3t = UNITY_ACCESS_INSTANCED_PROP(Props,_Frame3Time);
			
			float a = ct - f2t;
			float b = ct - f3t;
			
		float c = 1 - f3t;

			float3 vec; 
			
			float3 vertex3 = float3(v.vertex2.w,v.vertex3.xy);
			
			if(a<0)
			vec = v.vertex.xyz + (v.vertex1 - v.vertex.xyz)* ct/f2t; 
			else if(a>=0 && b<0)
			vec = v.vertex1 + (v.vertex2.xyz - v.vertex1)* a/(f3t-f2t);
			else if( c>0)
			vec = v.vertex2.xyz + (vertex3 - v.vertex2.xyz)* b/(c);
			else
			vec = vertex3;  
			
			result.pos = UnityObjectToClipPos(float4(vec,1));  
			result.uv = v.texcoord; 
			
			return result;
	    }
	    
	    float4 frag(v2f i) : COLOR
	    {
			UNITY_SETUP_INSTANCE_ID(i);
	        float4 color = tex2D(_MainTex, i.uv);
	        return color *_Color;
	    }
	    
		ENDCG
		
		//SetTexture [_MainTex] {combine texture}
		}
	} 
	FallBack "Diffuse"
}
