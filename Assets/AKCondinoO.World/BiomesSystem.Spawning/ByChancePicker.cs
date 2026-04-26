using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using LibNoise;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static AKCondinoO.World.Biomes.BiomeSpawnEntry;
using static AKCondinoO.World.BiomesConfigurationSnapshot;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.Spawning{
    internal class ByChanceObjectSpawnEntry<T>where T:class{
     internal static readonly Utilities.ObjectPool<ByChanceObjectSpawnEntry<T>>pool=
      Pool.GetPool<ByChanceObjectSpawnEntry<T>>(
       "",
       ()=>new(),
       (ByChanceObjectSpawnEntry<T>item)=>{
        item.prefab=null;
       }
      );
     internal T prefab;
     internal float chance;
     internal int priority;
     internal Bounds bounds;
     internal SpawnVariationsSnapshot variations;
        internal class SpawnVariationsSnapshot{
         internal bool alignToTerrain;
         internal Vector3 rotMin,rotMax;
         internal Vector3 scaleMin;
         internal Vector3 scaleMax;
            internal SpawnVariationsSnapshot(SpawnVariations variations){
             rotMin=variations.rotMin;rotMax=variations.rotMax;
             scaleMin=variations.scaleMin;
             scaleMax=variations.scaleMax;
             alignToTerrain=variations.alignToTerrain;
            }
            internal SpawnVariation Get(ModuleBase module,Vector3 noiseInput){
             uint rotationHash=math.hash(new int4((int)noiseInput.x,(int)noiseInput.y,(int)noiseInput.z,1));
             uint    scaleHash=math.hash(new int4((int)noiseInput.x,(int)noiseInput.y,(int)noiseInput.z,2));
             float rotationNoise=NormalizeHeight((float)module.GetValue(noiseInput.z,noiseInput.x,rotationHash),Height);
             float    scaleNoise=NormalizeHeight((float)module.GetValue(noiseInput.z,noiseInput.x,   scaleHash),Height);
             //Logs.Debug(()=>"rotationNoise:"+rotationNoise+";scaleNoise:"+scaleNoise);
             var variation=new SpawnVariation(){
              alignToTerrain=alignToTerrain,
              rot=Vector3.Lerp(rotMin,rotMax,rotationNoise),
              scale=Vector3.Lerp(scaleMin,scaleMax,scaleNoise),
             };
             return variation;
            }
        }
        internal struct SpawnVariation{
         internal bool alignToTerrain;
         internal Vector3 rot;
         internal Vector3 scale;
        }
    }
    internal class ByChancePicker<T>where T:class{
     internal readonly List<ByChanceObjectSpawnEntry<T>>items=new();
     private readonly List<ByChanceObjectSpawnEntry<T>>filteredItems=new();
     private ByChanceObjectSpawnEntry<T>[]lookupTable;
     private int resolution;
        internal void Build(int resolution=100_000,bool fill=false){
         this.resolution=resolution;
         if(items.Count<=0){
          lookupTable=Pool.RentArray<ByChanceObjectSpawnEntry<T>>(0);
          return;
         }
         filteredItems.Clear();
         foreach(var item in items){
          if(item.chance>0f)filteredItems.Add(item);
         }
         if(filteredItems.Count<=0){
          lookupTable=Pool.RentArray<ByChanceObjectSpawnEntry<T>>(0);
          return;
         }
         filteredItems.Sort((a,b)=>{
          int cmp=b.chance.CompareTo(a.chance);
          return cmp!=0?cmp:b.priority.CompareTo(a.priority);
         });
         float total=0f;
         if(fill){foreach(var item in filteredItems)total+=item.chance;}else{total=1f;}
         lookupTable=Pool.RentArray<ByChanceObjectSpawnEntry<T>>(resolution);
         float cumulative=0f;
         for(int i=0;i<filteredItems.Count;i++){
          var obj=filteredItems[i];
          int startIdx=(int)(cumulative/total*resolution);
          cumulative+=obj.chance;
          int endIdx=(int)(Math.Min(cumulative/total,1f)*resolution);
          for(int idx=startIdx;idx<endIdx;idx++){
           lookupTable[idx]=obj;
          }
         }
        }
        internal void Clear(){
         Pool.ReturnArray(lookupTable,true);
         lookupTable=null;
         foreach(var item in items){
          ByChanceObjectSpawnEntry<T>.pool.Return(item);
         }
         items.Clear();
         filteredItems.Clear();
        }
        internal bool Get(float value,out ByChanceObjectSpawnEntry<T>result){
         result=null;
         if(value<0f||value>1f)return false;
         int idx=(int)(value*resolution);
         if(idx>=lookupTable.Length)return false;
         if(idx>=resolution)idx=resolution-1;
         result=lookupTable[idx];
         if(result==null)return false;
         return true;
        }
    }
}