# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.2.1] - 2024-09-27

### *Network Scene Visualization*
- Fixed a bug which showed the wrong number of clients in the SceneView overlay
- Fixed a bug which caused null reference exception when entering Play Mode without Domain Reload
- Fixed a bug which showed wrong ownership visuals with Distributed Authority when reconnecting

### *Runtime Network Stats Monitor*
- Fixed a bug where RNSM showed no metrics in builds with code stripping enabled
- Fixed a bug where RNSM stopped working with NGO 2.0.0 or above

## [2.2.0] - 2024-07-24

### *General*
- Changed Window Menu location to Window/Multiplayer
- Activated the Hierarchy Network Debug View (Hierarchy overlay indicating NetworkObjects and ownership)

### *Network Scene Visualization*
- Fixed an issue where the console was spammed with errors when disconnecting from NGO.

### *Network Simulator*
- Fixed NullRef in Sample Scenario ConnectionParametersWithCurves

### *Runtime Net Stats Monitor*
- Fixed an issue throwing UI Layout errors when changing RNSM position

## [2.1.0] - 2023-11-21

### *General*
- Increased minimum version to 2023.3+

### *Network Simulator*
- Fixed an issue with execution order dependend initialization of NetworkAdapters.

### *Network Scene Visualization*
- Improved usability when pausing the editor.

## [2.0.0-pre.5] - 2023-09-19

### *General*
- *New*: Find all the multiplayer tools in a central place in the multiplayer tools window.

### *Network Profiler*
- Fixed an issue causing inconsistent foldout behavior.
- Fixed a bug where switching frames would reset the sorting filter.
- Improved UI for the Network Profiler.

### *Runtime Net Stats Monitor*
- Fix an index out of range exception that could occur when adding a new RNSM graph at runtime.
- Renamed the Component in the "Add Component" menu from "RuntimeNetStatsMonitor" to "Runtime Network Stats Monitor".

### *Network Simulator*
- Added presets for common home broadband connections and situations.

### *Network Scene Visualization*
- Text Overlays now also work in 2D scenes with 2D colliders.
- Removed slight delay between bandwidth text and color change.
- Made bandwidth update instantly after switching back from Ownership mode

## [2.0.0-pre.3] - 2023-05-23

### *General*
- Remove unintentionally public classes (such as test classes), most of which were already deprecated.

### *Network Scene Visualization*
- Fix to prevent NGO from throwing a NotServerException when visualizing ownership on a client that is not connected as the server or host.

## [2.0.0-pre.2] - 2023-05-01

### *General*
- Dropped support for Unity 2020.3; the next supported version is Unity 2021.3
- Fixed ``Failed to load type initialization for assembly Unity.Multiplayer.Tools.MetricTypes`` runtime exception when building using Managed Stripping level set to high.

### *Network Scene Visualization*

This release adds the Network Scene Visualization to the Multiplayer Tools Package. This tool allows users to visualize networking information (like bandwidth and ownership) on a per-object basis in the scene view using a number of visualizations, including mesh shading and a text overlay.

### *Network Simulator*
- Fixed an `ObjectDisposedException` happening after disconnecting and reconnecting while using the Network Simulator.
- Fixed duplicated `NetworkSimulatorPresets`.

### *Runtime Net Stats Monitor*
- Fixed an issue that prevented using the ``RuntimeNetStatsMonitor.AddCustomValue`` API for stats that are only sampled per second.
- Switched to a new color-blind friendly color-palette for default variable colors in graphs, which will provide increased contrast and more default values. This new color palette is the same one used in the new Network Scene Visualization tool.
- Reduced the maximum sample count in Graphs and Simple Moving Average counters from 4096 to 512. Sample counts higher than 512 are no longer needed since per-second sampling was introduced in 1.1.0.
- Deprecated public methods that could be used to control the conditional compilation of the RNSM. Conditional compilation of the RNSM will be removed in a future major release.

## [1.1.0] - 2022-09-22

### *Metrics*
- Improve the warning message for throttling, and increase the threshold for throttling a metric from 100 to 1,000 recorded events per frame.

### *Misc*
- Fixed compilation warning related to unsupported build targets

### *Runtime Net Stats Monitor*
- Graphs and Simple Moving Average counters can now be configured to be sampled per-second rather than per-frame
- Fixed an issue where RNSM line graphs could retain incorrect maximum values in some cases

## [1.0.0] - 2022-06-27

### *Runtime Net Stats Monitor*
- Doc-comment fixes based on 1.0 release XML doc validation 

## [1.0.0-pre.8] - 2022-06-15

### *Runtime Net Stats Monitor*
- Clamping numerical values to acceptable limits for public APIs
- Improve generated counter labels
- Prevent an exception when there's only one sample
- Added spacing between divider graph and axis number alignment
- Reusing existing numerical labels when the value doesn't change or barely changed
- Fix incorrect values for gauges in counter display elements using SMA
- Ensure RNSM counters display 1 rather than 1,000 milli
- Use infinity rather than float.Min for counter config bounds
- Reduce vertices in graphs with large sample count

## [1.0.0-pre.7] - 2022-04-27

### *Runtime Net Stats Monitor*

This release adds the Runtime Net Stats Monitor (RNSM) to the Multiplayer Tools Package. This tool offers a configurable component for displaying various network stats in an on-screen display at runtime, including stats from the Netcode for GameObjects package, as well as custom, user-defined stats.

For more information about the Runtime Net Stats Monitor, please see its documentation.

## [1.0.0-pre.6] - 2022-02-28

### *Network Profiler*
- Changed NetworkMessage to use the name of the message in the Type column

### *Metrics*
- Added throttling to event metric types
- Added a system to generate random data for tests
- Refactored underlying data structures to reduce redundancy
- Dramatically reduced runtime allocations caused by dispatching metrics to the profiler by updating the serialization implementation to use native buffers instead of BinaryFormatter
- Deprecated support for String when collecting metric payloads
- Added RTT to server metrics
- Added Packet count to metrics

### *Misc*
- Updated some internals exposed flags to enable some test improvements on NGO side

## [1.0.0-pre.2] - 2021-10-19

- Updated documentation files in preparation for publish

## [1.0.0-pre.1] - 2021-08-18

### *Netcode Profiler*

This release adds the Netcode Profiler to the Multiplayer Tools Package. This tool is used to inspect the network activity of **Netcode for GameObjects**.

#### Activity Section
- View detailed metrics about custom messages, scene transitions, and server logs
- View activity related to individual game objects, including network variable updates, rpcs, spawn and destroy events, and ownership changes

#### Messages Section
- View the raw messages that are being sent to the transport interface

#### Other Usability
- Connect to built players to inspect netcode activity remotely
- Filter results by name, type, number of bytes, and network direction
- Correlate network objects in the profiler with objects in the scene hierarchy
- View key metrics in graph form in the profiler charts
