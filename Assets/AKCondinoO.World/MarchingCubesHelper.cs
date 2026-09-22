using AKCondinoO.Bootstrap;
using System;
using System.Runtime.CompilerServices;
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
           hitPoint=GetInterpolatedSurface(
            context.cCoord,
            new Vector3Int(context.vCoord.x,y  ,context.vCoord.z),
            new Vector3Int(context.vCoord.x,y+1,context.vCoord.z),
            (float)d,
            (float)prevDensity,
            isoLevel
           );
           //Logs.Debug(()=>"coord:"+coord+";context.heightValue:"+context.heightValue+";d:"+d+";prevDensity:"+prevDensity);
           return true;
          }
          prevDensity=d;
         }
         hitPoint=default;
         normal=default;
         return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float GetMarchingSurfaceHeight(
         Vector2Int cCoord,
         Vector3Int vCoord,
         float x,
         float z,
         float isoLevel=-50.0f
        ){
         SampleDensityContext context=new(
          new(
           new Vector3Int(vCoord.x,0,vCoord.z),
           cCoord
          )
         );
         SampleTerrainHeight(ref context,out double terrainDensityStartHeight);
         double heightValue=context.heightValue;
         float terrainSmoothingHeight=(float)(terrainDensityStartHeight-heightValue);
         float surfaceEstimate=(float)(
          heightValue+
          terrainSmoothingHeight*(isoLevel/100f)
         );
         int y=Mathf.FloorToInt(surfaceEstimate);
         context.SetvCoordY(y);
         float density0=SampleDensity(ref context,heightValue);
         context.SetvCoordY(y+1);
         float density1=SampleDensity(ref context,heightValue);
         Vector2Int cnkRgn=cCoordTocnkRgn(cCoord);
         Vector3 surface=GetInterpolatedSurface(
          context.cCoord,
          new(context.vCoord.x,y  ,context.vCoord.z),
          new(context.vCoord.x,y+1,context.vCoord.z),
          density0,
          density1,
          isoLevel
         );
         //Logs.Debug(()=>"'x':"+x+";'z':"+z+";'cCoord':"+cCoord+";'vCoord':"+vCoord+";'surfaceEstimate':"+surfaceEstimate+";'terrainDensityStartHeight':"+terrainDensityStartHeight+";'heightValue':"+heightValue+";'density0':"+density0+";'density1':"+density1+";'surface':"+surface);
         return surface.y-.5f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector3 GetInterpolatedSurface(
         Vector2Int cCoord,
         Vector3Int vCoord0,
         Vector3Int vCoord1,
         float density0,
         float density1,
          float isoLevel=-50.0f
        ){
         Vector3 interpolation=InterpolateVertex(
          new Vector3(vCoord0.x,vCoord0.y,vCoord0.z),
          new Vector3(vCoord1.x,vCoord1.y,vCoord1.z),
          -density0,
          -density1,
          isoLevel
         );
         Vector2Int cnkRgn=cCoordTocnkRgn(cCoord);
         return interpolation+new Vector3(0.5f,0f,0.5f)-new Vector3(Width/2f,0f,Depth/2f)
          +new Vector3(cnkRgn.x,0,cnkRgn.y);
        }
    }
}