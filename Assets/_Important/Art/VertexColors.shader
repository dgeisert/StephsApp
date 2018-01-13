 Shader "Custom/VertexColors" {
     Properties {
         _Color ("Color", Color) = (1,1,1,1)
         _MainTex ("Albedo (RGB)", 2D) = "white" {}
     }
     SubShader {
         Tags { "RenderType"="Opaque" }
         LOD 200
         
         CGPROGRAM
         #pragma surface surf Standard vertex:vert fullforwardshadows
         #pragma target 3.0
         struct Input {
             float2 uv_MainTex;
             float3 vertexColor; // Vertex color stored here by vert() method
         };
         
         struct v2f {
           float4 pos : SV_POSITION;
           fixed4 color : COLOR;
         };
 
         void vert (inout appdata_full v, out Input o)
         {
             UNITY_INITIALIZE_OUTPUT(Input,o);
             o.vertexColor = v.color; // Save the Vertex Color in the Input for the surf() method
         }
 
         sampler2D _MainTex;
 
         fixed4 _Color;
 
         void surf (Input IN, inout SurfaceOutputStandard o) 
         {
             // Albedo comes from a texture tinted by color
             fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
             o.Albedo = c.rgb * IN.vertexColor; // Combine normal color with the vertex color
             o.Alpha = c.a;
         }
         ENDCG
     } 
     FallBack "Diffuse"
 }