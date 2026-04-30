using System.Linq;
using NUnit.Framework;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
using UnityEngine;

namespace Unity.Multiplayer.Tools.Tests.NetworkSimulator
{
    static class ScenariosHelper
    {
        public static readonly NetworkSimulatorPreset k_EmptyConfiguration = new();

        public static Tools.NetworkSimulator.Runtime.NetworkSimulator CreateNetworkSimulator()
        {
            return new GameObject().AddComponent<Tools.NetworkSimulator.Runtime.NetworkSimulator>();
        }

        public static NetworkSimulatorPreset[] CreateDummyPresetArray(int presetCount)
        {
            return Enumerable.Range(0, presetCount)
                .Select(i => new NetworkSimulatorPreset { Name = i.ToString() })
                .ToArray();
        }

        public static NetworkSimulatorPreset[] GetAllValidPresetsArray()
        {
            return NetworkSimulatorPresets.Values;
        }

        public static void AssertScenarioInitialState(NetworkScenario scenario, bool initialized)
        {
            if (initialized)
                Assert.True(scenario.IsInitialized);
            else
                Assert.False(scenario.IsInitialized);

            Assert.False(scenario.HasStarted);
            Assert.True(scenario.IsPaused);
        }
    }
}
