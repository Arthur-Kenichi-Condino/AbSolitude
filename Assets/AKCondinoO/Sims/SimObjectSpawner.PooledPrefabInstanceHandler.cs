using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObjectSpawner{
        internal class PooledPrefabInstanceHandler:INetworkPrefabInstanceHandler{
         GameObject prefab;
         internal readonly Queue<NetworkObject>pool=new Queue<NetworkObject>();
            internal PooledPrefabInstanceHandler(GameObject prefab){
             this.prefab=prefab;
            }
            //  client side only
            NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId,Vector3 position,Quaternion rotation){
             if(!pool.TryDequeue(out NetworkObject netObj)){
              netObj=Instantiate(prefab).GetComponent<NetworkObject>();
             }
             SimObject simObject=netObj.gameObject.GetComponent<SimObject>();
                       simObject.clientSidePooling=pool;
             //  set transform
             netObj.transform.position=position;
             netObj.transform.rotation=rotation;
             return netObj;
            }
            //  client and server
            void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject){
             if(Core.singleton.isClient){
              pool.Enqueue(networkObject);
             }
            }
        }
    }
}