# API Response Classes

This file defines the JSON response structures expected from various API endpoints.

## API_Response\<T>

A generic class representing a standard API response.

### Generic Type Parameter

-   `T`: The type of the data field.

### Public Fields

-   `T data`: The data returned by the API.
-   `string error`: An error message, if any.

## API_GET_RoomState

Represents the JSON response for retrieving room state.

### Public Fields

-   `RoomInfo_RoomState data`: The room state data.

## API_POST_RoomState

Represents the JSON structure for posting room state.

### Public Fields

-   `string key`: The update key.
-   `RoomInfo_RoomState roomState`: The room state to be posted.

## API_POST_CreateResult

Represents the JSON response for creating a room.

### Public Fields

-   `RoomInfo data`: The room information.

## API_ServerPingResponse

Represents the JSON response for a server ping.

### Public Fields

-   `string status`: The server status.
-   `int protocolVersion`: The server's protocol version.

## RoomInfo

Represents information about a room.

### Public Fields

-   `string roomId`: The room ID.
-   `string ownerUpdateToken`: The owner's update token.
-   `RoomInfo_RoomState roomState`: The room's state.

## RoomInfo_RoomState

Represents the state of a room.

### Public Fields

-   `GraphSettings settings`: The graph settings.
-   `ParseTreeNode[] equations`: An array of parse tree nodes representing equations.

## ACCEPTED_PROTOCOL

A static class defining the accepted protocol version.

### Public Constants

-   `int VERSION`: The accepted protocol version.