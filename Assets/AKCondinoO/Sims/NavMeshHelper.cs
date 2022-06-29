#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.Sims{
    internal class NavMeshHelper{   
     internal static int navMeshLayer;
     internal static NavMeshBuildSettings[]navMeshBuildSettings;
        internal static void SetNavMeshBuildSettings(){
         navMeshLayer=1<<LayerMask.NameToLayer("VoxelTerrain");
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
                   tileSize=64,
           debug=new NavMeshBuildDebugSettings{
            flags=NavMeshBuildDebugFlags.None,
           },
           maxJobWorkers=2,
          },
          new NavMeshBuildSettings{
           agentTypeID=GetAgentTypeIDByName("SmallSize").Value,
           agentRadius=0.25f,
           agentHeight=0.755f,
           agentClimb=0.75f,
           agentSlope=60f,
           overrideVoxelSize=true,
                   voxelSize=0.25f/3f,
           minRegionArea=0.25f,
           overrideTileSize=true,
                   tileSize=64,
           debug=new NavMeshBuildDebugSettings{
            flags=NavMeshBuildDebugFlags.None,
           },
           maxJobWorkers=2,
          },
          new NavMeshBuildSettings{
           agentTypeID=GetAgentTypeIDByName("LargeSize").Value,
           agentRadius=0.5625f,
           agentHeight=2.5f,
           agentClimb=1.75f,
           agentSlope=60f,
           overrideVoxelSize=true,
                   voxelSize=0.5625f/3f,
           minRegionArea=0.5625f,
           overrideTileSize=true,
                   tileSize=64,
           debug=new NavMeshBuildDebugSettings{
            flags=NavMeshBuildDebugFlags.None,
           },
           maxJobWorkers=2,
          },
         };
        }
        //  [https://answers.unity.com/questions/1650130/change-agenttype-at-runtime.html]
        internal static int?GetAgentTypeIDByName(string agentTypeName){
         int count=NavMesh.GetSettingsCount();
         for(int i=0;i<count;++i){
          int id=NavMesh.GetSettingsByIndex(i).agentTypeID;
          string name=NavMesh.GetSettingsNameFromID(id);
          if(name==agentTypeName){
           return id;
          }
         }
         return null;
        }
    }
}