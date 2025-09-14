#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Trees;
using LibNoise.Generator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using static AKCondinoO.Voxels.Biomes.BiomeSettings;
namespace AKCondinoO.Voxels.Biomes{
    internal class BaseBiomeSimObjectsSpawnSettings{
     internal BiomeBehaviour biomeBehaviour{get{if(VoxelSystem.singleton!=null){return VoxelSystem.singleton.biomeBehaviour;}return null;}}
        internal enum SpawnedTypes:int{
         All=0,
         Grass=1,
         Trees=2,
         Bushes=3,
        }
        internal struct SimObjectSettings{
         internal Vector3 size;
         internal float chance;
         public int priority;
         internal float inclination;
         internal Vector3 assetScale;
         internal Vector3 minScale;
         internal Vector3 maxScale;
         internal Vector2 rotation;
         internal float depth;
         internal readonly ReadOnlyDictionary<SpawnedTypes,Vector3>minSpacing;
         internal readonly ReadOnlyDictionary<SpawnedTypes,Vector3>maxSpacing;
         internal readonly ReadOnlyCollection<SpawnedTypes>blocksTypes;
         internal readonly ReadOnlyCollection<SpawnedTypes>isBlockedBy;
            internal SimObjectSettings(Vector3 size,float chance,int priority,float inclination,Vector3 assetScale,Vector3 minScale,Vector3 maxScale,Vector2 rotation,float depth,Dictionary<SpawnedTypes,Vector3>minSpacing,Dictionary<SpawnedTypes,Vector3>maxSpacing,SpawnedTypes[]blocksTypes,SpawnedTypes[]isBlockedBy){
             this.size=size;
             this.chance=chance;
             this.priority=priority;
             this.inclination=inclination;
             this.assetScale=assetScale;
             this.minScale=minScale;
             this.maxScale=maxScale;
             this.rotation=rotation;
             this.depth=depth;
             this.minSpacing=new ReadOnlyDictionary<SpawnedTypes,Vector3>(minSpacing);
             this.maxSpacing=new ReadOnlyDictionary<SpawnedTypes,Vector3>(maxSpacing);
             this.blocksTypes=new ReadOnlyCollection<SpawnedTypes>(blocksTypes);
             this.isBlockedBy=new ReadOnlyCollection<SpawnedTypes>(isBlockedBy);
            }
        }
        internal struct SimObjectSpawnModifiers{
         internal Vector3 scale;
         internal float rotation;
        }
        //internal class WeightedObject<T>{
        // internal T value;
        // internal float chance;//  de 0 a 1
        // internal int priority;//  prioridade em empate
        //}
        //internal class WeightedPicker<T>{
        // internal readonly T[]lookupTable;
        // internal readonly int resolution;
        //    internal WeightedPicker(List<WeightedObject<T>>items,int resolution=100_000){
        //     if(items==null||items.Count==0){
        //      Log.Error("WeightedPicker:lista de itens não pode ser vazia.");
        //      return;
        //     }
        //     this.resolution=resolution;
        //     lookupTable=new T[resolution];
        //     //  Ordena para garantir prioridade em empate
        //     items.Sort((a,b)=>{
        //      int cmp=b.priority.CompareTo(a.priority);//  prioridade maior primeiro
        //      if(cmp==0)return 0;//  mantém ordem original em empate real
        //      return cmp;
        //     });
        //     //  Normaliza soma dos pesos
        //     float totalChance=0f;
        //     foreach(var item in items)totalChance+=item.chance;
        //     if(totalChance<=0f){
        //      Log.Error("WeightedPicker:soma das chances deve ser > 0.");
        //      return;
        //     }
        //     //  Preenche tabela
        //     float cumulative=0f;
        //     int index=0;
        //     foreach(var item in items){
        //      cumulative+=item.chance/totalChance;
        //      int endIndex=(int)(cumulative*resolution);
        //      //  Garantir limites corretos
        //      if(endIndex>resolution)
        //       endIndex=resolution;
        //      for(;index<endIndex;index++)
        //       lookupTable[index]=item.value;
        //     }
        //     //  Preencher qualquer sobra (por arredondamento)
        //     for(;index<resolution;index++)
        //      lookupTable[index]=items[items.Count-1].value;
        //    }
        //    internal bool Get(float value,out T result){
        //     if(value<0f)value=0f;
        //     if(value>1f)value=1f;
        //     int idx=(int)(value*resolution);
        //     if(idx>=resolution)idx=resolution-1;
        //     result=lookupTable[idx];
        //     return true;
        //    }
        //}
        //internal class ByChanceObject<T>{
        // internal T value;
        // internal float chance;//  0 < Chance <= 1
        // internal int priority;//  desempate
        //}
        //internal class ByChancePicker<T>{
        // private readonly Dictionary<int,T>lookupTable=new Dictionary<int,T>();
        // private readonly float maxWeight;
        // private readonly int resolution; // tamanho da tabela virtual
        // private readonly bool isStruct;
        //    internal ByChancePicker(List<ByChanceObject<T>>items,int resolution=100000){
        //     if(items==null){
        //      Log.Error(nameof(items));
        //      return;
        //     }
        //     this.resolution=resolution;
        //     isStruct=typeof(T).IsValueType;
        //     //  Ordena por peso decrescente, depois prioridade decrescente
        //     ByChanceObject<T>[]sortedItems=items
        //      .OrderByDescending(x=>x.chance)
        //      .ThenByDescending(x=>x.priority)
        //      .ToArray();
        //     if(sortedItems.Length==0){
        //      Log.Error("items cannot be empty");
        //      return;
        //     }
        //     maxWeight=sortedItems.Max(x=>x.chance);
        //     //  Construir lookupTable usando resolução
        //     float cumulative=0f;
        //     foreach(var entry in sortedItems){
        //      int startIdx=(int)(cumulative*resolution);
        //      cumulative+=entry.chance;
        //      int endIdx=(int)(Math.Min(cumulative,1f)*resolution);
        //      for(int i=startIdx;i<endIdx;i++){
        //       lookupTable[i]=entry.value;
        //      }
        //     }
        //    }
        //    internal bool Get(float value,out T result){
        //     result=default;
        //     if(value<0f||value>1f){
        //      Log.Error(nameof(value));
        //      return false;
        //     }
        //     if(value>maxWeight)return false;//  null para reference ou default(T?) para struct
        //     int idx=(int)(value*resolution);
        //     if(lookupTable.TryGetValue(idx,out result)){
        //      return true;
        //     }
        //     return false;
        //    }
        //}
internal class ByChanceObject<T>
{
    internal T value;
    internal float chance; // 0 < chance <= 1
    internal int priority; // desempate
}

internal class ByChancePicker<T>
{
    private readonly T[] lookupTable;
    private readonly float maxWeight;
    private readonly int resolution;

