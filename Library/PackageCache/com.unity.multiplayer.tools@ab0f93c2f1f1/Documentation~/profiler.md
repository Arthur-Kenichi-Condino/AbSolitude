# Network Profiler

Use the Network Profiler to measure the performance of your multiplayer application while it's running.

The Network Profiler extends the [Unity Profiler](https://docs.unity3d.com/Manual/Profiler.html) in the Unity Editor. The Unity Profiler measures CPU, GPU, and memory usage in your application, and the Network Profiler adds network traffic profiling for applications built with [Netcode for GameObjects](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@latest/).

In multiplayer development, you need to manage three primary resources:

- Bandwidth consumption
- CPU usage
- Latency

These three resources are interdependent, so a change that improves one usually costs you another. For example, compression algorithms and heavier serialization strategies reduce bandwidth consumption, but they increase CPU usage and latency. Lighter serialization strategies reduce CPU usage and latency, but they increase bandwidth consumption.

> [!NOTE]
> The latency referred to here is the time taken to send a packet, not overall network latency, which is a function of many other factors.

## Prerequisites

To enable profiling support for Netcode for GameObjects, [install the Multiplayer Tools package](install-tools.md). The package adds two modules to the Unity Profiler:

- **NGO Messages**: Network messages sent to the transport.
- **NGO Objects**: Activity related to, and grouped by, NetworkObject instances.

## NGO Messages module

The **NGO Messages** module contains the following counters.

| **Counter** | **Description** |
| -- | -- |
| **Total Bytes Sent** | Total number of bytes sent during this frame. |
| **Total Bytes Received** | Total number of bytes received during this frame. |
| **Named Message Bytes Sent** | Total number of named message bytes sent to any connection in this frame. |
| **Named Message Bytes Received** | Total number of named message bytes received in this frame. |
| **Unnamed Message Bytes Sent** | Total number of unnamed message bytes sent to any connection in this frame. |
| **Unnamed Message Bytes Received** | Total number of unnamed message bytes received in this frame. |
| **Scene Event Bytes Sent** | Total number of scene event bytes sent during this frame. |
| **Scene Event Bytes Received** | Total number of scene event bytes received during this frame. |

> [!NOTE]
> Messages sent to the local client (for example, a server in host mode sending RPCs to itself) appear grayed out in the Profiler to indicate that they might not be sent over the network.

## NGO Objects module

The **NGO Objects** module contains the following counters.

| **Counter** | **Description** |
| -- | -- |
| **Rpc Bytes Sent** | Total number of bytes sent for remote procedure calls (RPCs) during this frame. |
| **Rpc Bytes Received** | Total number of bytes received for remote procedure calls (RPCs) during this frame. |
| **Network Variable Bytes Sent** | Total number of bytes sent for network variable updates during this frame. |
| **Network Variable Bytes Received** | Total number of bytes received for network variable updates during this frame. |
| **Object Spawned Bytes Sent** | Total number of bytes sent for object spawn messages during this frame. |
| **Object Spawned Bytes Received** | Total number of bytes received for object spawn messages during this frame. |
| **Object Destroyed Bytes Sent** | Total number of bytes sent for object destroy messages during this frame. |
| **Object Destroyed Bytes Received** | Total number of bytes received for object destroy messages during this frame. |
| **Ownership Change Bytes Sent** | Total number of bytes sent for ownership change messages during this frame. |
| **Ownership Change Bytes Received** | Total number of bytes received for ownership change messages during this frame. |

## Best practices

The Network Profiler reports a large amount of data, which can make it hard to choose an optimization strategy. Use the following best practices to make your optimization process easier.

### Filter results

You can use the following filter rules to narrow your search results:

| **Rule** | **Description** |
| -- | -- |
| `t:<type>` | Filters by the value in the type column. For example, `t:RPC`. |
| `dir:in` | Filters to received bytes only. |
| `dir:out` | Filters to sent bytes only. |
| `b<N` | Filters to events with fewer than `N` bytes. |
| `b<=N` | Filters to events with `N` bytes or fewer. |
| `b>N` | Filters to events with more than `N` bytes. |
| `b>=N` | Filters to events with `N` bytes or more. |
| `b==N` | Filters to events with exactly `N` bytes. |
| `b!=N` | Filters to events that don't have `N` bytes. |

You can combine these rules to continue narrowing your results.

To show a NetworkObject in the **Hierarchy** window, double-click it.

### Hide unnecessary content

You can hide profiler data to focus only on the information you're interested in.

To show only the network profiler modules, in the **Profiler** window open **Profiler Modules** and enable **NGO Messages** and **NGO Objects**.

### Profile a development build

For the most accurate results, profile your application as a development build on the target hardware. For information about profiling a development build on Windows, macOS, Linux, WebGL, iOS, or Android, refer to [Profile your application on a target platform](https://docs.unity3d.com/Manual/profiling-target-device.html).

### Profile in the Unity Editor

To get a broad sense of performance quickly rather than high accuracy, [profile in Play mode in the Editor](https://docs.unity3d.com/Manual/profiling-play-mode.html). For example, you can activate and deactivate multiple GameObjects at runtime to find the root of a performance problem.

### Iterate your optimization

Profile your application every time you add a feature or refactor an existing one. Comparing profiles before and after each change shows you the performance impact of that change.

There are no definite rules with performance optimization. Profile your application regularly, investigate the nature of your optimization problems, experiment with different solutions, and measure the results of your changes.
