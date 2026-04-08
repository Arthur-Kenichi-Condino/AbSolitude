using AKCondinoO.World.SimObjects;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal class WorldChunkSpawning:MonoBehaviour{
     internal WorldChunk chunk;
     internal ChunkSimObjectSpawner spawner;
        internal void OnAwake(){
         spawner=new(chunk,this);
        }
    }
}