    internal ByChancePicker(List<ByChanceObject<T>> items, int resolution = 100_000)
    {
        if (items == null || items.Count == 0)
        {
            lookupTable = new T[0];
            maxWeight = 0f;
            this.resolution = resolution;
            return;
        }

        this.resolution = resolution;

        // Filtra itens com chance <= 0
        var filteredItems = new List<ByChanceObject<T>>();
        foreach (var item in items)
        {
            if (item.chance > 0f)
                filteredItems.Add(item);
        }

        if (filteredItems.Count == 0)
        {
            lookupTable = new T[0];
            maxWeight = 0f;
            return;
        }

        // Ordena por chance decrescente e prioridade decrescente
        filteredItems.Sort((a, b) =>
        {
            int cmp = b.chance.CompareTo(a.chance);
            return cmp != 0 ? cmp : b.priority.CompareTo(a.priority);
        });

        // Calcula soma total
        float total = 0f;
        foreach (var item in filteredItems) total += item.chance;
        maxWeight = filteredItems[0].chance;

        // Cria lookupTable
        lookupTable = new T[resolution];
        float cumulative = 0f;
        for (int i = 0; i < filteredItems.Count; i++)
        {
            var obj = filteredItems[i];
            int startIdx = (int)(cumulative / total * resolution);
            cumulative += obj.chance;
            int endIdx = (int)(Math.Min(cumulative / total, 1f) * resolution);

            for (int idx = startIdx; idx < endIdx; idx++)
            {
                lookupTable[idx] = obj.value;
            }
        }
    }

