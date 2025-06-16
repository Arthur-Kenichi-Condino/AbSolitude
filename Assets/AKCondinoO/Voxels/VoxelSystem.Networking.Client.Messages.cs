#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using AKCondinoO.Networking;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels{
    internal partial class VoxelSystem{
        internal partial class ClientData{
            private void OnClientReceivedUnnamedMessage(ulong clientId,FastBufferReader reader){
             var messageType=(int)UnnamedMessageTypes.Undefined;
             reader.ReadValueSafe(out messageType);
            }
        }
    }
}