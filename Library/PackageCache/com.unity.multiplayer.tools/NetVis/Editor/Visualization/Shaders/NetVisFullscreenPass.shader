Shader "Hidden/NetVisFullscreenPass"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM

            #pragma shader_feature NET_SCENE_VIS_OUTLINE

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            int width;
            int height;

            // The texture of the rendered scene
            uniform sampler2D _SceneRenderTex;
            fixed4            _SceneRenderTex_ST;

            // The texture of the ObjectID buffer
            uniform sampler2D _ObjectIdTex;
            fixed4            _ObjectIdTex_ST;
            fixed4            _ObjectIdTex_TexelSize;

            // Array mapping ObjectIDs to colors
            StructuredBuffer<fixed4> _ObjectIdToColorBuffer;

            uniform float  _NetworkSceneSaturation;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _ObjectIdTex);
                return o;
            }

            int decodeObjectId(float4 color)
            {
                return (int)(color.r * 255) +
                      ((int)(color.g * 255) <<  8) +
                      ((int)(color.b * 255) << 16) +
                      ((int)(color.a * 255) << 24);
            }

#if NET_SCENE_VIS_OUTLINE
            // Returns 0 if a neighbouring pixel belongs to another object and 1 otherwise,
            // to be multiplied with the RGB component to create an outline
            float getEdgeOutlineMultiplier(float2 uv, int objectId, float highlightAlpha)
            {
                const float2 offsetRight = float2(_ObjectIdTex_TexelSize.x, 0);
                const float2 offsetUp    = float2(0, _ObjectIdTex_TexelSize.y);

                const int objectIdRight = decodeObjectId(tex2D(_ObjectIdTex, uv + offsetRight));
                const int objectIdUp = decodeObjectId(tex2D(_ObjectIdTex, uv + offsetUp));

                const bool isObjectBoundary =
                    objectId != objectIdRight ||
                    objectId != objectIdUp;

                const float alphaUp = _ObjectIdToColorBuffer.Load(objectIdUp).a;
                const fixed4 alphaRight = _ObjectIdToColorBuffer.Load(objectIdRight).a;

                const bool anyObjectNetworked = highlightAlpha > 0 || alphaUp > 0 || alphaRight > 0;

                const bool shouldDrawOutline = isObjectBoundary && anyObjectNetworked;
                return shouldDrawOutline ? 0 : 1;
            }
#endif

            fixed4 ColorToGrayscale(fixed4 color)
            {
                // Rec. 601: https://en.wikipedia.org/wiki/Rec._601
                // float grey = 0.299 * col2.x + 0.587 * col2.y + 0.114 * col2.z;
                // Rec. 709: https://en.wikipedia.org/wiki/Rec._709
                const float  grey = 0.2126 * color.x + 0.7152 * color.y + 0.0722 * color.z;
                const fixed4 grayscale = fixed4(grey, grey, grey, 1);

                color = lerp(grayscale, color, _NetworkSceneSaturation);
                return color;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                const fixed4 objectIdF4 = tex2D(_ObjectIdTex, i.uv);
                const int objectId = decodeObjectId(objectIdF4);
                const fixed4 highlightColor = _ObjectIdToColorBuffer.Load(objectId);

                const fixed4 sceneViewColor = ColorToGrayscale(tex2D(_SceneRenderTex, i.uv));

                fixed4 resultColor = highlightColor.a > 0.0f ? highlightColor : sceneViewColor;

#if NET_SCENE_VIS_OUTLINE
                resultColor.rgb *= getEdgeOutlineMultiplier(i.uv, objectId, highlightColor.a);
#endif
                return resultColor;
            }
            ENDCG
        }
    }
}
