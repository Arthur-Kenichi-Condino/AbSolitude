#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.Sims{
    internal class NavMeshHelper{   
     internal static NavMeshBuildSettings[]navMeshBuildSettings;
        internal static void SetNavMeshBuildSettings(){
         navMeshBuildSettings=new NavMeshBuildSettings[]{
          new NavMeshBuildSettings{
           agentTypeID=GetAgentTypeIDByName("MediumSize").Value,
           agentRadius=0.28125f,
           agentHeight=1.75f,
           agentClimb=0.75f,
           agentSlope=60f,
           overrideVoxelSize=true,
                   voxelSize=0.28125f/3f,
           minRegionArea=0.28125f,
           overrideTileSize=true,
                   tileSize=16,
          },
         };
        }
        //  [https://answers.unity.com/questions/1650130/change-agenttype-at-runtime.html]
        internal static int?GetAgentTypeIDByName(string agentTypeName){
         return null;
        }
    }
}