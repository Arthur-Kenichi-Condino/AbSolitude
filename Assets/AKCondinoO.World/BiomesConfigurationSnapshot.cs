using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using AKCondinoO.World.Biomes;
using AKCondinoO.World.MarchingCubes;
using LibNoise;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal static class BiomesConfigurationSnapshot{
     private static readonly ReaderWriterLockSlim rwl=new(LockRecursionPolicy.SupportsRecursion);
     private static float terrainSmoothingHeight;
     private static ModuleBase terrainModule;
     private static NoiseNodesSnapshot terrainNodesSnapshot;
        internal static void Build(){
         rwl.EnterWriteLock();
         try{
          DisposeAll();
          BiomesConfigurationSnapshot.terrainSmoothingHeight=BiomesSystem.singleton.terrainSmoothingHeight;
          terrainModule=BiomesSystem.singleton.terrain.Build(BiomesSystem.singleton.seed,null,out _,out terrainNodesSnapshot);
          terrainNodesSnapshot.MergeSpawnTables();
         }catch(Exception e){
          Logs.Message(Logs.LogType.Error,e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitWriteLock();
         }
        }
        internal static void DisposeAll(){
         if(terrainModule!=null){terrainModule.Dispose();}terrainModule=null;
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
         double heightValue=-1d;
         rwl.EnterReadLock();
         try{
          heightValue=terrainModule.GetValue(noiseInput.z,noiseInput.x,0);
         }catch(Exception e){
          Logs.Message(Logs.LogType.Error,e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitReadLock();
         }
         if(heightValue>=0d){
          if(noiseInputRounded.y<=heightValue+terrainSmoothingHeight){
           float density=100.0f;
           float delta=(float)heightValue-noiseInputRounded.y;
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
         }
         vxl=Voxel.air;
        }
    }
    internal class NoiseNodesSnapshot{
     static readonly Dictionary<(Type,string),ObjectPoolBase>noiseNodesSnapshotPool=new(){
      {(typeof(NoiseNodesSnapshot        ),""),Pool.GetPool<NoiseNodesSnapshot        >("",()=>new(),(NoiseNodesSnapshot         item)=>{item.Reset();})},
      {(typeof(NoiseNodesSnapshotOperator),""),Pool.GetPool<NoiseNodesSnapshotOperator>("",()=>new(),(NoiseNodesSnapshotOperator item)=>{item.Reset();})},
     };
        internal static NoiseNodesSnapshot Rent((Type,string)poolId){
         return(NoiseNodesSnapshot)noiseNodesSnapshotPool[poolId].ObjectRent();
        }
     internal NoiseNodesSnapshot root{get;private set;}
     protected NoiseNodesSnapshot parent;
     protected readonly List<BiomeSpawnTable>tempSpawnTables=new();
        internal virtual void SetFrom(NoiseNode noiseNode,NoiseNodesSnapshot parent){
         if(parent!=null){
          this.parent=parent;
          root=parent.root;
          tempSpawnTables.AddRange(parent.tempSpawnTables);
         }
         if(root==null){root=this;}
         if(noiseNode.spawnTables!=null){
          tempSpawnTables.AddRange(noiseNode.spawnTables);
         }
        }
     protected ModuleBase module;
        internal virtual void SetModule(ModuleBase module){
         this.module=module;
        }
        internal virtual void Reset(){
        }
     protected BiomeSpawnTablesSnapshot table;
        internal virtual void MergeSpawnTables(){
         if(tempSpawnTables.Count>0){
          table=new();
          table.DoSnapshot(tempSpawnTables);
         }
         tempSpawnTables.Clear();
        }
    }
    internal class NoiseNodesSnapshotOperator:NoiseNodesSnapshot{
     protected NoiseNodesSnapshot input;
        internal virtual void SetInput(NoiseNodesSnapshot input){
         this.input=input;
         tempSpawnTables.Clear();
        }
        internal override void MergeSpawnTables(){
         input.MergeSpawnTables();
        }
    }
    internal class BiomeSpawnTablesSnapshot{
        internal virtual void DoSnapshot(List<BiomeSpawnTable>tempSpawnTables){
         foreach(var spawnTable in tempSpawnTables){
         }
        }
        internal virtual void Reset(){
        }
    }
}