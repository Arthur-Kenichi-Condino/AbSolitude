using AKCondinoO.Bootstrap;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.Spawning{
    internal class BiomesSimObjectSpawningSystem{
        internal void OnChunkExists(Vector2Int cCoord){
        }
        internal class BiomesSimObjectSpawnJob:MultithreadedContainerJob{
         internal Vector2Int cCoord;
            public void OnScheduleSetContainerDataAtMainThread(){
            }
            public void ExecuteAtBackgroundThread(){
            }
            public void OnCompletedDoAtMainThread(){
            }
        }
    }
}