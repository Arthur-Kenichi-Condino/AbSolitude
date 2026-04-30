// Uncomment the line below to log test details.
// #define UNITY_MP_TOOLS_DEBUG_TRACE

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
    class ConnectionsCycleTests
    {
        Tools.NetworkSimulator.Runtime.NetworkSimulator m_NetworkSimulator;
        ConnectionsCycle m_ConnectionsCycle;

        [SetUp]
        public void Setup()
        {
            m_ConnectionsCycle = new();
        }

        [TearDown]
        public void Teardown()
        {
            if (m_NetworkSimulator != null)
            {
                Object.Destroy(m_NetworkSimulator.gameObject);
            }
        }

        [Test]
        public void InitializeConnectionsCycle_WithoutConfigurations_DoesNotThrowExceptions()
        {
            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator(); // Initializing this in Setup fails to assign the scenario before Start is called.

            Assert.False(Equals(m_ConnectionsCycle, m_NetworkSimulator.Scenario));
            ScenariosHelper.AssertScenarioInitialState(m_ConnectionsCycle, false);
            Assert.DoesNotThrow(() =>
            {
                m_NetworkSimulator.Scenario = m_ConnectionsCycle;       // Initialization happens here, synchronously
            });
            Assert.AreEqual(m_ConnectionsCycle, m_NetworkSimulator.Scenario);
            ScenariosHelper.AssertScenarioInitialState(m_ConnectionsCycle, true);
        }

        [Test]
        public void InitializeConnectionsCycle_WithSingleConfiguration_DoesNotThrowExceptions()
        {
            m_ConnectionsCycle.Configurations.Add(new()
            {
                ConnectionPreset = NetworkSimulatorPresets.None
            });
            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator(); // Initializing this in Setup fails to assign the scenario before Start is called.

            Assert.False(Equals(m_ConnectionsCycle, m_NetworkSimulator.Scenario));
            ScenariosHelper.AssertScenarioInitialState(m_ConnectionsCycle, false);
            Assert.DoesNotThrow(() =>
            {
                m_NetworkSimulator.Scenario = m_ConnectionsCycle;       // Initialization happens here, synchronously
            });
            Assert.AreEqual(m_ConnectionsCycle, m_NetworkSimulator.Scenario);
            ScenariosHelper.AssertScenarioInitialState(m_ConnectionsCycle, true);
        }

        [Test]
        public async Task RunConnectionsCycle_WithConfigurations_ChangePresets(
            [Values(0, 1, 2, 4)] int presetCount,
            [Values(0, 1, 2)] int repetitions)
        {
            const int changeIntervalMilliseconds = 100;

            var presets = ScenariosHelper.CreateDummyPresetArray(presetCount);
            var expectedPresets = new Queue<INetworkSimulatorPreset>(Enumerable.Repeat(presets, repetitions).SelectMany(array => array));
            var iterationsCount = presetCount * (1 + repetitions);

            m_ConnectionsCycle.Configurations.Clear();

            // Add Presets to Configurations
            foreach (var preset in presets)
            {
                m_ConnectionsCycle.Configurations.Add(new()
                {
                    ConnectionPreset = preset,
                    ChangeIntervalMilliseconds = changeIntervalMilliseconds
                });
            }

            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator(); // Initializing this in Setup fails to assign the scenario before Start is called.
            var propertyChangedCallbackIsCalled = false;
            m_NetworkSimulator.m_PropertyChanged += AssertPresetChange;
            m_NetworkSimulator.Scenario = m_ConnectionsCycle;               // Initialization happens here, synchronously
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

                if (expectedPresets.Count == 0)
                {
                    return;
                }

                var preset = expectedPresets.Dequeue();

                DebugUtil.Trace($"Expected: {preset.Name}, actual: {m_NetworkSimulator.ConnectionPreset?.Name}," +
                    $"expected presets count: {expectedPresets.Count}");

                Assert.AreEqual(preset, m_NetworkSimulator.ConnectionPreset);
                propertyChangedCallbackIsCalled = true;
            }

            if (presetCount > 0 && repetitions > 0)
                Assert.True(propertyChangedCallbackIsCalled);
            else
                Assert.False(propertyChangedCallbackIsCalled);
        }

        [Test]
        public async Task RunConnectionsCycle_RemovingConfigurations_DoesNotThrowExceptions(
            [Values(0, 4)] int presetCount,
            [Values(0, 1)] int repetitions,
            [Values(50, 100, 150)] int removeIntervalInMs)
        {
            const int changeIntervalMilliseconds = 100;

            var presets = ScenariosHelper.CreateDummyPresetArray(presetCount);
            var configurations = new Stack<ConnectionsCycle.Configuration>(presetCount);
            m_ConnectionsCycle.Configurations.Clear();

            // Add Presets to Configurations
            foreach (var preset in presets)
            {
                configurations.Push(new()
                {
                    ConnectionPreset = preset,
                    ChangeIntervalMilliseconds = changeIntervalMilliseconds
                });
                m_ConnectionsCycle.Configurations.Add(configurations.Peek());
            }

            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator(); // Initializing this in Setup fails to assign the scenario before Start is called.
            var propertyChangedCallbackIsCalled = false;
            m_NetworkSimulator.m_PropertyChanged += AssertPresetChange;
            m_NetworkSimulator.Scenario = m_ConnectionsCycle;               // Initialization happens here, synchronously
            m_NetworkSimulator.Scenario.IsPaused = false;                   // The scenario will execute during the following frames

            await Task.Run(async () =>
            {
                while (configurations.Count > 0)
                {
                    await Task.Delay(removeIntervalInMs);
                    var configuration = configurations.Pop();
                    m_ConnectionsCycle.Configurations.Remove(configuration);
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

                DebugUtil.Trace($"Expected Count: {configurations.Count}, actual count: {m_ConnectionsCycle.Configurations.Count}." +
                    $"Expected Items: {string.Join(',', configurations.Select(c => c.ConnectionPreset))}," +
                    $"actual items: {string.Join(',', m_ConnectionsCycle.Configurations.Select(c => c.ConnectionPreset))}");

                Assert.AreEqual(configurations.Count, m_ConnectionsCycle.Configurations.Count);
                Assert.That(configurations, Is.EquivalentTo(m_ConnectionsCycle.Configurations));
                propertyChangedCallbackIsCalled = true;
            }

            if (presetCount > 0)
                Assert.True(propertyChangedCallbackIsCalled);
            else
                Assert.False(propertyChangedCallbackIsCalled);
        }
    }
}
