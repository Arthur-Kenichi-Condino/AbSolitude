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
        internal class WeightedObject<T>{
         internal T value;
         internal float chance;//  de 0 a 1
         internal int priority;//  prioridade em empate
        }
        internal class WeightedPicker<T>{
         internal readonly T[]lookupTable;
         internal readonly int resolution;
            internal WeightedPicker(List<WeightedObject<T>>items,int resolution=100_000){
             if(items==null||items.Count==0){
              Log.Error("WeightedPicker:lista de itens não pode ser vazia.");
              return;
             }
             this.resolution=resolution;
             lookupTable=new T[resolution];
             //  Ordena para garantir prioridade em empate
             items.Sort((a,b)=>{
              int cmp=b.priority.CompareTo(a.priority);//  prioridade maior primeiro
              if(cmp==0)return 0;//  mantém ordem original em empate real
              return cmp;
             });
             //  Normaliza soma dos pesos
             float totalChance=0f;
             foreach(var item in items)totalChance+=item.chance;
             if(totalChance<=0f){
              Log.Error("WeightedPicker:soma das chances deve ser > 0.");
              return;
             }
             //  Preenche tabela
             float cumulative=0f;
             int index=0;
             foreach(var item in items){
              cumulative+=item.chance/totalChance;
              int endIndex=(int)(cumulative*resolution);
              //  Garantir limites corretos
              if(endIndex>resolution)
               endIndex=resolution;
              for(;index<endIndex;index++)
               lookupTable[index]=item.value;
             }
             //  Preencher qualquer sobra (por arredondamento)
             for(;index<resolution;index++)
              lookupTable[index]=items[items.Count-1].value;
            }
            internal bool Get(float value,out T result){
             if(value<0f)value=0f;
             if(value>1f)value=1f;
             int idx=(int)(value*resolution);
             if(idx>=resolution)idx=resolution-1;
             result=lookupTable[idx];
             return true;
            }
        }
        internal class ByChanceObject<T>{
         internal T value;
         internal float chance;//  0 < Chance <= 1
         internal int priority;//  desempate
        }
        internal class ByChancePicker<T>{
         private readonly Dictionary<int,T>lookupTable=new Dictionary<int,T>();
         private readonly float maxWeight;
         private readonly int resolution; // tamanho da tabela virtual
         private readonly bool isStruct;
            internal ByChancePicker(List<ByChanceObject<T>>items,int resolution=1000){
             if(items==null){
              Log.Error(nameof(items));
              return;
             }
             this.resolution=resolution;
             isStruct=typeof(T).IsValueType;
             //  Ordena por peso decrescente, depois prioridade decrescente
             ByChanceObject<T>[]sortedItems=items
              .OrderByDescending(x=>x.chance)
              .ThenByDescending(x=>x.priority)
              .ToArray();
             if(sortedItems.Length==0){
              Log.Error("items cannot be empty");
              return;
             }
             maxWeight=sortedItems.Max(x=>x.chance);
             //  Construir lookupTable usando resolução
             float cumulative=0f;
             foreach(var entry in sortedItems){
              int startIdx=(int)(cumulative*resolution);
              cumulative+=entry.chance;
              int endIdx=(int)(Math.Min(cumulative,1f)*resolution);
              for(int i=startIdx;i<endIdx;i++){
               lookupTable[i]=entry.value;
              }
             }
            }
            internal bool Get(float value,out T result){
             result=default;
             if(value<0f||value>1f){
              Log.Error(nameof(value));
              return false;
             }
             if(value>maxWeight)return false;//  null para reference ou default(T?) para struct
             int idx=(int)(value*resolution);
             if(lookupTable.TryGetValue(idx,out result)){
              return true;
             }
             return false;
            }
        }
     readonly protected Dictionary<int,ByChancePicker<(Type simObject,SimObjectSettings settings)>>simObjectPicking=new();
      readonly Dictionary<int,List<ByChanceObject<(Type,SimObjectSettings)>>>simObjectPickingItems=new();
     readonly protected Dictionary<Type,Dictionary<int,List<SimObjectSettings>>>allSettings=new Dictionary<Type,Dictionary<int,List<SimObjectSettings>>>();
     //readonly protected Dictionary<int,int>settingsCountForSelection=new Dictionary<int,int>();
     readonly BaseBiome biome;
     internal static Vector3 maxSpawnSize=new Vector3(256f,256f,256f);
     internal static Vector3 margin      =new Vector3(  1f,  1f,  1f);
        internal BaseBiomeSimObjectsSpawnSettings(BaseBiome biome){
         this.biome=biome;
        }
        internal void Set(){
         //simObjectPickingItems.Clear();
         foreach(var kvp in simObjectPickingItems){
          kvp.Value.Clear();
         }
         if(biomeBehaviour.useHardCodedSurfaceSpawnIfAvailable){
          #region hard-coded settings
          #endregion hard-coded settings
         }
         foreach(SurfaceSpawn surfaceSpawnSetting in biomeBehaviour.settings.biomeSurfaceSpawns){
          if(surfaceSpawnSetting.chance<=0){continue;}
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
          int picking=surfaceSpawnSetting.picking;
          if(!simObjectPickingItems.TryGetValue(picking,out var itemsForPicking)){
           simObjectPickingItems.Add(picking,itemsForPicking=new());
          }
          itemsForPicking.Add(byChanceSimObject);
          Log.DebugMessage("spawn setting added for:"+simObjectType);
          Log.Warning("TO DO: criar cooldown de spawn de objeto (e remover/refazer 'divisão de chance' no Selection) e lidar com objetos maiores que um chunk");
          Log.Warning("TO DO: rotacionar objeto para que não fique flutuando ao ser spawn'ado");
          Log.Warning("TO DO: limitar tamanho de objetos aqui pelo tamanho máximo de spawn menos 1");
          //settingsListAtPicking.Add(
          // new SimObjectSettings(
          //  size:       surfaceSpawnSetting.size,
          //  chance:     surfaceSpawnSetting.chance,
          //  priority:   surfaceSpawnSetting.priority,
          //  inclination:surfaceSpawnSetting.inclination,
          //  assetScale: surfaceSpawnSetting.assetScale,
          //  minScale:   surfaceSpawnSetting.minScale,
          //  maxScale:   surfaceSpawnSetting.maxScale,
          //  rotation:   surfaceSpawnSetting.rotation,
          //  depth:      surfaceSpawnSetting.depth,
          //  minSpacing:surfaceSpawnSetting.minSpacing.ToDictionary(k=>k.spawnedType,v=>v.spacingDis),
          //  maxSpacing:surfaceSpawnSetting.minSpacing.ToDictionary(k=>k.spawnedType,v=>v.spacingDis),
          //  blocksTypes:surfaceSpawnSetting.blocksTypes,
          //  isBlockedBy:surfaceSpawnSetting.isBlockedBy
          // ){
          // }
          //);
         }
         foreach(var pickingItemsPair in simObjectPickingItems){
          int picking=pickingItemsPair.Key;
          ByChancePicker<(Type simObject,SimObjectSettings settings)>pickList=new(pickingItemsPair.Value);
          SetSimObjectTypeAtPicking(picking,pickList);
          //int settingsCount=0;
          //foreach(var typeSettingsListByPickingPair in allSettings){
          // Type type=typeSettingsListByPickingPair.Key;
          // Dictionary<int,List<SimObjectSettings>>settingsListByPicking=typeSettingsListByPickingPair.Value;
          // if(types.Contains(type)){
          //  if(settingsListByPicking.TryGetValue(selection,out List<SimObjectSettings>settingsList)){
          //   settingsCount+=settingsList.Count;
          //  }
          // }
          //}
          //settingsCountForSelection[selection]=settingsCount;
          //Log.DebugMessage("BaseBiomeSimObjectsSpawnSettings Set():settingsCountForSelection["+selection+"]="+settingsCount);
         }
        }
        protected void SetSimObjectTypeAtPicking(int picking,ByChancePicker<(Type simObject,SimObjectSettings settings)>pickList){
         simObjectPicking[picking]=pickList;
         //typesAtPicking.Add(simObjectType);
         //if(!allSettings.TryGetValue(simObjectType,out var simObjectTypeSettings)){
         // simObjectTypeSettings=allSettings[simObjectType]=new Dictionary<int,List<SimObjectSettings>>();
         //}
         //if(!simObjectTypeSettings.TryGetValue(picking,out simObjectTypeSettingsListAtPicking)){
         // simObjectTypeSettingsListAtPicking=simObjectTypeSettings[picking]=new List<SimObjectSettings>();
         //}
        }
     internal Perlin simObjectSpawnChancePerlin;
        internal(Type simObject,SimObjectSettings simObjectSettings)?TryGetSettingsToSpawnSimObject(Vector3Int noiseInputRounded,out double selectionValue){
         Vector3 noiseInput=noiseInputRounded+biome.deround;
         int selection=biome.GetSelectorValue(noiseInput,out selectionValue);
         selection=1;//  debug
         if(simObjectPicking.TryGetValue(selection,out var pickList)){
          float dicing=Mathf.Clamp01(((float)simObjectSpawnChancePerlin.GetValue(noiseInput.z,noiseInput.x,0)+1f)/2f);
          bool picked=pickList.Get(dicing,out var result);
          if(picked){
           return result;
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