#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Terrain{
    internal partial class VoxelTerrainChunk{
     readonly List<Interaction>interactions=new List<Interaction>();
        public void SetInteractionsList(){
         interactions.Add(new GoHere(this));
        }
        public void GetInteractions(out List<Interaction>interactions){
         interactions=this.interactions;
        }
        public virtual string ContextName(){
         return"Voxel Terrain Chunk";
        }
    }
}