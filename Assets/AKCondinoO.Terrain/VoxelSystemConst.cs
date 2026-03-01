using UnityEngine;
namespace AKCondinoO.Terrain{
    internal static class VoxelSystemConst{
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
    }
}