    internal bool Get(float value, out T result)
    {
        result = default;

        if (value < 0f || value > 1f) return false;
        if (value * resolution >= lookupTable.Length) return false; // acima da chance máxima

        if (lookupTable.Length == 0) return false;

        int idx = (int)(value * resolution);
        if (idx >= lookupTable.Length) idx = lookupTable.Length - 1;

        result = lookupTable[idx];
        if (EqualityComparer<T>.Default.Equals(result, default))
            return false;

        return true;
    }
}
     internal class SpawnPickingLayer{
      public int layer;
      public Vector3 maxDimensions=Vector3.zero;
      public readonly Dictionary<int,ByChancePicker<(Type simObject,SimObjectSettings settings)>>simObjectPicking=new();
       public readonly Dictionary<int,List<ByChanceObject<(Type,SimObjectSettings)>>>simObjectPickingItems=new();
     }
     readonly protected Dictionary<int,SpawnPickingLayer>simObjectPickingByLayer=new();
     //readonly protected Dictionary<int,ByChancePicker<(Type simObject,SimObjectSettings settings)>>simObjectPicking=new();
      //readonly Dictionary<int,List<ByChanceObject<(Type,SimObjectSettings)>>>simObjectPickingItems=new();
     readonly protected Dictionary<Type,Dictionary<int,List<SimObjectSettings>>>allSettings=new Dictionary<Type,Dictionary<int,List<SimObjectSettings>>>();
     //readonly protected Dictionary<int,int>settingsCountForSelection=new Dictionary<int,int>();
     readonly BaseBiome biome;
     //internal static Vector3 maxSpawnSize=new Vector3(50f,256f,50f);
     //internal static Vector3 margin      =new Vector3(  1f,  1f,  1f);
        internal BaseBiomeSimObjectsSpawnSettings(BaseBiome biome){
         this.biome=biome;
        }
        internal void Set(){
         foreach(var layerSpawnPickingPair in simObjectPickingByLayer){
          SpawnPickingLayer pickingLayer=layerSpawnPickingPair.Value;
          foreach(var kvp in pickingLayer.simObjectPickingItems){
           kvp.Value.Clear();
          }
          pickingLayer.maxDimensions=Vector3.zero;
         }
         for(int i=0;i<biomeBehaviour.settings.biomeSurfaceSpawnsByLayer.Length;++i){
          SurfaceSpawnLayer surfaceSpawnLayer=biomeBehaviour.settings.biomeSurfaceSpawnsByLayer[i];
          int layer=surfaceSpawnLayer.layer;
          if(!simObjectPickingByLayer.TryGetValue(layer,out SpawnPickingLayer pickingLayer)){
           simObjectPickingByLayer.Add(layer,pickingLayer=new());
          }
          pickingLayer.layer=layer;
          Vector3 maxDimensions=pickingLayer.maxDimensions;
          foreach(SurfaceSpawn surfaceSpawnSetting in surfaceSpawnLayer.biomeSurfaceSpawns){
           if(surfaceSpawnSetting.chance<=0){continue;}
           maxDimensions.x=Mathf.Max(surfaceSpawnSetting.size.x,maxDimensions.x);
           maxDimensions.y=Mathf.Max(surfaceSpawnSetting.size.y,maxDimensions.y);
           maxDimensions.z=Mathf.Max(surfaceSpawnSetting.size.z,maxDimensions.z);
           int picking=surfaceSpawnSetting.picking;
           if(!pickingLayer.simObjectPickingItems.TryGetValue(picking,out var itemsForPicking)){
            pickingLayer.simObjectPickingItems.Add(picking,itemsForPicking=new());
           }
           Type simObjectType=surfaceSpawnSetting.simObject.GetType();
           SimObjectSettings simObjectSettings=new(
            size:       surfaceSpawnSetting.size,
            chance:     surfaceSpawnSetting.chance,
            priority:   surfaceSpawnSetting.priority,
            inclination:surfaceSpawnSetting.inclination,
            assetScale: surfaceSpawnSetting.assetScale,
            minScale:   surfaceSpawnSetting.minScale,
            maxScale:   surfaceSpawnSetting.maxScale,
            rotation:   surfaceSpawnSetting.rotation,
            depth:      surfaceSpawnSetting.depth,
            minSpacing: surfaceSpawnSetting.minSpacing.ToDictionary(k=>k.spawnedType,v=>v.spacingDis),
            maxSpacing: surfaceSpawnSetting.minSpacing.ToDictionary(k=>k.spawnedType,v=>v.spacingDis),
            blocksTypes:surfaceSpawnSetting.blocksTypes,
            isBlockedBy:surfaceSpawnSetting.isBlockedBy
           );
           ByChanceObject<(Type,SimObjectSettings)>byChanceSimObject=new();
           byChanceSimObject.value   =(simObjectType,simObjectSettings);
           byChanceSimObject.chance  =simObjectSettings.chance;
           byChanceSimObject.priority=simObjectSettings.priority;
           itemsForPicking.Add(byChanceSimObject);
           Log.DebugMessage("spawn setting added for:"+simObjectType);
          }
          pickingLayer.maxDimensions=maxDimensions;
         }
         if(biomeBehaviour.useHardCodedSurfaceSpawnIfAvailable){
          #region hard-coded settings
          #endregion hard-coded settings
         }
         //Log.Warning("TO DO: rotacionar objeto para que não fique flutuando ao ser spawn'ado");
         //Log.Warning("TO DO: limitar tamanho de objetos aqui pelo tamanho máximo de spawn");
         foreach(var layerSpawnPickingPair in simObjectPickingByLayer){
          int layer=layerSpawnPickingPair.Key;
          SpawnPickingLayer pickingLayer=layerSpawnPickingPair.Value;
          foreach(var pickingItemsPair in pickingLayer.simObjectPickingItems){
           int picking=pickingItemsPair.Key;
           ByChancePicker<(Type simObject,SimObjectSettings settings)>pickList=new(pickingItemsPair.Value);
           pickingLayer.simObjectPicking[picking]=pickList;
          }
         }
        }
     internal Perlin simObjectSpawnChancePerlin;
        internal(Type simObject,SimObjectSettings simObjectSettings)?TryGetSettingsToSpawnSimObject(int layer,Vector3Int noiseInputRounded,out double selectionValue,out SpawnPickingLayer pickingLayer){

         //if(!((noiseInputRounded.x==1&&
         //      noiseInputRounded.z==1)||
         //     (noiseInputRounded.x==1&&
         //      noiseInputRounded.z==2))){
         // pickingLayer=null;
         // selectionValue=default;
         // return null;
         //}
            //if(cnkRgn1.x!=0||cnkRgn1.y!=0){
            // continue;
            //}
            //if(!(noiseInputRounded.x==1&&noiseInputRounded.z==1)&&!(noiseInputRounded.x==2&&noiseInputRounded.z==2)){
            //if(!(noiseInputRounded.x==noiseInputRounded.z)){
            // pickingLayer=null;
            // selectionValue=default;
            // return null;
            //}
            //if(noiseInputRounded.z!=1&&noiseInputRounded.z!=2){
            // pickingLayer=null;
            // selectionValue=default;
            // return null;
            //}

         if(!simObjectPickingByLayer.TryGetValue(layer,out pickingLayer)){
          selectionValue=default;
          return null;
         }
         Vector3 noiseInput=noiseInputRounded+biome.deround;
         int selection=biome.GetSelectorValue(noiseInput,out selectionValue);
         selection=1;//  debug
         if(pickingLayer.simObjectPicking.TryGetValue(selection,out var pickList)){
          float dicing=Mathf.Clamp01(((float)simObjectSpawnChancePerlin.GetValue(noiseInput.z,noiseInput.x,0)+1f)/2f);
          bool picked=pickList.Get(dicing,out var result);
          if(picked){
           SimObjectSettings settings=result.settings;
           Vector3 size=settings.size;
           int max=Mathf.CeilToInt(Mathf.Max(size.x,size.z)*1.05f);
           if(
            noiseInputRounded.x%max==0&&
            noiseInputRounded.z%max==0
           ){
            return result;
           }
           //return result;
          }
          //int count=0;
          //foreach(var type in types){
          // if(allSettings.TryGetValue(type,out var typeSettings)){
          //  if(typeSettings.TryGetValue(selection,out var typeSettingsListForSelection)){
          //   foreach(SimObjectSettings setting in typeSettingsListForSelection){
          //    float chance=setting.chance;//settingsCountForSelection[selection];
          //    
          //    count++;
          //    if(dicing<chance){
          //     //Log.DebugMessage("'dicing<chance':dicing:"+dicing+";chance:"+chance+";(type,setting):"+(type,setting));
          //     return(type,setting);
          //    }
          //   }
          //  }
          // }
          //}
         }
         return null;
        }
     internal Perlin  scaleModifierPerlin;
     internal Perlin rotationModifierPerlin;
        internal virtual SimObjectSpawnModifiers GetSimObjectSpawnModifiers(Vector3Int noiseInputRounded,SimObjectSettings simObjectSettings){
         Vector3 noiseInput=noiseInputRounded+biome.deround;
         SimObjectSpawnModifiers modifiers=new SimObjectSpawnModifiers{
          scale=Vector3.Lerp(
           simObjectSettings.minScale,
           simObjectSettings.maxScale,
           Mathf.Clamp01(
            ((float)   scaleModifierPerlin.GetValue(noiseInput.z,noiseInput.x,0)+1f)/2f
           )
          ),
          rotation=Mathf.Lerp(
           simObjectSettings.rotation.x,
           simObjectSettings.rotation.y,
           Mathf.Clamp01(
            ((float)rotationModifierPerlin.GetValue(noiseInput.z,noiseInput.x,0)+1f)/2f
           )
          ),
         };
         return modifiers;
        }
    }
}