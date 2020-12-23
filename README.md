# SpotifyLib

a .NET Standard implementation of spotify.

Implements a Mercury client to communicate with the hm:// endpoints used by the spotify desktop client.
Also contains normal Rest endpoints to communicate with https endpoints.

hm is short hermes, a protocol used internally between servers at Spotify. It is basically zeromq with a protobuf envelope with some defined headers.

So, kind of like HTTP define verbs and structure on-top of TCP, Hermes define verbs and structure on-top of zeromq. It is used for HTTP-like Request/Response as well as Publish/Subscribe. For instance, a client request data about an album and waits for a response. Another example could be a client subscribing to events about a playlist. The moment someone publishes a change to the playlist, the client will know.

In one sense, it can do more than HTTP, but in another sense, it is much simpler because of the limited use. It was built many years ago, before HTTP/2 and grpc. It is still used heavily at Spotify.
(https://www.csc.kth.se/~gkreitz/spotifypubsub/spotifypubsub.pdf)
