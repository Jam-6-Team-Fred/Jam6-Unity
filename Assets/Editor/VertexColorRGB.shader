Shader "SceneView/VertexColorRGB"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            appdata_full vert(appdata_full i)
            {
                appdata_full o = i;
                o.vertex = UnityObjectToClipPos(i.vertex);
                return o;
            }

            float3 frag(appdata_full i) : SV_Target
            {
                return i.color.rgb;
            }
            ENDCG
        }
    }
}