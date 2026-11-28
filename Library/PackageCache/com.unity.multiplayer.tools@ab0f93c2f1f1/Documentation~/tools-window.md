# Multiplayer Tools window reference

Explore the tools available in the Multiplayer Tools window to visualize, simulate, profile, and debug the network activity in your multiplayer application.

The **Multiplayer Tools** window gives you access to all the multiplayer tools and their documentation in one place. It also lists the installed version of each package that the tools depend on.

To open the window, go to **Window** > **Multiplayer** > **Multiplayer Tools**.

If a tool is unavailable because of a missing dependency, the window displays a message that explains which packages the tool requires. Select **Open Package Manager** to open the **Package Manager** window and install the required package.

## Multiplayer Tools window

The **Multiplayer Tools** window contains the following tools.

| **Tool** | **Description** |
| -- | -- |
| **Network Scene Visualization** | Overlays network information, such as ownership and bandwidth, on GameObjects in the **Scene** view. Select **Open** to display the **Network Visualization** overlay. For more information, refer to [Network Scene Visualization](network-scene-vis.md). |
| **Runtime Network Stats Monitor** | Displays network statistics in an on-screen display at runtime. Select **Add to Scene** to add a GameObject named `Runtime Network Stats Monitor` to the current scene. For more information, refer to [Runtime Network Stats Monitor](runtime-stats-monitor.md). |
| **Network Simulator** | Simulates adverse network conditions, both in the Unity Editor and in a build. Select **Add to Scene** to add a GameObject named `Network Simulator` to the current scene. For more information, refer to [Network Simulator](network-simulator.md). |
| **Network Profiler** | Measures the bandwidth your application uses, in the Unity Profiler. Select **Open** to open the **Profiler** window. For more information, refer to [Network Profiler](profiler.md). |
| **Hierarchy Network Debug View** | Displays network ownership information in the **Hierarchy** window. Select **Enable** to turn the debug view on, and **Disable** to turn it off. For more information, refer to [Hierarchy Network Debug View](hierarchy-network-debug.md). |

> [!NOTE]
> Because of restrictions in the Profiler API, Unity can't load the Network Profiler modules automatically. After you select **Open**, add the **NGO Messages** and **NGO Objects** modules in the **Profiler** window manually.

## Additional resources

- [Multiplayer Tools package](index.md)
- [Install the Multiplayer Tools package](install-tools.md)
- [Unity Profiler](https://docs.unity3d.com/Manual/Profiler.html)
- [Netcode for GameObjects](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@latest/)
