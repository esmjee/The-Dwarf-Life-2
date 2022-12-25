using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Netcode.TestHelpers.Runtime;

namespace Unity.Netcode.RuntimeTests
{
    public class NetworkListChangedTestComponent : NetworkBehaviour
    {

    }

    public class ListChangedObject : NetworkBehaviour
    {
        public int ExpectedPreviousValue = 0;
        public int ExpectedValue = 0;
        public bool AddDone = false;

        public NetworkList<int> MyNetworkList = new NetworkList<int>();

        public override void OnNetworkSpawn()
        {
            MyNetworkList.OnListChanged += Changed;
            base.OnNetworkSpawn();
        }

        public void Changed(NetworkListEvent<int> listEvent)
        {
            if (listEvent.Type == NetworkListEvent<int>.EventType.Value)
            {
                if (listEvent.PreviousValue != ExpectedPreviousValue)
                {
                    Debug.Log($"Expected previous value mismatch {listEvent.PreviousValue} versus {ExpectedPreviousValue}");
                    Debug.Assert(listEvent.PreviousValue == ExpectedPreviousValue);
                }

                if (listEvent.Value != ExpectedValue)
                {
                    Debug.Log($"Expected value mismatch {listEvent.Value} versus {ExpectedValue}");
                    Debug.Assert(listEvent.Value == ExpectedValue);
                }

                AddDone = true;
            }
        }
    }

    public class NetworkListChangedTests : NetcodeIntegrationTest
    {
        protected override int NumberOfClients => 2;

        private ulong m_ClientId0;
        private GameObject m_PrefabToSpawn;

        private NetworkObject m_NetSpawnedObject1;

        protected override void OnServerAndClientsCreated()
        {
            m_PrefabToSpawn = CreateNetworkObjectPrefab("ListChangedObject");
            m_PrefabToSpawn.AddComponent<ListChangedObject>();
        }

        [UnityTest]
        public IEnumerator NetworkListChangedTest()
        {
            m_ClientId0 = m_ClientNetworkManagers[0].LocalClientId;

            // create 3 objects
            var spawnedObject1 = SpawnObject(m_PrefabToSpawn, m_ServerNetworkManager);
            m_NetSpawnedObject1 = spawnedObject1.GetComponent<NetworkObject>();

            m_NetSpawnedObject1.GetComponent<ListChangedObject>().MyNetworkList.Add(42);
            m_NetSpawnedObject1.GetComponent<ListChangedObject>().ExpectedPreviousValue = 42;
            m_NetSpawnedObject1.GetComponent<ListChangedObject>().ExpectedValue = 44;
            m_NetSpawnedObject1.GetComponent<ListChangedObject>().MyNetworkList[0] = 44;

            Debug.Assert(m_NetSpawnedObject1.GetComponent<ListChangedObject>().AddDone);

            return null;
        }
    }
}
