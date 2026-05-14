using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.Multiplayer.Tools.Adapters.Ngo1;
using Unity.Multiplayer.Tools.Adapters.Ngo1WithUtp2;
using Unity.Multiplayer.Tools.Adapters.Utp2;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Multiplayer.Tools.Adapters.Tests
{
    internal class AdapterInitializerTests
    {
        GameObject m_NetworkGameObject;

        /// <summary>
        /// In the event any tests run prior to this test, we want
        /// to remove any adapters before proceeding with these tests.
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var adapters = NetworkAdapters.Adapters.ToList();
            foreach (var adapter in adapters)
            {
                NetworkAdapters.RemoveAdapter(adapter);
            }
        }


        [SetUp]
        public void SetUp()
        {
            m_NetworkGameObject = new GameObject();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_NetworkGameObject);
        }

        [UnityTest]
        public IEnumerator Ngo1AdapterInitializer_RegistersAdapter()
        {
            Assert.IsFalse(NetworkAdapters.Adapters.Any(networkAdapter => networkAdapter is Ngo1Adapter), "Ngo1Adapter exists before test.");

            var ngo1AdapterAdded = false;

            NetworkAdapters.OnAdapterAdded += networkAdapter =>
            {
                if (networkAdapter is Ngo1Adapter)
                {
                    ngo1AdapterAdded = true;
                }
            };

            Ngo1AdapterInitializer.InitializeAdapter();
            InitAndStartNetworkManager();

            yield return null;

            Assert.IsTrue(ngo1AdapterAdded, "OnAdapterAdded was not called for Ngo1Adapter exists.");
            Assert.IsTrue(NetworkAdapters.Adapters.Any(networkAdapter => networkAdapter is Ngo1Adapter), "No Ngo1Adapter exists.");
        }

        [UnityTest]
        public IEnumerator Ngo1WithUtp2AdapterInitializer_RegistersAdapter()
        {
            Assert.IsEmpty(Ngo1WithUtp2AdapterInitializer.s_Adapters, "Expected no Ngo1WithUtp2Adapter exists before the test.");

            var utp2AdapterAdded = false;

            NetworkAdapters.OnAdapterAdded += networkAdapter =>
            {
                if (networkAdapter is Utp2Adapter)
                {
                    utp2AdapterAdded = true;
                }
            };

            Ngo1WithUtp2AdapterInitializer.InitializeAdapter();
            InitAndStartNetworkManager();

            yield return null;

            Assert.IsTrue(utp2AdapterAdded, "OnAdapterAdded was not called for Ngo1Adapter exists.");
            var adapter = Ngo1WithUtp2AdapterInitializer.s_Adapters.Values.FirstOrDefault();
            Assert.IsNotNull(adapter);
            Assert.IsInstanceOf<Utp2Adapter>(adapter);
        }

        void InitAndStartNetworkManager()
        {
            var networkManager = m_NetworkGameObject.AddComponent<NetworkManager>();
            var transport = m_NetworkGameObject.AddComponent<UnityTransport>();

            networkManager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport,
            };

            networkManager.StartHost();
        }
    }
}
