// This file implements the JSON responses that we can expect from various endpoints
using System;

[Serializable]
public class API_Response<T> { //Can't use nested generics with JsonUtility.FromJson<>() :c
    public T data;
    public string error;
}

public class API_GET_RoomState {
    public RoomInfo_RoomState data;
    // public string error;
}

[Serializable]
public class API_POST_RoomState {
    public string key;
    public RoomInfo_RoomState roomState;
}

[Serializable]
public class API_POST_CreateResult {
    public RoomInfo data;
}

[Serializable]
public class API_ServerPingResponse {
    public string status;
    public int protocolVersion;
}

[Serializable]
public class RoomInfo {
    public string roomId;
    public string ownerUpdateToken;

    public RoomInfo_RoomState roomState;
}

[Serializable]
public class RoomInfo_RoomState {
    public GraphSettings settings;
    public ParseTreeNode[] equations;
}

public static class ACCEPTED_PROTOCOL {
    public const int VERSION = 0;
}