using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using AKCondinoO.World.Biomes;
using AKCondinoO.World.MarchingCubes;
using AKCondinoO.World.Spawning;
using LibNoise;
using LibNoise.Operator;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal class BiomesConfigurationContext{
     internal static readonly Utilities.ObjectPool<BiomesConfigurationContext>pool=
      Pool.GetPool<BiomesConfigurationContext>(
       "",
       ()=>new(),
       (BiomesConfigurationContext item)=>{
        Pool.ReturnArray<bool  >(item.hasTerrainHeightNoiseCache,true);item.hasTerrainHeightNoiseCache=null;
        Pool.ReturnArray<double>(item.   terrainHeightNoiseCache,true);item.   terrainHeightNoiseCache=null;
       }
      );
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
     private static readonly ReaderWriterLockSlim rwl=new(LockRecursionPolicy.SupportsRecursion);
        internal static void IsReading(){
         rwl.EnterReadLock();
        }
        internal static void StoppedReading(){
         rwl. ExitReadLock();
        }
     private static Snapshot snapshot;
        private class Snapshot{
         internal static readonly Utilities.ObjectPool<Snapshot>pool=
          Pool.GetPool<Snapshot>(
           "",
           ()=>new(),
           (Snapshot item)=>{item.Dispose();}
          );
         internal float terrainSmoothingHeight;
         internal ModuleBase terrainModule;
         internal NoiseNodesSnapshot terrainNodesSnapshot;
            internal void DoSnapshot(){
             terrainSmoothingHeight=BiomesSystem.singleton.terrainSmoothingHeight;
             terrainModule=BiomesSystem.singleton.terrain.Build(BiomesSystem.singleton.seed,null,out _,out terrainNodesSnapshot);
             terrainNodesSnapshot.MergeTables();
            }
            internal void Dispose(){
             if(terrainModule!=null){terrainModule.Dispose();}terrainModule=null;
             if(terrainNodesSnapshot!=null){
              NoiseNodesSnapshot.Return(terrainNodesSnapshot.GetType(),terrainNodesSnapshot);terrainNodesSnapshot=null;
             }
            }
        }
        internal static void Build(){
         Snapshot newSnapshot=Snapshot.pool.Rent();
         newSnapshot.DoSnapshot();
         var oldSnapshot=snapshot;
         rwl.EnterWriteLock();
         try{
         snapshot=newSnapshot;
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitWriteLock();
         }
         Snapshot.pool.Return(oldSnapshot);
        }
        internal static void DisposeAll(){
         Snapshot.pool.Return(snapshot);snapshot=null;
        }
        internal static void Setvxl(ref Voxel vxl,Vector3Int vCoord,Vector2Int cCoord,BiomesConfigurationContext context){
         var snapshot=BiomesConfigurationSnapshot.snapshot;
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
          heightValue=snapshot.terrainModule.GetValue(noiseInput.z,noiseInput.x,0);
          context.UpdateTerrainHeightNoiseCache(heightValue);
         }
         if(heightValue>=0d){
          Resolve(new(noiseInput.z,noiseInput.x,0),out MaterialTablesSnapshot materialTable);
          MaterialId material=MaterialId.MuddyDirt;
          if(materialTable!=null){material=materialTable.baseMaterial;}
          if(noiseInputRounded.y<=heightValue+snapshot.terrainSmoothingHeight){
           float density=100.0f;
           float delta=(float)heightValue-noiseInputRounded.y;
           float smoothingValue=(snapshot.terrainSmoothingHeight-delta)/snapshot.terrainSmoothingHeight;
           density*=1f-smoothingValue;
           density=Mathf.Clamp(density,0f,100.0f);
           vxl=new(
            density,
            material,
            Vector3.zero
           );
           return;
          }
         }
         vxl=Voxel.air;
        }
        private static void Resolve(Vector3 noiseInput,out   MaterialTablesSnapshot table){
         var snapshot=BiomesConfigurationSnapshot.snapshot;
         table=null;
         snapshot.terrainNodesSnapshot.Resolve(noiseInput,out table);
        }
        private static void Resolve(Vector3 noiseInput,out BiomeSpawnTablesSnapshot table){
         var snapshot=BiomesConfigurationSnapshot.snapshot;
         table=null;
         snapshot.terrainNodesSnapshot.Resolve(noiseInput,out table);
        }
    }
    internal class NoiseNodesSnapshot{
     static readonly Dictionary<(Type,string),ObjectPoolBase>noiseNodesSnapshotPool=new(){
      {(typeof(NoiseNodesSnapshot          ),""),Pool.GetPool<NoiseNodesSnapshot          >("",()=>new(),(NoiseNodesSnapshot           item)=>{item.Reset();})},
      {(typeof(OperatorNoiseNodesSnapshot  ),""),Pool.GetPool<OperatorNoiseNodesSnapshot  >("",()=>new(),(OperatorNoiseNodesSnapshot   item)=>{item.Reset();})},
      {(typeof(SelectorNoiseNodesSnapshot  ),""),Pool.GetPool<SelectorNoiseNodesSnapshot  >("",()=>new(),(SelectorNoiseNodesSnapshot   item)=>{item.Reset();})},
      {(typeof(MultiplierNoiseNodesSnapshot),""),Pool.GetPool<MultiplierNoiseNodesSnapshot>("",()=>new(),(MultiplierNoiseNodesSnapshot item)=>{item.Reset();})},
     };
        internal static NoiseNodesSnapshot Rent(Type poolId){
         return(NoiseNodesSnapshot)noiseNodesSnapshotPool[(poolId,"")].ObjectRent();
        }
        internal static void Return(Type poolId,NoiseNodesSnapshot nodesSnapshot){
         noiseNodesSnapshotPool[(poolId,"")].ObjectReturn(nodesSnapshot);
        }
     internal NoiseNodesSnapshot root{get;private set;}
     protected NoiseNodesSnapshot parent;
     protected readonly List<NodeMaterialTable>tempMaterialTables=new();
     protected readonly List<BiomeSpawnTable>tempSpawnTables=new();
        internal virtual void SetFrom(NoiseNode noiseNode,NoiseNodesSnapshot parent){
         if(parent!=null){
          this.parent=parent;
          root=parent.root;
          tempMaterialTables.AddRange(parent.tempMaterialTables);
          tempSpawnTables   .AddRange(parent.tempSpawnTables   );
         }
         if(root==null){root=this;}
         if(noiseNode.materialTables!=null){tempMaterialTables.AddRange(noiseNode.materialTables);}
         if(noiseNode.spawnTables   !=null){tempSpawnTables   .AddRange(noiseNode.spawnTables   );}
        }
        internal virtual void Reset(){
         parent=null;
         root=null;
         ClearTempTables();
         module=null;
         if(materialTable!=null){
          MaterialTablesSnapshot.pool.Return(materialTable);materialTable=null;
         }
         if(spawnTable!=null){
          BiomeSpawnTablesSnapshot.pool.Return(spawnTable);spawnTable=null;
         }
        }
        internal virtual void ClearTempTables(){
         ClearTempMaterialTables();
         ClearTempSpawnTables();
        }
        internal virtual void ClearTempMaterialTables(){
         tempMaterialTables.Clear();
        }
        internal virtual void ClearTempSpawnTables(){
         tempSpawnTables.Clear();
        }
     protected ModuleBase module;
        internal virtual void SetModule(ModuleBase module){
         this.module=module;
        }
        internal virtual double GetValue(Vector3 noiseInput){
         return this.module.GetValue(noiseInput.x,noiseInput.y,noiseInput.z);
        }
        internal virtual void MergeTables(){
         MergeMaterialTables();
         MergeSpawnTables();
        }
     protected MaterialTablesSnapshot materialTable;
        internal virtual void MergeMaterialTables(){
         if(tempMaterialTables.Count>0){
          materialTable=MaterialTablesSnapshot.pool.Rent();
          materialTable.DoSnapshot(tempMaterialTables);
         }
         ClearTempMaterialTables();
        }
        internal virtual void Resolve(Vector3 noiseInput,out   MaterialTablesSnapshot table){
         table=this.materialTable;
        }
     protected BiomeSpawnTablesSnapshot spawnTable;
        internal virtual void MergeSpawnTables(){
         if(tempSpawnTables.Count>0){
          spawnTable=BiomeSpawnTablesSnapshot.pool.Rent();
          spawnTable.DoSnapshot(tempSpawnTables);
         }
         ClearTempSpawnTables();
        }
        internal virtual void Resolve(Vector3 noiseInput,out BiomeSpawnTablesSnapshot table){
         table=this.spawnTable;
        }
    }
    internal class OperatorNoiseNodesSnapshot:NoiseNodesSnapshot{
        internal override void Reset(){
         NoiseNodesSnapshot.Return(input.GetType(),input);input=null;
         base.Reset();
        }
     protected NoiseNodesSnapshot input;
        internal virtual void SetInput(NoiseNodesSnapshot input){
         this.input=input;
         ClearTempTables();
        }
        internal override void MergeTables(){
         input.MergeTables();
        }
        internal override void Resolve(Vector3 noiseInput,out   MaterialTablesSnapshot table){
         input.Resolve(noiseInput,out table);
        }
        internal override void Resolve(Vector3 noiseInput,out BiomeSpawnTablesSnapshot table){
         input.Resolve(noiseInput,out table);
        }
    }
    internal class SelectorNoiseNodesSnapshot:NoiseNodesSnapshot{
        internal override void Reset(){
         NoiseNodesSnapshot.Return(inputA.GetType(),inputA);inputA=null;
         NoiseNodesSnapshot.Return(inputB.GetType(),inputB);inputB=null;
         NoiseNodesSnapshot.Return(controller.GetType(),controller);controller=null;
         base.Reset();
        }
     protected NoiseNodesSnapshot inputA;
     protected NoiseNodesSnapshot inputB;
     protected NoiseNodesSnapshot controller;
        internal virtual void SetInput(
         NoiseNodesSnapshot inputA,
         NoiseNodesSnapshot inputB,
         NoiseNodesSnapshot controller
        ){
         this.inputA=inputA;
         this.inputB=inputB;
         this.controller=controller;
         ClearTempTables();
        }
        internal override void MergeTables(){
         inputA.MergeTables();
         inputB.MergeTables();
         controller.ClearTempTables();
        }
        internal override void Resolve(Vector3 noiseInput,out   MaterialTablesSnapshot table){
         var input=OnResolveGetNode(noiseInput);
         input.Resolve(noiseInput,out table);
        }
        internal override void Resolve(Vector3 noiseInput,out BiomeSpawnTablesSnapshot table){
         var input=OnResolveGetNode(noiseInput);
         input.Resolve(noiseInput,out table);
        }
        private NoiseNodesSnapshot OnResolveGetNode(Vector3 noiseInput){
         var cv=controller.GetValue(noiseInput);
         var select=(Select)module;
         var max=select.Maximum;
         var min=select.Minimum;
         if(cv<min||cv>max){
          return inputA;
         }
         return inputB;
        }
    }
    internal class MultiplierNoiseNodesSnapshot:NoiseNodesSnapshot{
        internal override void Reset(){
         NoiseNodesSnapshot.Return(lhs.GetType(),lhs);lhs=null;
         NoiseNodesSnapshot.Return(rhs.GetType(),rhs);rhs=null;
         base.Reset();
        }
     public NoiseNodesSnapshot lhs;
     public NoiseNodesSnapshot rhs;
        internal virtual void SetInput(
         NoiseNodesSnapshot lhs,
         NoiseNodesSnapshot rhs
        ){
         this.lhs=lhs;
         this.rhs=rhs;
         ClearTempTables();
        }
        internal override void MergeTables(){
         lhs.MergeTables();
         rhs.ClearTempTables();
        }
        internal override void Resolve(Vector3 noiseInput,out   MaterialTablesSnapshot table){
         lhs.Resolve(noiseInput,out table);
        }
        internal override void Resolve(Vector3 noiseInput,out BiomeSpawnTablesSnapshot table){
         lhs.Resolve(noiseInput,out table);
        }
    }
    internal class MaterialTablesSnapshot{
     internal static readonly Utilities.ObjectPool<MaterialTablesSnapshot>pool=
      Pool.GetPool<MaterialTablesSnapshot>(
       "",
       ()=>new(),
       (MaterialTablesSnapshot item)=>{
        item.Reset();
       }
      );
     internal MaterialId baseMaterial;
        internal virtual void DoSnapshot(List<NodeMaterialTable>tempMaterialTables){
         foreach(var materialTable in tempMaterialTables){
          foreach(var entry in materialTable.entries){
           baseMaterial=(MaterialId)Math.Max((uint)entry.material,(uint)baseMaterial);
          }
         }
        }
        internal virtual void Reset(){
        }
    }
    internal class BiomeSpawnTablesSnapshot{
     internal static readonly Utilities.ObjectPool<BiomeSpawnTablesSnapshot>pool=
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
            Logs.Debug("'spawnTable entry':"+entry.prefab.name);
            ByChanceObjectSpawnEntry<SimObject>pickerEntry=ByChanceObjectSpawnEntry<SimObject>.pool.Rent();
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