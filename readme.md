# au-client

This is a fun little educational demo of how Among Us can be implemented in Unity with tasks replaced with Computer Science themed questions.
This implementation targets WebGL and communiactes with the server through WebSockets.

For the server, see https://github.com/gdenes355/au-server.

## Scenes
Start with the `Welcome` scene which displays the join screen

The lobby is in the `Lobby` scene

The game (with the default and the beta map) are in `Game` and `Game2`.

## Getting started
1. Open the AmongElves folder in Unity (tested with 2019.4.16f1).
2. Navigate to the `Welcome` scene
3. Play

## Running against a local server
All network communication is handled by `AU_NetworkManagerWS`, which is a subclass of `INetworkManager`. Note that the old `AU_NetworkManagerXhttp` is no longer used.

`AU_NetworkManagerWS` specifies the URL to the WebSocket server (currently `wss://ws.gdenes.com`). Uncomment `ws://localhost:8765` to enable local debugging.

## External resource used:
* https://github.com/jirihybek/unity-websocket-webgl
* Bits and pieces of https://www.infogamerhub.com/i-made-among-us-in-one-day/, including assets and some client architecture