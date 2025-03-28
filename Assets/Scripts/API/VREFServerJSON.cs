// This file implements the JSON responses that we can expect from various endpoints
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class API_ServerPingResponse {
    public string status;
    public int protocolVersion;
}

[Serializable]
public class API_RoomInfoResponse {
    public API_RoomInfo data;
    public string error;
}

[Serializable]
public class API_RoomInfo {
    public string roomId;
    public string ownerUpdateToken;
    public API_RoomInfo_RoomState roomState;
}

[Serializable]
public class API_RoomInfo_RoomState {
    public GraphSettings settings;
    public ParseTreeNode[] equations;
}