using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime.BuiltInScenarios;
using Object = UnityEngine.Object;

namespace Unity.Multiplayer.Tools.Tests.NetworkSimulator
{
    class RandomConnectionsSwapTests
    {
        Tools.NetworkSimulator.Runtime.NetworkSimulator m_NetworkSimulator;
        RandomConnectionsSwap m_ConnectionsSwitch;

        [SetUp]
        public void Setup()
        {
            m_ConnectionsSwitch = new();
        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(m_NetworkSimulator.gameObject);
        }

        [Test]
        public void InitializeRandomConnectionsSwap_WithoutConfigurations_DoesNotThrowExceptions()
        {
            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator(); // Initializing this in Setup fails to assign the scenario before Start is called.

            Assert.False(Equals(m_ConnectionsSwitch, m_NetworkSimulator.Scenario));
            ScenariosHelper.AssertScenarioInitialState(m_ConnectionsSwitch, false);
            Assert.DoesNotThrow(() =>
            {
                m_NetworkSimulator.Scenario = m_ConnectionsSwitch;      // Initialization happens here, synchronously
            });
            Assert.AreEqual(m_ConnectionsSwitch, m_NetworkSimulator.Scenario);
            ScenariosHelper.AssertScenarioInitialState(m_ConnectionsSwitch, true);
        }

        [Test]
        public void InitializeRandomConnectionsSwap_WithSingleConfiguration_DoesNotThrowExceptions()
        {
            m_ConnectionsSwitch.Configurations.Add(new()
            {
                ConnectionPreset = NetworkSimulatorPresets.None
            });
            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator(); // Initializing this in Setup fails to assign the scenario before Start is called.

            Assert.False(Equals(m_ConnectionsSwitch, m_NetworkSimulator.Scenario));
            ScenariosHelper.AssertScenarioInitialState(m_ConnectionsSwitch, false);
            Assert.DoesNotThrow(() =>
            {
                m_NetworkSimulator.Scenario = m_ConnectionsSwitch;      // Initialization happens here, synchronously
            });
            Assert.AreEqual(m_ConnectionsSwitch, m_NetworkSimulator.Scenario);
            ScenariosHelper.AssertScenarioInitialState(m_ConnectionsSwitch, true);
        }

        [Test]
        public async Task RunRandomConnectionsSwap_WithConfigurations_ChangePresets(
            [Values(0, 1, 2, 4)] int presetCount,
            [Values(0, 1, 2)] int repetitions)
        {
            const int changeIntervalMilliseconds = 100;

            var presets = ScenariosHelper.CreateDummyPresetArray(presetCount);
            var iterationsCount = presetCount * (1 + repetitions);

            m_ConnectionsSwitch.ChangeIntervalMilliseconds = changeIntervalMilliseconds;
            m_ConnectionsSwitch.Configurations.Clear();

            // Add Presets to Configurations
            foreach (var preset in presets)
            {
                m_ConnectionsSwitch.Configurations.Add(new()
                {
                    ConnectionPreset = preset
                });
            }

            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator(); // Initializing this in Setup fails to assign the scenario before Start is called.
            var propertyChangedCallbackIsCalled = false;
            m_NetworkSimulator.m_PropertyChanged += AssertPresetChange;
            m_NetworkSimulator.Scenario = m_ConnectionsSwitch;              // Initialization happens here, synchronously
            m_NetworkSimulator.Scenario.IsPaused = false;                   // The scenario will execute during the following frames

            var halfInterval = changeIntervalMilliseconds / 2;
            await Task.Delay(Math.Max(halfInterval, iterationsCount * changeIntervalMilliseconds - halfInterval));
            m_NetworkSimulator.m_PropertyChanged -= AssertPresetChange;

            void AssertPresetChange(object sender, PropertyChangedEventArgs args)
            {
                if (args.PropertyName != nameof(m_NetworkSimulator.ConnectionPreset))
                {
                    return;
                }

                var preset = m_NetworkSimulator.ConnectionPreset;

                DebugUtil.Trace($"Expected: {preset}, Actual items: {string.Join<NetworkSimulatorPreset>(',', presets)}");
                Assert.Contains(preset, presets);
                propertyChangedCallbackIsCalled = true;
            }

            if (presetCount > 0)
                Assert.True(propertyChangedCallbackIsCalled);
            else
                Assert.False(propertyChangedCallbackIsCalled);
        }

        [Test]
        public async Task RunRandomConnectionsSwap_RemovingConfigurations_DoesNotThrowExceptions(
            [Values(0, 4)] int presetCount,
            [Values(0, 1)] int repetitions,
            [Values(50, 100, 150)] int removeIntervalInMs)
        {
            var presets = ScenariosHelper.CreateDummyPresetArray(presetCount);
            var configurations = new Stack<RandomConnectionsSwap.Configuration>(presetCount);
            m_ConnectionsSwitch.Configurations.Clear();

            // Add Presets to Configurations
            foreach (var preset in presets)
            {
                configurations.Push(new()
                {
                    ConnectionPreset = preset,
                });
                m_ConnectionsSwitch.Configurations.Add(configurations.Peek());
            }

            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator(); // Initializing this in Setup fails to assign the scenario before Start is called.
            var propertyChangedCallbackIsCalled = false;
            m_NetworkSimulator.m_PropertyChanged += AssertPresetChange;
            m_NetworkSimulator.Scenario = m_ConnectionsSwitch;              // Initialization happens here, synchronously
            m_NetworkSimulator.Scenario.IsPaused = false;                   // The scenario will execute during the following frames

            await Task.Run(async () =>
            {
                while (configurations.Count > 0)
                {
                    await Task.Delay(removeIntervalInMs);
                    var configuration = configurations.Pop();
                    m_ConnectionsSwitch.Configurations.Remove(configuration);
                    DebugUtil.Trace($"Item removed: {configuration.ConnectionPreset.Name}, Left: {configurations.Count}");
                }
            });

            m_NetworkSimulator.m_PropertyChanged -= AssertPresetChange;

            void AssertPresetChange(object sender, PropertyChangedEventArgs args)
            {
                if (args.PropertyName != nameof(m_NetworkSimulator.ConnectionPreset))
                {
                    return;
                }

                if (configurations.Count == 0)
                {
                    return;
                }

                DebugUtil.Trace($"Expected Count: {configurations.Count}, actual count: {m_ConnectionsSwitch.Configurations.Count}." +
                    $"Expected Items: {string.Join(',', configurations.Select(c => c.ConnectionPreset))}," +
                    $"actual items: {string.Join(',', m_ConnectionsSwitch.Configurations.Select(c => c.ConnectionPreset))}");

                Assert.AreEqual(configurations.Count, m_ConnectionsSwitch.Configurations.Count);
                Assert.That(configurations, Is.EquivalentTo(m_ConnectionsSwitch.Configurations));
                propertyChangedCallbackIsCalled = true;
            }

            if (presetCount > 0)
                Assert.True(propertyChangedCallbackIsCalled);
            else
                Assert.False(propertyChangedCallbackIsCalled);
        }
    }
}
