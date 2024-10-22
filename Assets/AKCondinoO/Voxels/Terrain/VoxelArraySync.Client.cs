using System;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelArraySync{
        internal partial class ClientData{
         [NonSerialized]internal bool clientSideVoxelsChangesReceived;
            internal void OnClientSideVoxelsValueChanged(NetVoxelArrayContainer previous,NetVoxelArrayContainer current){
             if(Core.singleton.isClient){
              if(current!=null){
               Log.DebugMessage("'clientSideVoxelsChangesReceived'",netVoxelArray);
               clientSideVoxelsChangesReceived=true;
               Log.DebugMessage("OnClientSideVoxelsValueChanged:current.cnkIdx:"+current.cnkIdx+";current.segment:"+current.segment);
              }
             }
            }
        }
    }
}