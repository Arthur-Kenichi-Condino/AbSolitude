using UnityEngine;
namespace AKCondinoO.World{
    internal static class WorldChunkManagerConst{
     internal const int MaxcCoordx=312;
     internal const int MaxcCoordy=312;
     internal const ushort Height=(256);
     internal const ushort Width=(16);
     internal const ushort Depth=(16);
     internal const ushort FlattenOffset=(Width*Depth);
     internal const int VoxelsPerChunk=(FlattenOffset*Height);
        internal static Vector2Int vecPosTocCoord(Vector3 pos){
         pos.x/=(float)Width;
         pos.z/=(float)Depth;
         return new Vector2Int(
          Mathf.FloorToInt(pos.x),
          Mathf.FloorToInt(pos.z)
         );
        }
        internal static Vector3Int vecPosTovCoord(Vector3 pos){
         Vector3Int coord=new Vector3Int(
          Mathf.FloorToInt((pos.x%Width +Width )%Width ),
          Mathf.FloorToInt((pos.y%Height+Height)%Height),
          Mathf.FloorToInt((pos.z%Depth +Depth )%Depth )
         );
         return coord;
        }
        internal static Vector2Int cCoordTocnkRgn(Vector2Int cCoord){
         return new Vector2Int(cCoord.x*Width,cCoord.y*Depth);
        }
        internal static void ValidatevCoord(ref Vector2Int cCoord,ref Vector3Int vxlCoord){
         Vector2Int relativecCoord=vecPosTocCoord(vxlCoord);
         cCoord+=relativecCoord;
         vxlCoord=vecPosTovCoord(vxlCoord);
        }
    }
}