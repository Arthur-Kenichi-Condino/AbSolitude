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
using static AKCondinoO.World.BiomesConfigurationSnapshot;
using static AKCondinoO.World.Spawning.ByChanceObjectSpawnEntry<AKCondinoO.SimObjects.SimObject>;
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
        internal class Snapshot{
         internal static readonly Utilities.ObjectPool<Snapshot>pool=
          Pool.GetPool<Snapshot>(
           "",
           ()=>new(),
           (Snapshot item)=>{item.Dispose();}
          );
         internal float terrainSmoothingHeight;
         internal NoiseNodesSnapshot nodes;
            internal void DoSnapshot(){
             terrainSmoothingHeight=BiomesSystem.singleton.terrainSmoothingHeight;
             BiomesSystem.singleton.terrain.Build(BiomesSystem.singleton.seed,null,out _,out nodes);
             nodes.DiscoverChannels();
             nodes.Propagation();
            }
            internal void Dispose(){
             if(nodes!=null){
              NoiseNodesSnapshot.Return(nodes.GetType(),nodes);nodes=null;
             }
            }
        }
        internal class SpawnSettingsSnapshot{
         internal static readonly Utilities.ObjectPool<SpawnSettingsSnapshot>pool=
          Pool.GetPool<SpawnSettingsSnapshot>(
           "",
           ()=>new(),
           (SpawnSettingsSnapshot item)=>{
            item.OnReturnToPoolRecycle();
           }
          );
         internal int minLayer;
         internal int maxLayer;
         internal readonly SortedDictionary<int,SnapshotSpawnLayerData>layerData=new();
            internal void OnReturnToPoolRecycle(){
             minLayer=int.MaxValue;
             maxLayer=int.MinValue;
             layerData.Clear();
            }
            internal void MergeFrom(SpawnSettingsSnapshot other){
             if(other==null)return;
             minLayer=Math.Min(minLayer,other.minLayer);
             maxLayer=Math.Max(maxLayer,other.maxLayer);
             foreach(var kvp in other.layerData){
              layerData[kvp.Key]=kvp.Value;
             }
            }
        }
        internal struct SnapshotSpawnLayerData{
         internal int gridSize;
         internal Vector3 maxBoundsSize;
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
        internal static void SetTerrainvxl(ref Voxel vxl,Vector3Int vCoord,Vector2Int cCoord,BiomesConfigurationContext context){
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
          heightValue=snapshot.nodes.GetValue(NoiseChannel.Terrain,new(noiseInput.z,noiseInput.x,0));
          context.UpdateTerrainHeightNoiseCache(heightValue);
         }
         if(heightValue>=0d){
          Resolve(NoiseChannel.Terrain,new(noiseInput.z,noiseInput.x,0),out MaterialTablesSnapshot materialTable);
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
        internal static SpawnSettingsSnapshot GetSpawnSettings(NoiseChannel channel){
         snapshot.nodes.GetSpawnSettings(channel,out var spawnSettingsSnapshot);
         return spawnSettingsSnapshot;
        }
        internal static ByChanceObjectSpawnEntry<SimObject>GetSpawnEntry(NoiseChannel channel,Vector3Int vCoord,Vector2Int cCoord,int layer,out SpawnVariation variation){
         var snapshot=BiomesConfigurationSnapshot.snapshot;
         variation=default;
         Vector2Int cnkRgn=cCoordTocnkRgn(cCoord);
         Vector3Int noiseInputRounded=vCoord+new Vector3Int(cnkRgn.x,0,cnkRgn.y);
         Vector3    noiseInput       =noiseInputRounded+new Vector3(.5f,.5f,.5f);
         Resolve(channel,new(noiseInput.z,noiseInput.x,0),out BiomeSpawnTablesSnapshot table);
         if(table!=null){
          double spawnValue=snapshot.nodes.GetValue(channel,new(noiseInput.z,noiseInput.x,0));
          var picker=table.pickerByLayer[layer];
          if(picker.Get(1,out var result)){
           variation=result.variations.Get(snapshot,noiseInput);
           return result;
          }
         }
         return null;
        }
        private static void Resolve(NoiseChannel channel,Vector3 noiseInput,out MaterialTablesSnapshot table){
         var snapshot=BiomesConfigurationSnapshot.snapshot;
         snapshot.nodes.Resolve(channel,noiseInput,out table);
        }
        private static void Resolve(NoiseChannel channel,Vector3 noiseInput,out BiomeSpawnTablesSnapshot table){
         var snapshot=BiomesConfigurationSnapshot.snapshot;
         snapshot.nodes.Resolve(channel,noiseInput,out table);
        }
        internal static float NormalizeHeight(double height,float maxHeight){
         return(float)(height/maxHeight);
        }
    }
    enum NoiseChannel{
     None=-1,
     Terrain=0,
     Spawn,
     Rotation,
     Scale,
     Temperature,
    }
    internal class NoiseNodesSnapshot{
     static readonly Dictionary<(Type,string),ObjectPoolBase>noiseNodesSnapshotPool=new(){
      {(typeof(NoiseNodesSnapshot          ),""),Pool.GetPool<NoiseNodesSnapshot          >("",()=>new(),(NoiseNodesSnapshot           item)=>{item.OnReturnToPoolRecycle();},false)},
      {(typeof(GraphNodesSnapshot          ),""),Pool.GetPool<GraphNodesSnapshot          >("",()=>new(),(GraphNodesSnapshot           item)=>{item.OnReturnToPoolRecycle();},false)},
      {(typeof(OperatorNoiseNodesSnapshot  ),""),Pool.GetPool<OperatorNoiseNodesSnapshot  >("",()=>new(),(OperatorNoiseNodesSnapshot   item)=>{item.OnReturnToPoolRecycle();},false)},
      {(typeof(SelectorNoiseNodesSnapshot  ),""),Pool.GetPool<SelectorNoiseNodesSnapshot  >("",()=>new(),(SelectorNoiseNodesSnapshot   item)=>{item.OnReturnToPoolRecycle();},false)},
      {(typeof(MultiplierNoiseNodesSnapshot),""),Pool.GetPool<MultiplierNoiseNodesSnapshot>("",()=>new(),(MultiplierNoiseNodesSnapshot item)=>{item.OnReturnToPoolRecycle();},false)},
     };
        internal static NoiseNodesSnapshot Rent(Type poolId){
         return(NoiseNodesSnapshot)noiseNodesSnapshotPool[(poolId,"")].ObjectRent();
        }
        internal static void Return(Type poolId,NoiseNodesSnapshot nodesSnapshot){
         noiseNodesSnapshotPool[(poolId,"")].ObjectReturn(nodesSnapshot);
        }
     internal NoiseNodesSnapshot root{get;private set;}
     protected NoiseNodesSnapshot parent;
     internal readonly HashSet<NoiseChannel>inheritedChannels=new();
     protected readonly Dictionary<NoiseChannel,ModuleBase>modules=new();
     protected readonly Dictionary<NoiseChannel,SpawnSettingsSnapshot>spawnSettings=new();
     protected readonly Dictionary<NoiseChannel,MaterialTablesSnapshot>materialTables=new();
     protected readonly Dictionary<NoiseChannel,BiomeSpawnTablesSnapshot>spawnTables=new();
        internal virtual void OnReturnToPoolRecycle(){
         parent=null;
         root=null;
         ClearTempTables();
         foreach(var kvp in modules){kvp.Value.Dispose();}modules.Clear();
         foreach(var kvp in spawnSettings){SpawnSettingsSnapshot.pool.Return(kvp.Value);}spawnSettings.Clear();
         foreach(var kvp in materialTables){MaterialTablesSnapshot.pool.Return(kvp.Value);}materialTables.Clear();
         foreach(var kvp in spawnTables){BiomeSpawnTablesSnapshot.pool.Return(kvp.Value);}spawnTables.Clear();
         inheritedChannels.Clear();
        }
     protected readonly Dictionary<NoiseChannel,List<NodeMaterialTable>>tempMaterialTables=new();
     protected readonly Dictionary<NoiseChannel,List<BiomeSpawnTable>>tempSpawnTables=new();
        internal virtual void SetFrom(Node noiseNode,NoiseNodesSnapshot parent){
         var materialList=GetOrCreateMaterialList(noiseNode.channel);
         var spawnList=GetOrCreateSpawnList(noiseNode.channel);
         if(parent!=null){
          this.parent=parent;
          root=parent.root;
          foreach(var kvp in parent.tempMaterialTables){
           GetOrCreateMaterialList(kvp.Key).AddRange(kvp.Value);
          }
          foreach(var kvp in parent.tempSpawnTables){
           GetOrCreateSpawnList(kvp.Key).AddRange(kvp.Value);
          }
         }
         if(root==null){root=this;}
         if(noiseNode.materialTables!=null){materialList.AddRange(noiseNode.materialTables);}
         if(noiseNode.spawnTables!=null){spawnList.AddRange(noiseNode.spawnTables);}
        }
        internal virtual void ClearTempTables(){
         foreach(var channel in inheritedChannels){
          ClearTempMaterialTables(channel);
          ClearTempSpawnTables(channel);
         }
        }
        internal virtual void ClearTempMaterialTables(NoiseChannel channel){
         if(tempMaterialTables.TryGetValue(channel,out var list)){
          list.Clear();
         }
        }
        internal virtual void ClearTempSpawnTables(NoiseChannel channel){
         if(tempSpawnTables.TryGetValue(channel,out var list)){
          list.Clear();
         }
        }
        List<NodeMaterialTable>GetOrCreateMaterialList(NoiseChannel channel){
         if(!tempMaterialTables.TryGetValue(channel,out var list)){
          list=new List<NodeMaterialTable>();
          tempMaterialTables[channel]=list;
         }
         return list;
        }
        List<BiomeSpawnTable>GetOrCreateSpawnList(NoiseChannel channel){
         if(!tempSpawnTables.TryGetValue(channel,out var list)){
          list=new List<BiomeSpawnTable>();
          tempSpawnTables[channel]=list;
         }
         return list;
        }
        internal virtual void SetModule(NoiseChannel channel,ModuleBase module){
         this.modules[channel]=module;
        }
        internal virtual ModuleBase GetModule(NoiseChannel channel){
         modules.TryGetValue(channel,out var m);
         return m;
        }
        internal virtual double GetValue(NoiseChannel channel,Vector3 noiseInput){
         var m=GetModule(channel);
         if(m==null)return 0;
         return m.GetValue(noiseInput.x,noiseInput.y,noiseInput.z);
        }
        internal virtual void DiscoverChannels(){
         foreach(var k in tempMaterialTables.Keys)inheritedChannels.Add(k);
         foreach(var k in tempSpawnTables.Keys)   inheritedChannels.Add(k);
         foreach(var k in modules.Keys)           inheritedChannels.Add(k);
        }
        internal virtual void Propagation(){
         foreach(var channel in inheritedChannels){
          MergeMaterialTables(channel);
          MergeSpawnTables(channel);
         }
        }
        internal virtual void MergeMaterialTables(NoiseChannel channel){
         if(tempMaterialTables.TryGetValue(channel,out var list)&&list.Count>0){
          var materialTable=MaterialTablesSnapshot.pool.Rent();
          materialTable.DoSnapshot(list);
          materialTables[channel]=materialTable;
         }
         ClearTempMaterialTables(channel);
        }
        internal virtual void MergeSpawnTables(NoiseChannel channel){
         if(tempSpawnTables.TryGetValue(channel,out var list)&&list.Count>0){
          var spawnTable=BiomeSpawnTablesSnapshot.pool.Rent();
          var spawnSettingsSnapshot=spawnTable.DoSnapshot(list);
          spawnTables[channel]=spawnTable;
          if(spawnSettingsSnapshot!=null){
           GetOrCreateSpawnSettings(channel).MergeFrom(spawnSettingsSnapshot);
           SpawnSettingsSnapshot.pool.Return(spawnSettingsSnapshot);
          }
         }
         ClearTempSpawnTables(channel);
        }
        protected SpawnSettingsSnapshot GetOrCreateSpawnSettings(NoiseChannel channel){
         if(!spawnSettings.TryGetValue(channel,out var s)){
          s=SpawnSettingsSnapshot.pool.Rent();
          spawnSettings[channel]=s;
         }
         return s;
        }
        internal void MergeSpawnSettings(NoiseChannel channel,SpawnSettingsSnapshot target){
         target.MergeFrom(spawnSettings[channel]);
         SpawnSettingsSnapshot.pool.Return(spawnSettings[channel]);
         spawnSettings.Remove(channel);
        }
        internal virtual void Resolve(NoiseChannel channel,Vector3 noiseInput,out MaterialTablesSnapshot table){
         materialTables.TryGetValue(channel,out table);
        }
        internal virtual void Resolve(NoiseChannel channel,Vector3 noiseInput,out BiomeSpawnTablesSnapshot table){
         spawnTables.TryGetValue(channel,out table);
        }
        internal virtual void GetSpawnSettings(NoiseChannel channel,out SpawnSettingsSnapshot spawnSettingsSnapshot){
         Logs.Debug(()=>"spawnSettings:"+spawnSettings.Count+";channels:"+inheritedChannels.Count);
         spawnSettings.TryGetValue(channel,out spawnSettingsSnapshot);
        }
    }
    internal class GraphNodesSnapshot:NoiseNodesSnapshot{
     internal readonly Dictionary<NoiseChannel,NoiseNodesSnapshot>branches=new();
        internal override void OnReturnToPoolRecycle(){
         foreach(var kvp in branches){
          var branch=kvp.Value;
          Return(branch.GetType(),branch);
         }
         branches.Clear();
         base.OnReturnToPoolRecycle();
        }
        internal override void DiscoverChannels(){
         foreach(var kvp in branches){
          kvp.Value.DiscoverChannels();
          foreach(var ch in kvp.Value.inheritedChannels)
           inheritedChannels.Add(ch);
         }
        }
        internal override void Propagation(){
         foreach(var kvp in branches){
          kvp.Value.Propagation();
         }
         foreach(var channel in inheritedChannels){
          foreach(var kvp in branches){
           var branch=kvp.Value;
           branch.GetSpawnSettings(channel,out var spawnSettingsSnapshot);
           if(spawnSettingsSnapshot!=null){
            GetOrCreateSpawnSettings(channel).MergeFrom(spawnSettingsSnapshot);
           }
          }
         }
        }
        internal override void Resolve(NoiseChannel channel,Vector3 noiseInput,out   MaterialTablesSnapshot table){
         table=null;
         if(branches.TryGetValue(channel,out var branch)){
          branch.Resolve(channel,noiseInput,out table);
         }
        }
        internal override void Resolve(NoiseChannel channel,Vector3 noiseInput,out BiomeSpawnTablesSnapshot table){
         table=null;
         foreach(var branch in branches.Values){
          branch.Resolve(channel,noiseInput,out BiomeSpawnTablesSnapshot t);
          if(t!=null){
           table=t;
           return;
          }
         }
        }
    }
    internal class OperatorNoiseNodesSnapshot:NoiseNodesSnapshot{
        internal override void OnReturnToPoolRecycle(){
         NoiseNodesSnapshot.Return(input.GetType(),input);input=null;
         base.OnReturnToPoolRecycle();
        }
     protected NoiseNodesSnapshot input;
        internal virtual void SetInput(NoiseNodesSnapshot input){
         this.input=input;
         ClearTempTables();
        }
        internal override void DiscoverChannels(){
         input.DiscoverChannels();
         foreach(var ch in input.inheritedChannels)
          inheritedChannels.Add(ch);
        }
        internal override void Propagation(){
         input.Propagation();
         foreach(var channel in inheritedChannels){
          input.GetSpawnSettings(channel,out var spawnSettingsSnapshot);
          if(spawnSettingsSnapshot!=null){
           GetOrCreateSpawnSettings(channel).MergeFrom(spawnSettingsSnapshot);
          }
         }
        }
        internal override void Resolve(NoiseChannel channel,Vector3 noiseInput,out   MaterialTablesSnapshot table){
         input.Resolve(channel,noiseInput,out table);
        }
        internal override void Resolve(NoiseChannel channel,Vector3 noiseInput,out BiomeSpawnTablesSnapshot table){
         input.Resolve(channel,noiseInput,out table);
        }
    }
    internal class SelectorNoiseNodesSnapshot:NoiseNodesSnapshot{
        internal override void OnReturnToPoolRecycle(){
         NoiseNodesSnapshot.Return(inputA.GetType(),inputA);inputA=null;
         NoiseNodesSnapshot.Return(inputB.GetType(),inputB);inputB=null;
         NoiseNodesSnapshot.Return(controller.GetType(),controller);controller=null;
         base.OnReturnToPoolRecycle();
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
        internal override void DiscoverChannels(){
         inputA.DiscoverChannels();
         inputB.DiscoverChannels();
         foreach(var ch in inputA.inheritedChannels)
          inheritedChannels.Add(ch);
         foreach(var ch in inputB.inheritedChannels)
          inheritedChannels.Add(ch);
        }
        internal override void Propagation(){
         inputA.Propagation();
         inputB.Propagation();
         controller.ClearTempTables();
         foreach(var channel in inheritedChannels){
          var target=GetOrCreateSpawnSettings(channel);
          inputA.GetSpawnSettings(channel,out var spawnSettingsSnapshotA);
          inputB.GetSpawnSettings(channel,out var spawnSettingsSnapshotB);
          if(spawnSettingsSnapshotA!=null)target.MergeFrom(spawnSettingsSnapshotA);
          if(spawnSettingsSnapshotB!=null)target.MergeFrom(spawnSettingsSnapshotB);
         }
        }
        internal override void Resolve(NoiseChannel channel,Vector3 noiseInput,out   MaterialTablesSnapshot table){
         var input=OnResolveGetNode(NoiseChannel.Terrain,noiseInput);
         input.Resolve(channel,noiseInput,out table);
        }
        internal override void Resolve(NoiseChannel channel,Vector3 noiseInput,out BiomeSpawnTablesSnapshot table){
         var input=OnResolveGetNode(NoiseChannel.Terrain,noiseInput);
         input.Resolve(channel,noiseInput,out table);
        }
        private NoiseNodesSnapshot OnResolveGetNode(NoiseChannel channel,Vector3 noiseInput){
         var cv=controller.GetValue(channel,noiseInput);
         var select=(Select)GetModule(channel);
         var max=select.Maximum;
         var min=select.Minimum;
         if(cv<min||cv>max){
          return inputA;
         }
         return inputB;
        }
    }
    internal class MultiplierNoiseNodesSnapshot:NoiseNodesSnapshot{
        internal override void OnReturnToPoolRecycle(){
         NoiseNodesSnapshot.Return(lhs.GetType(),lhs);lhs=null;
         NoiseNodesSnapshot.Return(rhs.GetType(),rhs);rhs=null;
         base.OnReturnToPoolRecycle();
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
        internal override void DiscoverChannels(){
         lhs.DiscoverChannels();
         rhs.DiscoverChannels();
         foreach(var ch in lhs.inheritedChannels)
          inheritedChannels.Add(ch);
         foreach(var ch in rhs.inheritedChannels)
          inheritedChannels.Add(ch);
        }
        internal override void Propagation(){
         lhs.Propagation();
         rhs.Propagation();
         foreach(var channel in inheritedChannels){
          lhs.GetSpawnSettings(channel,out var l);
          lhs.GetSpawnSettings(channel,out var r);
          if(l!=null)
           GetOrCreateSpawnSettings(channel).MergeFrom(l);
          if(r!=null)
           GetOrCreateSpawnSettings(channel).MergeFrom(r);
         }
         rhs.ClearTempTables();
        }
        internal override void Resolve(NoiseChannel channel,Vector3 noiseInput,out   MaterialTablesSnapshot table){
         lhs.Resolve(channel,noiseInput,out table);
        }
        internal override void Resolve(NoiseChannel channel,Vector3 noiseInput,out BiomeSpawnTablesSnapshot table){
         lhs.Resolve(channel,noiseInput,out table);
        }
    }
    internal class MaterialTablesSnapshot{
     internal static readonly Utilities.ObjectPool<MaterialTablesSnapshot>pool=
      Pool.GetPool<MaterialTablesSnapshot>(
       "",
       ()=>new(),
       (MaterialTablesSnapshot item)=>{
        item.OnReturnToPoolRecycle();
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
        internal virtual void OnReturnToPoolRecycle(){
         baseMaterial=default;
        }
    }
    internal class BiomeSpawnTablesSnapshot{
     internal static readonly Utilities.ObjectPool<BiomeSpawnTablesSnapshot>pool=
      Pool.GetPool<BiomeSpawnTablesSnapshot>(
       "",
       ()=>new(),
       (BiomeSpawnTablesSnapshot item)=>{
        item.OnReturnToPoolRecycle();
       }
      );
     internal readonly Dictionary<int,
      ByChancePicker<SimObject>
     >pickerByLayer=new();
        internal virtual SpawnSettingsSnapshot DoSnapshot(List<BiomeSpawnTable>tempSpawnTables){
         SpawnSettingsSnapshot spawnSettingsSnapshot=null;
         foreach(var spawnTable in tempSpawnTables){
          if(spawnSettingsSnapshot==null){
           spawnSettingsSnapshot=SpawnSettingsSnapshot.pool.Rent();
          }
          foreach(var spawnTableLayer in spawnTable.layers){
           int layer=spawnTableLayer.layer;
           var spawnLayerData=new SnapshotSpawnLayerData(){
            gridSize=spawnTableLayer.gridSize,
           };
           if(!pickerByLayer.TryGetValue(layer,out var picker)){
            pickerByLayer.Add(layer,picker=new());
           }
           if(layer<spawnSettingsSnapshot.minLayer){
            spawnSettingsSnapshot.minLayer=layer;
           }
           if(layer>spawnSettingsSnapshot.maxLayer){
            spawnSettingsSnapshot.maxLayer=layer;
           }
           foreach(var entry in spawnTableLayer.entries){
            Logs.Debug(()=>entry.prefab!=null?"'spawnTable entry':"+entry.prefab.name:"'spawnTable entry':null");
            ByChanceObjectSpawnEntry<SimObject>pickerEntry=ByChanceObjectSpawnEntry<SimObject>.pool.Rent();
            pickerEntry.prefab=entry.prefab;
            pickerEntry.chance=entry.chance;
            if(pickerEntry.prefab.meshPrefab!=null){
             var meshPrefab=pickerEntry.prefab.meshPrefab;
             var prefabMeshRenderer=meshPrefab.GetComponent<MeshRenderer>();
             var prefabMeshFilter  =meshPrefab.GetComponent<MeshFilter  >();
             if(prefabMeshRenderer!=null&&prefabMeshFilter!=null){
              var sharedMesh=prefabMeshFilter.sharedMesh;
              Logs.Debug(()=>"prefab sharedMesh:"+sharedMesh);
              if(sharedMesh!=null){
               var localBounds=sharedMesh.bounds;
               Logs.Debug(()=>"prefab localBounds:"+localBounds);
               var bounds=localBounds;
               bounds.center=Vector3.Scale(bounds.center,meshPrefab.transform.localScale);
               bounds.size  =Vector3.Scale(bounds.size  ,meshPrefab.transform.localScale);
               Logs.Debug(()=>"prefab bounds:"+bounds);
               spawnLayerData.maxBoundsSize=Vector3.Max(spawnLayerData.maxBoundsSize,bounds.size);
               pickerEntry.bounds=bounds;
              }
             }
            }
            pickerEntry.variations=new(entry.variations);
            picker.items.Add(pickerEntry);
           }
           spawnSettingsSnapshot.layerData[layer]=spawnLayerData;
          }
         }
         foreach(var kvp in pickerByLayer){
          var picker=kvp.Value;
          picker.Build();
         }
         return spawnSettingsSnapshot;
        }
        internal virtual void OnReturnToPoolRecycle(){
         foreach(var kvp in pickerByLayer){
          var picker=kvp.Value;
          picker.Clear();
         }
        }
    }
}