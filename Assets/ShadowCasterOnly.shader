Shader "Custom/ShadowCasterOnly" {
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
        }
    }
}
