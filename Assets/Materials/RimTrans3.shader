Shader "Custom/RimTrans3" {
    Properties {
       _MainTex ("Color (RGB) Alpha (A)", 2D) = "white"
      _BumpMap ("Bumpmap", 2D) = "bump" {}
      _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,1.0)
      _AlbedoColor ("Albedo Color", Color) = (0.26,0.19,0.16,0.3)
      _Emission ("Emission", Range(0.0,1.0)) = 0.1
      _RimPower ("RimPower", Range(0.0,1.0)) = 0.1


    }
    SubShader {
	Tags { "Queue"="Transparent" "RenderType"="Transparent" }


	Lighting On //Cull Off ZWrite On
	//Blend SrcAlpha OneMinusSrcAlpha

    //BlendOp Add
    //Blend OneMinusDstColor One, One Zero // screen
   //Blend SrcAlpha One, One Zero // linear dodge

      CGPROGRAM
      #pragma surface surf Lambert addshadow alpha 

      struct Input {
          float2 uv_MainTex;
          float2 uv_BumpMap;
          float3 viewDir;
          float4 screenPos;
      };

      sampler2D _MainTex;
      sampler2D _BumpMap;
      float4 _RimColor;
      float4 _AlbedoColor;
      float _Alpha;
      float _Emission;
      float _RimPower;

 

      void surf (Input IN, inout SurfaceOutput o) {

        o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
        o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
        // o.Alpha = tex2D (_MainTex, IN.uv_MainTex).a;

        float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
        float2 reticle = float2(0.5,0.5);
        float dist = distance(screenUV, reticle);
        dist =  abs ( (dist/0.75) - 1.0 ) ;

        half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
        float rimpow = pow (rim, _RimPower);

           half rimInv = saturate(dot (normalize(IN.viewDir), o.Normal));

        rimInv = rimInv * dist;
        rimInv = pow(rimInv, 4);

     	o.Emission = _RimColor.rgb *  lerp( rimpow, _Emission, rimpow ) ;


     

        o.Albedo = _AlbedoColor ;

        o.Alpha =  ( _AlbedoColor.a * rimInv ) +   _RimColor.a *  ( (o.Emission.r + o.Emission.g + o.Emission.b) / ( _RimColor.r + _RimColor.g + _RimColor.b ) );

      }

      ENDCG
    } 
    Fallback "Diffuse"
  }