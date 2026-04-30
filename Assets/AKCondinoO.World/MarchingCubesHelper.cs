using UnityEngine;
using static AKCondinoO.World.BiomesConfigurationSnapshot;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.MarchingCubes{
    internal static class MarchingCubesHelper{
        internal static bool TryFindSurfaceTopDown(
         Vector3Int vCoord,Vector2Int cCoord,
         int startY,
         int bottomY,
         out Vector3 hitPoint,
         out Vector3 normal,
         float isoLevel=-50.0f
        ){
         var sampleInput=new SampleContext(vCoord,cCoord);
         var sampleDensityContext=new SampleDensityContext(sampleInput);
         double prevDensity=SampleDensity(ref sampleDensityContext,out _);
         //for(int y=startY-1;y>=bottomY;y--){
         // var coord=new Vector3Int(vCoord.x,y,vCoord.z);
         // double d=SampleDensity(coord,cCoord,);
         // //  Detecta crossing (iso surface)
         // if(prevDensity>isoLevel&&d<=isoLevel){
         //  normal=SampleNormal(coord);
         //  if(Vector3.Dot(normal,Vector3.up)<=0f){
         //   prevDensity=d;
         //   continue;
         //  }
         //  hitPoint=coord;
         //  return true;
         // }
         // prevDensity=d;
         //}
         hitPoint=default;
         normal=default;
         return false;
        }
    }
}