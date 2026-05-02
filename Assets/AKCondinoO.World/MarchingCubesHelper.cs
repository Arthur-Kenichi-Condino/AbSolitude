using AKCondinoO.Bootstrap;
using UnityEngine;
using static AKCondinoO.World.BiomesConfigurationSnapshot;
using static AKCondinoO.World.MarchingCubes.MarchingCubesCore;
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
         var startCoord=new Vector3Int(vCoord.x,startY,vCoord.z);
         var sampleInput=new SampleContext(startCoord,cCoord);
         var sampleDensityContext=new SampleDensityContext(sampleInput);
         double prevDensity=SampleDensity(ref sampleDensityContext,out _);
         for(int y=startY-1;y>=bottomY;y--){
          var coord=new Vector3Int(vCoord.x,y,vCoord.z);
          var input=new SampleContext(coord,cCoord);
          var context=new SampleDensityContext(input){
           hasHeight=sampleDensityContext.hasHeight,
           heightValue=sampleDensityContext.heightValue,
          };
          double d=SampleDensity(ref context,out _);
          //  Detecta crossing (iso surface)
          bool prevInside=(-prevDensity<isoLevel);
          bool currInside=(-d<isoLevel);
          if(prevInside!=currInside){
           SampleNormalContext normalContext=default;
           SampleNormalContext.Build(coord,cCoord,ref context,ref normalContext);
           normal=-SampleNormal(ref normalContext);
           if(Vector3.Dot(normal,Vector3.up)<=0f){
            prevDensity=d;
            continue;
           }
           Vector2Int cnkRgn=cCoordTocnkRgn(cCoord);
           hitPoint=new Vector3(coord.x,y,coord.z)+new Vector3(0.5f,0.5f,0.5f)-new Vector3(Width/2f,0,Depth/2f)+new Vector3(cnkRgn.x,0,cnkRgn.y);
           //Logs.Debug(()=>"coord:"+coord+";context.heightValue:"+context.heightValue+";d:"+d+";prevDensity:"+prevDensity);
           return true;
          }
          prevDensity=d;
         }
         hitPoint=default;
         normal=default;
         return false;
        }
    }
}