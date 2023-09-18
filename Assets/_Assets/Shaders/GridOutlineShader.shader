Shader "Custom/2DUnlitWithGrid"
{
    Properties
    {
        _GridWidth ("Grid Width", float) = 1.0
        _GridHeight ("Grid Height", float) = 1.0
        _OutlineThickness ("Outline Thickness", Range(0,5)) = 0.05
        _Offset ("Grid Offset", Vector) = (0,0,0,0)
        _GridColor ("Grid Color", Color) = (0,0,0,1)
        _Amplitude ("Amplitude", float) = 0.1
        _Frequency ("Frequency", float) = 1.0
        _Speed ("Speed", float) = 1.0
    }
    
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 pos : TEXCOORD0;
                float4 color : COLOR0;
                float time : TEXCOORD1;
            };

            float _GridWidth;
            float _GridHeight;
            float _OutlineThickness;
            float2 _Offset;
            float4 _GridColor;
            float _Amplitude;
            float _Frequency;
            float _Speed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.pos = v.vertex.xy;
                o.color = v.color;
                o.time = _Time.y * _Speed;  // Unity built-in _Time variable
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Grid calculations
                float2 offsetPos = i.pos + _Offset;
                float2 gridPosition;
                gridPosition.x = floor(offsetPos.x / _GridWidth);
                gridPosition.y = floor(offsetPos.y / _GridHeight);
                float2 gridUV = offsetPos - gridPosition * float2(_GridWidth, _GridHeight);
                float outlineX = step(_OutlineThickness, gridUV.x) - step(_GridWidth - _OutlineThickness, gridUV.x);
                float outlineY = step(_OutlineThickness, gridUV.y) - step(_GridHeight - _OutlineThickness, gridUV.y);
                float outline = max(outlineX, outlineY);

                // Check if the color is blue
                if (i.color.b > i.color.r && i.color.b > i.color.g)
                {
                    // Generate a simple water effect using a sine wave
                    float wave = sin(i.vertex.y * _Frequency + i.time) * _Amplitude;
                    float4 shiftedColor = float4(i.color.rgb + wave, i.color.a);
                    float4 color = (1 - outline) * shiftedColor + outline * _GridColor;
                    return color;
                }
                else
                {
                    float4 color = (1 - outline) * i.color + outline * _GridColor;
                    return color;
                }
            }
            ENDCG
        }
    }
}
