using AKCondinoO.Bootstrap;
using AKCondinoO.World.MarchingCubes;
using LibNoise;
using System;
using System.Threading;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal static class BiomesConfigurationSnapshot{
     private static readonly ReaderWriterLockSlim rwl=new(LockRecursionPolicy.SupportsRecursion);
     private static float terrainSmoothingHeight;
     private static ModuleBase terrainModule;
        internal static void Build(){
         rwl.EnterWriteLock();
         try{
          BiomesConfigurationSnapshot.terrainSmoothingHeight=BiomesSystem.singleton.terrainSmoothingHeight;
          if(terrainModule!=null){terrainModule.Dispose();}
          terrainModule=BiomesSystem.singleton.terrain.Build(BiomesSystem.singleton.seed);
         }catch(Exception e){
          Logs.Message(Logs.LogType.Error,e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitWriteLock();
         }
        }
        internal static void Setvxl(ref Voxel vxl,Vector3Int vCoord,Vector2Int cCoord){
         /*  fora do mundo, baixo:  */
         if(vCoord.y<=0){
          vxl=Voxel.bedrock;
          return;
         /*  fora do mundo, cima:  */
         }else if(vCoord.y>=Height){
          vxl=Voxel.air;
          return;
         }
         ValidatevCoord(ref cCoord,ref vCoord);
         Vector2Int cnkRgn=cCoordTocnkRgn(cCoord);
         Vector3Int noiseInputRounded=vCoord+new Vector3Int(cnkRgn.x,0,cnkRgn.y);
         Vector3    noiseInput       =noiseInputRounded+new Vector3(.5f,.5f,.5f);
         rwl.EnterReadLock();
         try{
          double heightValue=terrainModule.GetValue(noiseInput.z,noiseInput.x,0);
          if(noiseInput.y<=heightValue+terrainSmoothingHeight){
           float density=100.0f;
           float delta=(float)heightValue-noiseInput.y;
           float smoothingValue=(terrainSmoothingHeight-delta)/terrainSmoothingHeight;
           density*=1f-smoothingValue;
           density=Mathf.Clamp(density,0f,100.0f);
           vxl=new(
            density,
            MaterialId.MuddyDirt,
            Vector3.zero
           );
           return;
          }
          vxl=Voxel.air;
         }catch(Exception e){
          Logs.Message(Logs.LogType.Error,e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitReadLock();
         }
        }
    }
}