using UnityEngine;

namespace DisplayBoltPositions
{
    internal class IndicatorShader
    {
        public static readonly int ColorProperty = Shader.PropertyToID("_Color");

        private static float PulseSpeed = 1f;
        private static float MinOpacity = 0.2f;
        private static float MaxOpacity = 0.8f;

        private static string Code = $@"
            Shader ""Custom/DisplayBoltPositions_Indicator""
            {{
                Properties
                {{
                    _Color (""Color"", Color) = (0,1,1,0.5)
                }}
                SubShader
                {{
                    Tags {{ ""Queue""=""Transparent+1000"" }}
                    Pass
                    {{
                        ZTest Always
                        ZWrite Off
                        Blend SrcAlpha OneMinusSrcAlpha
                        Color [_Color]
                    }}
                }}
            }}";

        public static Material Material = new Material(Code);
    }
}
