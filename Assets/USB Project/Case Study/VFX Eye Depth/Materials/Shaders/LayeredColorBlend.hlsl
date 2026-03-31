void LayeredColorBlend_float (in float4 A, in float4 B, in float4 C, out float3 Out)
{   
    float3 bb = B.rgb * B.a;
    float3 cc = C.rgb * C.a;
    
    float bbd = dot(B, B);
    float ccd = dot(C, C);
    
    Out = A.rgb;
    
    float t = 0.0f;
    
    if (B.a > 0.0f)
    {
        Out = lerp(Out, B, B.a);
    }    
    
    if (cc.x > t && cc.y > t && cc.z > t)
    {
        Out = lerp(Out, Out + cc, C);
    }      
}