
Shader "LXZ/VertexAnimation-NoColorBuf" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CurTime("Time", Float) = 0 
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
	   // #include "UnityCG.cginc"  
	   
	   
	   #pragma glsl_no_auto_normalization 
	   //http://docs.unity3d.com/Manual/SL-ShaderPrograms.html
	     
	    sampler2D _MainTex;
		float _CurTime;
	    float _Frame2Time;
		float _Frame3Time;
		float4 _Color;
	    
	    struct appdata {
	        float4 vertex : POSITION;
			float3 vertex1 : NORMAL;
			float4 vertex2 : TANGENT;
	        float2 texcoord : TEXCOORD0;
		    float2 vertex3: TEXCOORD1;
			//float3 vertex4: COLOR;
	    }; 
	    
	    struct v2f {
	        float4 pos : POSITION;
	        float2 uv : TEXCOORD0;
	    };
	    

	    
	    v2f vert(appdata v) {
	        v2f result; 
			
			float a = _CurTime - _Frame2Time;
			float b = _CurTime - _Frame3Time;
			
		   float3 vec; 
		   
		   float3 vertex3 = float3(v.vertex2.w,v.vertex3.xy);
		   
		 	if(a<0)
			   vec = v.vertex.xyz + (v.vertex1 - v.vertex.xyz)* _CurTime/_Frame2Time; //转到0-1插值
		 	else if(a>=0 && b<0)
			{ 
			    vec = v.vertex1 + (v.vertex2.xyz - v.vertex1)* a/(_Frame3Time-_Frame2Time); //转到0-1插值
			} 
			else
			   vec = v.vertex2.xyz + (vertex3 - v.vertex2.xyz)* b/(1-_Frame3Time); //转到0-1插值
			
			   
	       result.pos = mul(UNITY_MATRIX_MVP, float4(vec,1)); 
			//result.pos = mul(UNITY_MATRIX_MVP, float4(v.vertex1.xyz,1)); 
			//result.pos = mul(UNITY_MATRIX_MVP, float4(v.vertex2.xyz,1)); 
			// result.pos = mul(UNITY_MATRIX_MVP, float4(vertex3.xyz,1)); 
			// result.pos = mul(UNITY_MATRIX_MVP, float4(vertex4.xyz,1)); 
	        result.uv = v.texcoord; 
	        
	        return result;
	    }
	    
	    float4 frag(v2f i) : COLOR
	    {
	        float4 color = tex2D(_MainTex, i.uv);
	        return color *_Color;
	    }
	    
		ENDCG
		
		//SetTexture [_MainTex] {combine texture}
		}
	} 
	FallBack "Diffuse"
}
