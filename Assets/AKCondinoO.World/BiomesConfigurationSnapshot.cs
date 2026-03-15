using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using AKCondinoO.World.Biomes;
using AKCondinoO.World.MarchingCubes;
using AKCondinoO.World.Spawning;
using LibNoise;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal class BiomesConfigurationContext{
     internal int width;
     internal int height;
     internal int depth;
     internal Vector3Int coord;
     internal Vector3Int terrainHeightNoiseCachePadding;
     internal bool[]hasTerrainHeightNoiseCache;
     internal double[]terrainHeightNoiseCache;
        internal int TerrainHeightNoiseCacheIndex(){
         return(coord.z+terrainHeightNoiseCachePadding.z)+(coord.x+terrainHeightNoiseCachePadding.x)*depth;
        }
        internal void UpdateTerrainHeightNoiseCache(double noiseValue){
         int index=TerrainHeightNoiseCacheIndex();
         terrainHeightNoiseCache[index]=noiseValue;
         hasTerrainHeightNoiseCache[index]=true;
        }
        internal bool TryUseTerrainHeightNoiseCache(out double noiseValue){
         noiseValue=-1d;
         int index=TerrainHeightNoiseCacheIndex();
         if(hasTerrainHeightNoiseCache[index]){
          noiseValue=terrainHeightNoiseCache[index];
          return true;
         }
         return false;
        }
    }
    internal static class BiomesConfigurationSnapshot{
     internal static readonly Utilities.ObjectPool<BiomesConfigurationContext>biomesConfigurationContextPool=
      Pool.GetPool<BiomesConfigurationContext>(
       "",
       ()=>new(),
       (BiomesConfigurationContext item)=>{
        Pool.ReturnArray<bool  >(item.hasTerrainHeightNoiseCache,true);item.hasTerrainHeightNoiseCache=null;
        Pool.ReturnArray<double>(item.   terrainHeightNoiseCache,true);item.   terrainHeightNoiseCache=null;
       }
      );
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
         if(terrainNodesSnapshot!=null){
          NoiseNodesSnapshot.Return(terrainNodesSnapshot.GetType(),terrainNodesSnapshot);terrainNodesSnapshot=null;
         }
        }
        internal static void Resolve(Vector3Int noiseInput,out BiomeSpawnTablesSnapshot table){
         table=null;
         rwl.EnterReadLock();
         try{
          terrainNodesSnapshot.Resolve(noiseInput,out table);
         }catch(Exception e){
          Logs.Message(Logs.LogType.Error,e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitReadLock();
         }
        }
        internal static void Setvxl(ref Voxel vxl,Vector3Int vCoord,Vector2Int cCoord,BiomesConfigurationContext context){
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
         if(!context.TryUseTerrainHeightNoiseCache(out heightValue)){
          rwl.EnterReadLock();
          try{
           heightValue=terrainModule.GetValue(noiseInput.z,noiseInput.x,0);
          }catch(Exception e){
           Logs.Message(Logs.LogType.Error,e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          }finally{
           rwl.ExitReadLock();
          }
          context.UpdateTerrainHeightNoiseCache(heightValue);
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
        internal static NoiseNodesSnapshot Rent(Type poolId){
         return(NoiseNodesSnapshot)noiseNodesSnapshotPool[(poolId,"")].ObjectRent();
        }
        internal static void Return(Type poolId,NoiseNodesSnapshot nodesSnapshot){
         noiseNodesSnapshotPool[(poolId,"")].ObjectReturn(nodesSnapshot);
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
        internal virtual void Reset(){
         parent=null;
         root=null;
         tempSpawnTables.Clear();
         module=null;
         if(table!=null){
          BiomeSpawnTablesSnapshot.biomeSpawnTablesSnapshotPool.Return(table);table=null;
         }
        }
     protected ModuleBase module;
        internal virtual void SetModule(ModuleBase module){
         this.module=module;
        }
     protected BiomeSpawnTablesSnapshot table;
        internal virtual void MergeSpawnTables(){
         if(tempSpawnTables.Count>0){
          table=BiomeSpawnTablesSnapshot.biomeSpawnTablesSnapshotPool.Rent();
          table.DoSnapshot(tempSpawnTables);
         }
         tempSpawnTables.Clear();
        }
        internal virtual void Resolve(Vector3Int noiseInput,out BiomeSpawnTablesSnapshot table){
         table=this.table;
        }
    }
    internal class NoiseNodesSnapshotOperator:NoiseNodesSnapshot{
        internal override void Reset(){
         NoiseNodesSnapshot.Return(input.GetType(),input);input=null;
         base.Reset();
        }
     protected NoiseNodesSnapshot input;
        internal virtual void SetInput(NoiseNodesSnapshot input){
         this.input=input;
         tempSpawnTables.Clear();
        }
        internal override void MergeSpawnTables(){
         input.MergeSpawnTables();
        }
        internal override void Resolve(Vector3Int noiseInput,out BiomeSpawnTablesSnapshot table){
         input.Resolve(noiseInput,out table);
        }
    }
    internal class BiomeSpawnTablesSnapshot{
     internal static readonly Utilities.ObjectPool<BiomeSpawnTablesSnapshot>biomeSpawnTablesSnapshotPool=
      Pool.GetPool<BiomeSpawnTablesSnapshot>(
       "",
       ()=>new(),
       (BiomeSpawnTablesSnapshot item)=>{
        item.Reset();
       }
      );
     internal readonly Dictionary<int,
      ByChancePicker<SimObject>
     >pickerByLayer=new();
        internal virtual void DoSnapshot(List<BiomeSpawnTable>tempSpawnTables){
         foreach(var spawnTable in tempSpawnTables){
          foreach(var spawnTableLayer in spawnTable.layers){
           if(!pickerByLayer.TryGetValue(spawnTableLayer.layer,out var picker)){
            pickerByLayer.Add(spawnTableLayer.layer,picker=new());
           }
           foreach(var entry in spawnTableLayer.entries){
            Logs.Message(Logs.LogType.Debug,"'spawnTable entry':"+entry.prefab.name);
            ByChanceObjectSpawnEntry<SimObject>pickerEntry=ByChancePicker<SimObject>.entryPool.Rent();
            pickerEntry.prefab=entry.prefab;
            pickerEntry.chance=entry.chance;
            picker.items.Add(pickerEntry);
           }
          }
         }
         foreach(var kvp in pickerByLayer){
          var picker=kvp.Value;
          picker.Build();
         }
        }
        internal virtual void Reset(){
         foreach(var kvp in pickerByLayer){
          var picker=kvp.Value;
          picker.Clear();
         }
        }
    }
}