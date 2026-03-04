using AKCondinoO.Utilities;
using AKCondinoO.World.Voxels.Terrain;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal class WorldChunk:MonoBehaviour{
     internal Bounds bounds;
     private VoxelTerrainChunk voxelsTerrain;
        void Awake(){
         bounds=new(new(),new(Width,Height,Depth));
         voxelsTerrain=new(this);
        }
     private bool firstCall;
        internal void Initialize(){
         firstCall=true;
        }
     internal Vector2Int cCoord;
     internal Vector2Int cnkRgn;
        internal void OnEnsureExists(Vector2Int cCoord){
         if(firstCall||cCoord!=this.cCoord){
          this.cCoord=cCoord;
          this.cnkRgn=cCoordTocnkRgn(this.cCoord);
          voxelsTerrain.DoMarchingCubes();
         }
        }
        void OnDrawGizmos(){
         #if UNITY_EDITOR
          DrawGizmos.Bounds(bounds,Color.gray);
         #endif
        }
    }
}