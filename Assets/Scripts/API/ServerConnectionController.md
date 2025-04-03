# ServerConnection

This class manages the connection to a server for synchronizing room state.

## Public Properties

-   `hostname`: The hostname of the server.
-   `roomId`: The ID of the room.
-   `updateToken`: The token used for updating the room state.
-   `instance`: A static instance of the ServerConnection class.

## Public Methods

-   `void Awake()`
    -   Ensures there's only one ServerConnection object.
-   `void Update()`

## Private Fields

-   `lastSync`: Stores the last synchronization time.
-   `SYNC_FREQUENCY_MS`: Defines the synchronization frequency in milliseconds.

## Private Methods

-   `async Task getAndUseRoomState()`
    -   Fetches the room state and sets settings and equations appropriately.
-   `async Task<Boolean> pushRoomState(ParseTreeNode[] parseTrees, GraphSettings graphSettings)`
    -   Pushes the equations (token parse trees) and the room settings to the server.