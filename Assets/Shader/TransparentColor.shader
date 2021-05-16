Shader "Custom/TransparentColor" {
    
Properties {
    _Color ("Color", Color) = (1,1,1,1)
}
    
Category {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True"}
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
    
    SubShader {Pass {
        GLSLPROGRAM
        #ifdef FRAGMENT
        uniform lowp vec4 _Color;
        void main() {
            gl_FragColor = _Color;
        }
        #endif     
        ENDGLSL
    }}
    
    SubShader {Pass {
        SetTexture[_MainTex] {Combine texture * constant ConstantColor[_Color]}
    }}
}
    
}