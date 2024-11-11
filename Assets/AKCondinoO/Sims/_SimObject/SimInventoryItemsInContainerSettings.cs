#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using static AKCondinoO.Sims.Actors.BaseAI;
using static AKCondinoO.Sims.Actors.SimActor;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventoryItemsInContainerSettings{//  Global
     internal readonly Dictionary<Type,InContainerSettings>allSettings=new Dictionary<Type,InContainerSettings>();
        internal SimInventoryItemsInContainerSettings(){
        }
        internal void Set(){
         //Log.DebugMessage("Set SimInventoryItemsInContainerSettings");
         foreach(var simObjectPrefab in SimObjectSpawner.singleton.simObjectPrefabs){
          SimObjectAsInventoryItemInContainerData inContainerData=simObjectPrefab.Value.GetComponent<SimObjectAsInventoryItemInContainerData>();
          if(inContainerData!=null){
           SimObject simObject=simObjectPrefab.Value.GetComponent<SimObject>();
           Type simObjectType=simObjectPrefab.Key;
           if(typeDictionary.ContainsKey(simObjectType)){
            switch(typeDictionary[simObjectType]){
             case("RemingtonModel700BDL"):{
              break;
             }
            }
           }
           //Log.DebugMessage("simObjectType:"+simObjectType+";added to SimInventoryItemsInContainerSettings");
           allSettings.Add(simObjectType,
            new InContainerSettings(
             inContainerData
            )
           );
          }
         }
        }
        internal class InContainerSettings{
         public HandsUsage handsUsage;
         public WeaponTypes weaponType;
         public readonly ReadOnlyDictionary<Type,int>spacesUsage;
         public readonly ReadOnlyDictionary<Type,
                          ReadOnlyDictionary<(Type containerSimType,ActorMotion?containerSimMotion,ActorWeaponLayerMotion?containerSimWeaponLayerMotion,ActorToolLayerMotion?containerSimToolLayerMotion,string layer),
                           ReadOnlyDictionary<string,
                            InContainerTransformData
                           >
                          >
                         >transformSettings;
            internal InContainerSettings(SimObjectAsInventoryItemInContainerData inContainerData){
             this.handsUsage=inContainerData.handsUsage;
             this.weaponType=inContainerData.weaponType;
             Dictionary<Type,int>spacesUsage=new Dictionary<Type,int>();
             var transformSettings=new Dictionary<Type,
                                        ReadOnlyDictionary<(Type containerSimType,ActorMotion?containerSimMotion,ActorWeaponLayerMotion?containerSimWeaponLayerMotion,ActorToolLayerMotion?containerSimToolLayerMotion,string layer),
                                         ReadOnlyDictionary<string,
                                          InContainerTransformData
                                         >
                                        >
                                       >();
             for(int i=0;i<inContainerData.dataArrays.Length;++i){
              SimObjectAsInventoryItemInContainerData.InContainerData data=inContainerData.dataArrays[i];
              Type simInventoryType=ReflectionUtil.GetTypeByName(data.simInventoryType,typeof(SimInventory));
              if(simInventoryType!=null){
               //Log.DebugMessage("simInventoryType:"+simInventoryType);
               int spaces=data.simInventorySpacesUsage;
               spacesUsage.Add(simInventoryType,spaces);
               var transformSettingsDictionary=new Dictionary<(Type containerSimType,ActorMotion?containerSimMotion,ActorWeaponLayerMotion?containerSimWeaponLayerMotion,ActorToolLayerMotion?containerSimToolLayerMotion,string layer),
                                                     Dictionary<string,
                                                      InContainerTransformData
                                                     >
                                                    >();
               foreach(var transformData in data.transformData){
                Type simType=ReflectionUtil.GetTypeByName(transformData.simTypeName,typeof(SimObject));
                if(simType!=null){
                 var key=(simType,transformData.motion,transformData.weaponMotion,transformData.toolMotion,transformData.layer);
                 if(!transformSettingsDictionary.TryGetValue(key,out var parentBodyPartNameDictionary)){
                  transformSettingsDictionary.Add(key,parentBodyPartNameDictionary=new());
                 }
                 parentBodyPartNameDictionary.Add(transformData.parentBodyPartName,transformData);
                }
               }
               var transformSettingsForReadOnly=new Dictionary<(Type containerSimType,ActorMotion?containerSimMotion,ActorWeaponLayerMotion?containerSimWeaponLayerMotion,ActorToolLayerMotion?containerSimToolLayerMotion,string layer),
                                                     ReadOnlyDictionary<string,
                                                      InContainerTransformData
                                                     >
                                                    >();
               foreach(var parentBodyPartNameDictionary in transformSettingsDictionary){
                transformSettingsForReadOnly.Add(parentBodyPartNameDictionary.Key,new(parentBodyPartNameDictionary.Value));
               }
               transformSettings.Add(simInventoryType,new(transformSettingsForReadOnly));
              }
             }
             this.spacesUsage=new ReadOnlyDictionary<Type,int>(spacesUsage);
             this.transformSettings=new(transformSettings);
            }
        }
     internal static readonly Dictionary<Type,string>typeDictionary=new Dictionary<Type,string>{
      {typeof(RemingtonModel700BDL),"RemingtonModel700BDL"},
     };
    }
}