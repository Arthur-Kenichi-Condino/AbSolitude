// Uncomment the line below to log test details.
//#define UNITY_MP_TOOLS_DEBUG_TRACE

using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
using UnityEngine;

namespace Unity.Multiplayer.Tools.Tests.NetworkSimulator
{
    class NetworkScenarioTests
    {
        Tools.NetworkSimulator.Runtime.NetworkSimulator m_NetworkSimulator;

        [TearDown]
        public void Teardown()
        {
            if (m_NetworkSimulator != null)
            {
                Object.Destroy(m_NetworkSimulator.gameObject);
            }
        }

        [Test]
        public async Task NetworkScenarioBehavior_ChangeAtRuntime_StopsRunning()
        {
            var firstScenario = new MockScenarioBehaviour();
            var secondScenario = new MockScenarioBehaviour();

            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator(); // Initializing this in Setup fails to assign the scenario before Start is called.
            m_NetworkSimulator.AutoRunScenario = true;
            m_NetworkSimulator.Scenario = firstScenario;
            m_NetworkSimulator.Scenario.IsPaused = false;

            await Task.Yield();

            m_NetworkSimulator.Scenario = secondScenario;
            firstScenario.Reset();

            await Task.Yield();

            Assert.AreEqual(0, firstScenario.RanStartCount);
            Assert.AreNotEqual(0, secondScenario.RanUpdateCount);
        }

        [Test]
        public async Task NetworkScenarioTask_ChangeAtRuntime_StopsRunning()
        {
            var cancellationToken = new CancellationTokenSource();
            var firstScenario = new MockScenarioTask(cancellationToken);
            var secondScenario = new MockScenarioBehaviour();
            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator(); // Initializing this in Setup fails to assign the scenario before Start is called.
            m_NetworkSimulator.AutoRunScenario = true;
            m_NetworkSimulator.Scenario = firstScenario;
            m_NetworkSimulator.Scenario.IsPaused = false;

            await Task.Yield();

            m_NetworkSimulator.Scenario = secondScenario;
            firstScenario.Reset();

            await Task.Yield();

            cancellationToken.Cancel();

            Assert.AreEqual(0, firstScenario.RanCount);
        }

        [Test]
        public async Task NetworkScenario_ChangeAtRuntime_StartIsCalled()
        {
            var firstScenario = new MockScenarioBehaviour();
            var secondScenario = new MockScenarioBehaviour();
            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator(); // Initializing this in Setup fails to assign the scenario before Start is called.
            m_NetworkSimulator.AutoRunScenario = true;
            m_NetworkSimulator.Scenario = firstScenario;
            m_NetworkSimulator.Scenario.IsPaused = false;

            await Task.Yield();

            m_NetworkSimulator.Scenario = secondScenario;

            await Task.Yield();

            Assert.AreEqual(1, secondScenario.RanStartCount);
        }
    }

    class MockScenarioBehaviour : NetworkScenarioBehaviour
    {
        public int RanStartCount { get; private set; } = 0;

        public int RanUpdateCount { get; private set; } = 0;

        public override void Start(INetworkEventsApi networkEventsApi)
        {
            RanStartCount++;
        }

        protected override void Update(float deltaTime)
        {
            RanUpdateCount++;
        }

        public void Reset()
        {
            RanStartCount = 0;
            RanUpdateCount = 0;
        }
    }

    class MockScenarioTask : NetworkScenarioTask
    {
        private readonly CancellationTokenSource m_CancellationToken;

        public MockScenarioTask(CancellationTokenSource cancellationToken)
        {
            m_CancellationToken = cancellationToken;
        }

        public override void Dispose()
        {
            if (!m_CancellationToken.IsCancellationRequested)
            {
                m_CancellationToken.Cancel();
            }
        }

        public int RanCount { get; private set; } = 0;

        public int RanLoopCount { get; private set; } = 0;

        protected override async Task Run(INetworkEventsApi networkEventsApi, CancellationToken cancellationToken)
        {
            RanCount++;

            while (!m_CancellationToken.IsCancellationRequested)
            {
                await Task.Yield();

                RanLoopCount++;
            }
        }

        public void Reset()
        {
            RanCount = 0;
            RanLoopCount = 0;
        }
    }
}
