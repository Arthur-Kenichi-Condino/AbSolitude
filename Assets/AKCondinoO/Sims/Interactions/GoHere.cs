#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using AKCondinoO.Voxels.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class GoHere:Interaction{
     internal VoxelTerrainChunk terrainChunk;
        internal GoHere(VoxelTerrainChunk chunk){
         terrainChunk=chunk;
        }
        internal override void Do(BaseAI sim){
        }
        public override string ToString(){
         return"Ir Até...";
        }
    }
}