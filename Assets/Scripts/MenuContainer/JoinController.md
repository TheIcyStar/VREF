# JoinController

This class manages the UI and logic for joining a multiplayer game session.

## Public Fields

### `hostInputField : GameObject`

### `joinButton : GameObject`

## Public Methods

### `beginJoin() : void`

### `beginJoinSolo() : void`

## Private Fields

### `buttonTextComponent : TMP_Text`

## Private Methods

### `Start() : void`

### `checkConnection(string inputText) : Task<ServerResponseStatus>`

Async function for checking if the host and room number are OK

### `checkRoom(string host, string room) : Task<ServerResponseStatus>`

### `resetButtonText() : void`