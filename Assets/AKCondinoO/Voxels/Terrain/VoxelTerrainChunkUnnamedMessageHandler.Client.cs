#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelTerrainChunkUnnamedMessageHandler{
     [NonSerialized]int?clientSidecnkIdx=null;
        private void OnClientSideNetcnkIdxValueChanged(int previous,int current){
         if(Core.singleton.isClient){
          if(!IsOwner){
           if(clientSidecnkIdx==null||current!=clientSidecnkIdx.Value){
            clientSidecnkIdx=current;
           }
          }
         }
        }
    }
}