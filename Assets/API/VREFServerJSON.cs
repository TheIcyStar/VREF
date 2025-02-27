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
public class API_Response<T> {
    T data;
    string error;
}

[Serializable]
public class API_RoomInfo {
    string roomId;
    int ownerId;
    List<int> attendeeIds;
    API_RoomInfo_RoomState roomState;
}

public class API_RoomInfo_RoomState { //inventing naming conventions for representing json on the fly here 🤠, how do people do json in c# anyway?
    List<string> equations; //todo: change to tokens
    List<string> objects;
}