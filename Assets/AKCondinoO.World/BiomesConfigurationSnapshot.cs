using AKCondinoO.World.MarchingCubes;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal static class BiomesConfigurationSnapshot{
        internal static void Setvxl(ref Voxel vxl,Vector3Int vCoord,Vector2Int cCoord){
         ValidatevCoord(ref cCoord,ref vCoord);
         /*  fora do mundo, baixo:  */
         if(vCoord.y<=0){
          vxl=Voxel.bedrock;
          return;
         /*  fora do mundo, cima:  */
         }else if(vCoord.y>=Height){
          vxl=Voxel.air;
          return;
         }
         vxl=Voxel.air;
        }
    }
}