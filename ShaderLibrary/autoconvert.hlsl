SCVertexData FromVertexInput(appdata v, SCPositionAndDirection camera, SCPositionAndDirection head, SCPositionAndDirection headBone, float3 N, float3 T, float bitangentDir)
{
    SCVertexData vertex = (SCVertexData)0;
    vertex.uv = v.uv;
    vertex.position = mul(SC_O2W(), v.vertex).xyz;
    vertex.N = N;
    vertex.T = T;
    vertex.B = normalize(cross(vertex.N, vertex.T) * v.tangent.w * bitangentDir) * length(vertex.N);
    vertex.TBN = half3x3(vertex.T, vertex.B, vertex.N);
    vertex.crossDirection = v.tangent.w;
    vertex.color = v.color;
    vertex.V = normalize(camera.position.xyz - vertex.position);
    vertex.Head = normalize(head.position.xyz - vertex.position);
    vertex.cameraDepth = dot(camera.position.xyz - vertex.position, camera.forward) > 0 ? length(camera.position.xyz - vertex.position) : 0;
    vertex.headDepth = dot(head.position.xyz - vertex.position, head.forward) > 0 ? length(head.position.xyz - vertex.position) : 0;
    return vertex;
}

void ToVertexOutput(inout v2f o, SCVertexData vertex)
{
    o.uv[0].xy = vertex.uv[0].xy;
    o.uv[1].xy = vertex.uv[1].xy;
    o.uv[2].xy = vertex.uv[2].xy;
    o.uv[3].xy = vertex.uv[3].xy;
    o.position = vertex.position;
    o.N = vertex.N;
    o.T.xyz = vertex.T;
    o.T.w = vertex.crossDirection;
}

SCVertexData FromPixelInput(v2f i, SCPositionAndDirection camera, SCPositionAndDirection head, SCPositionAndDirection headBone, float bitangentDir, bool isFront)
{
    SCVertexData vertex = (SCVertexData)0; // エラーに気づきやすいように0を代入しない
    vertex.uv = (float4[8])0;
    vertex.uv[0].xy = i.uv[0].xy;
    vertex.uv[1].xy = i.uv[1].xy;
    vertex.uv[2].xy = i.uv[2].xy;
    vertex.uv[3].xy = i.uv[3].xy;
    vertex.position = i.position;
    vertex.N = normalize(i.N);
    vertex.T = normalize(i.T.xyz);
    vertex.B = normalize(cross(vertex.N, vertex.T) * i.T.w * bitangentDir);
    vertex.TBN = half3x3(vertex.T, vertex.B, vertex.N);
    vertex.crossDirection = i.T.w;
    #if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
        /*
        vertex.position = GetAbsolutePositionWS(vertex.position);
        vertex.V = camera.position.xyz - vertex.position;
        vertex.V = normalize(vertex.V + camera.forward * max(dot(-vertex.V,camera.forward) * 2 + 0.25, 0));
        vertex.Head = head.position.xyz - vertex.position;
        vertex.Head = normalize(vertex.Head + head.forward * max(dot(-vertex.Head,head.forward) * 2 + 0.25, 0));
        if(UNITY_MATRIX_P._m33 != 0.0) vertex.V = camera.forward;
        if(UNITY_MATRIX_P._m33 != 0.0) vertex.Head = head.forward;
        vertex.cameraDepth = dot(camera.position.xyz - vertex.position, camera.forward) > 0 ? length(camera.position.xyz - vertex.position) : 0;
        vertex.headDepth = dot(head.position.xyz - vertex.position, head.forward) > 0 ? length(head.position.xyz - vertex.position) : 0;
        */
        vertex.V = -vertex.position;
        vertex.V = normalize(vertex.V + camera.forward * max(dot(-vertex.V,camera.forward) * 2 + 0.25, 0));
        vertex.Head = head.position.xyz - camera.position.xyz - vertex.position;
        vertex.Head = normalize(vertex.Head + head.forward * max(dot(-vertex.Head,head.forward) * 2 + 0.25, 0));
        if(UNITY_MATRIX_P._m33 != 0.0) vertex.V = camera.forward;
        if(UNITY_MATRIX_P._m33 != 0.0) vertex.Head = head.forward;
        vertex.cameraDepth = dot(-vertex.position, camera.forward) > 0 ? length(vertex.position) : 0;
        vertex.headDepth = dot(head.position.xyz - camera.position.xyz - vertex.position, head.forward) > 0 ? length(head.position.xyz - camera.position.xyz - vertex.position) : 0;
    #else
        vertex.V = camera.position.xyz - vertex.position;
        vertex.V = normalize(vertex.V + camera.forward * max(dot(-vertex.V,camera.forward) * 2 + 0.25, 0));
        vertex.Head = head.position.xyz - vertex.position;
        vertex.Head = normalize(vertex.Head + head.forward * max(dot(-vertex.Head,head.forward) * 2 + 0.25, 0));
        if(UNITY_MATRIX_P._m33 != 0.0) vertex.V = camera.forward;
        if(UNITY_MATRIX_P._m33 != 0.0) vertex.Head = head.forward;
        vertex.cameraDepth = dot(camera.position.xyz - vertex.position, camera.forward) > 0 ? length(camera.position.xyz - vertex.position) : 0;
        vertex.headDepth = dot(head.position.xyz - vertex.position, head.forward) > 0 ? length(head.position.xyz - vertex.position) : 0;
    #endif
    vertex.isFront = isFront;
    vertex.positionRaw = i.pos;
    vertex.uvColor = i.pos.xy / _ScreenParams.xy;
    #if defined(UNITY_SINGLE_PASS_STEREO)
        vertex.uvColor.x *= 0.5;
    #endif
    vertex.uvDepth = vertex.uvColor;
    #if UNITY_UV_STARTS_AT_TOP
        if(_ProjectionParams.x > 0) vertex.uvDepth.y = 1.0 - vertex.uvDepth.y;
    #else
        if(_ProjectionParams.x < 0) vertex.uvDepth.y = 1.0 - vertex.uvDepth.y;
    #endif
    return vertex;
}
