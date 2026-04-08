using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.SimObjects{
    internal class ChunkSimObjectSpawner{
     private readonly WorldChunk chunk;
     private readonly WorldChunkSpawning spawning;
        internal ChunkSimObjectSpawner(WorldChunk chunk,WorldChunkSpawning spawning){
         this.chunk=chunk;this.spawning=spawning;
        }
    }
}