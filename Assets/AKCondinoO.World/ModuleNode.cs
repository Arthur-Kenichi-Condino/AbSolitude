using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using AKCondinoO.World.Spawning;
using LibNoise;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    internal abstract class ModuleNode:ScriptableObject{
     public NoiseChannel channel;
     public int seedOffset;
     public BiomeMaterialTable[]materialTables;
     public BiomeSpawnTable[]biomeSpawnTables;
        internal virtual NoiseNodesSnapshot DoSnapshot(int worldSeed,NoiseNodesSnapshot parent){
         var snapshot=CreateSnapshot();
         snapshot.channel=channel;
         snapshot.module=CreateModule(worldSeed,snapshot);
         snapshot.parent=parent;
         snapshot.DoCollection(this);
         return snapshot;
        }
        protected virtual NoiseNodesSnapshot CreateSnapshot(){
         NoiseNodesSnapshot snapshot=NoiseNodesSnapshot.Rent(typeof(NoiseNodesSnapshot));
         return snapshot;
        }
        protected abstract ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot);
        protected static int SeedHash(int seed,int id){
         unchecked{
          int h=seed;
          h=h*374761393+id;
          h=(h<<13)^h;
          return h*1274126177;
         }
        }
    }
    internal class NoiseNodesSnapshot{
     static readonly Dictionary<(Type,string),ObjectPoolBase>noiseNodesSnapshotPool=new(){
      {(typeof(          NoiseNodesSnapshot),""),Pool.GetPool<          NoiseNodesSnapshot>("",()=>new(),(          NoiseNodesSnapshot item)=>{item.OnReturnToPoolRecycle();},false)},
      {(typeof(  OperatorNoiseNodesSnapshot),""),Pool.GetPool<  OperatorNoiseNodesSnapshot>("",()=>new(),(  OperatorNoiseNodesSnapshot item)=>{item.OnReturnToPoolRecycle();},false)},
      {(typeof(  SelectorNoiseNodesSnapshot),""),Pool.GetPool<  SelectorNoiseNodesSnapshot>("",()=>new(),(  SelectorNoiseNodesSnapshot item)=>{item.OnReturnToPoolRecycle();},false)},
      {(typeof(MultiplierNoiseNodesSnapshot),""),Pool.GetPool<MultiplierNoiseNodesSnapshot>("",()=>new(),(MultiplierNoiseNodesSnapshot item)=>{item.OnReturnToPoolRecycle();},false)},
     };
        internal static NoiseNodesSnapshot Rent(Type poolId){
         return(NoiseNodesSnapshot)noiseNodesSnapshotPool[(poolId,"")].ObjectRent();
        }
        internal static void Return(Type poolId,NoiseNodesSnapshot nodesSnapshot){
         noiseNodesSnapshotPool[(poolId,"")].ObjectReturn(nodesSnapshot);
        }
     internal NoiseChannel channel;
     internal NoiseNodesSnapshot parent;
     internal ModuleBase module;
     protected readonly List<BiomeMaterialTable>collectedMaterialTables=new();
     internal SnapshotMaterialTable mergedMaterialTable;
     protected readonly List<BiomeSpawnTable>collectedBiomeSpawnTables=new();
     internal SnapshotBiomeSpawnTable mergedBiomeSpawnTable;
        internal virtual void OnReturnToPoolRecycle(){
         channel=NoiseChannel.None;
         parent=null;
         module.Dispose();module=null;
         collectedMaterialTables.Clear();
         SnapshotMaterialTable.pool.Return(mergedMaterialTable);mergedMaterialTable=null;
         collectedBiomeSpawnTables.Clear();
         SnapshotBiomeSpawnTable.pool.Return(mergedBiomeSpawnTable);mergedBiomeSpawnTable=null;
         SnapshotSpawnSettings.pool.Return(spawnSettings);spawnSettings=null;
        }
        internal virtual void DoCollection(ModuleNode moduleNode){
         if(parent!=null){
          collectedMaterialTables.AddRange(parent.collectedMaterialTables);
          collectedBiomeSpawnTables.AddRange(parent.collectedBiomeSpawnTables);
         }
         collectedMaterialTables.AddRange(moduleNode.materialTables);
         collectedBiomeSpawnTables.AddRange(moduleNode.biomeSpawnTables);
        }
        internal virtual void MergeCollection(){
         MergeMaterialTables();
         MergeBiomeSpawnTables();
        }
        protected virtual void MergeMaterialTables(){
         mergedMaterialTable=SnapshotMaterialTable.pool.Rent();
         mergedMaterialTable.DoSnapshot(collectedMaterialTables);
         collectedMaterialTables.Clear();
        }
        protected virtual void MergeBiomeSpawnTables(){
         mergedBiomeSpawnTable=SnapshotBiomeSpawnTable.pool.Rent();
         spawnSettings=mergedBiomeSpawnTable.DoSnapshot(collectedBiomeSpawnTables);
         collectedBiomeSpawnTables.Clear();
        }
     internal SnapshotSpawnSettings spawnSettings;
        internal virtual void SettingsPropagation(){
        }
        internal void PropagateSpawnSettings(SnapshotSpawnSettings target){
         target.MergeFrom(spawnSettings);
         SnapshotSpawnSettings.pool.Return(spawnSettings);spawnSettings=null;
        }
        internal virtual SnapshotMaterialTable GetMaterialTable(Vector3 noiseInput){
         return mergedMaterialTable;
        }
        internal virtual SnapshotBiomeSpawnTable GetBiomeSpawnTable(Vector3 noiseInput){
         return mergedBiomeSpawnTable;
        }
    }
    internal class SnapshotMaterialTable{
     internal static readonly Utilities.ObjectPool<SnapshotMaterialTable>pool=
      Pool.GetPool<SnapshotMaterialTable>(
       "",
       ()=>new(),
       (SnapshotMaterialTable item)=>{
        item.OnReturnToPoolRecycle();
       }
      );
     internal MaterialId baseMaterial;
        internal virtual void OnReturnToPoolRecycle(){
         baseMaterial=default;
        }
        internal virtual void DoSnapshot(List<BiomeMaterialTable>tempMaterialTables){
         foreach(var materialTable in tempMaterialTables){
          foreach(var entry in materialTable.entries){
           baseMaterial=(MaterialId)Math.Max((uint)entry.material,(uint)baseMaterial);
          }
         }
        }
    }
    internal class SnapshotBiomeSpawnTable{
     internal static readonly Utilities.ObjectPool<SnapshotBiomeSpawnTable>pool=
      Pool.GetPool<SnapshotBiomeSpawnTable>(
       "",
       ()=>new(),
       (SnapshotBiomeSpawnTable item)=>{
        item.OnReturnToPoolRecycle();
       }
      );
     internal readonly Dictionary<int,
      ByChancePicker<SimObject>
     >pickerByLayer=new();
        internal virtual void OnReturnToPoolRecycle(){
         foreach(var kvp in pickerByLayer){
          var picker=kvp.Value;
          picker.Clear();
         }
        }
        internal virtual SnapshotSpawnSettings DoSnapshot(List<BiomeSpawnTable>tempSpawnTables){
         SnapshotSpawnSettings spawnSettingsSnapshot=null;
         foreach(var spawnTable in tempSpawnTables){
          if(spawnSettingsSnapshot==null){
           spawnSettingsSnapshot=SnapshotSpawnSettings.pool.Rent();
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
    }
    internal class SnapshotSpawnSettings{
     internal static readonly Utilities.ObjectPool<SnapshotSpawnSettings>pool=
      Pool.GetPool<SnapshotSpawnSettings>(
       "",
       ()=>new(),
       (SnapshotSpawnSettings item)=>{
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
        internal void MergeFrom(SnapshotSpawnSettings other){
         if(other==null)return;
         minLayer=Math.Min(minLayer,other.minLayer);
         maxLayer=Math.Max(maxLayer,other.maxLayer);
         foreach(var kvp in other.layerData){
          if(layerData.TryGetValue(kvp.Key,out var data)){
           data.Merge(kvp.Value);
           layerData[kvp.Key]=data;
          }else{
           layerData[kvp.Key]=kvp.Value;
          }
         }
        }
    }
    internal struct SnapshotSpawnLayerData{
     internal int gridSize;
     internal Vector3 maxBoundsSize;
        internal void Merge(SnapshotSpawnLayerData other){
         gridSize=Math.Max(gridSize,other.gridSize);
         maxBoundsSize=Vector3.Max(maxBoundsSize,other.maxBoundsSize);
        }
    }
}