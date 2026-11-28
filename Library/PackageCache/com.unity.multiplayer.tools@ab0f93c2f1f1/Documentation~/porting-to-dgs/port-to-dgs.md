# Porting from client-hosted to Dedicated Game Server (DGS)

Learn how to port your game from a client-hosted architecture to a Dedicated Game Server (DGS) architecture. This section provides guidance on the differences between client-hosted and DGS, the changes you need to make in your game, and considerations for hosting.

You might have started developing your game with client-hosted in mind but then realized it wasn’t giving you the performance, reliability, or security you wanted. There are multiple reasons for choosing both a dedicated game server (DGS) solution and a client-hosted solution. This section provides guidance around switching from a client-hosted game to a dedicated-server game in Unity using [Netcode for GameObjects (NGO)](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@latest?subfolder=/manual/index.html).

There are two distinct meanings of the word “host” that you must take care not to confuse: the NGO host and the hardware host.

- The **NGO host** is where both a client and a server run simultaneously. The hosting provider (the hardware host) runs your Unity server build.
- The **hardware host** (virtual or bare-metal) runs your Unity server build in a data center or hosting provider.

| **Topic**                       | **Description**                  |
| :------------------------------ | :------------------------------- |
| **[Client-hosted vs DGS-hosted](client-vs-dgs.md)** | Understand the key differences between client-hosted and DGS architectures, and learn how to adapt your game accordingly. |
| **[Game changes](game-changes.md)** | A list of changes you need to make in your game when porting to DGS. |
| **[Optimizing server builds](optimizing-server-builds.md)** | Tips and tricks for optimizing your server builds to improve performance and reduce resource usage. |
| **[Hosting considerations](hosting-considerations.md)**    | Important considerations for hosting your DGS, including server configuration, network settings, and deployment strategies. |