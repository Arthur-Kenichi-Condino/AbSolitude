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
using static AKCondinoO.Sims.Inventory.SimObjectAsInventoryItemSettings;
using static AKCondinoO.Sims.Inventory.SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData;
using static AKCondinoO.Sims.Inventory.SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData.SimObjectAsInventoryItemTransformForMotion;
using static AKCondinoO.Sims.Inventory.SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData.SimObjectAsInventoryItemTransformForMotion.SimObjectAsInventoryItemTransformForSimType;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventoryItemsInContainerSettings{
        internal class SimObjectSettings{
         public readonly ReadOnlyDictionary<Type,int>inventorySpaces;
          public readonly ReadOnlyDictionary<Type,ReadOnlyDictionary<ActorMotion,ReadOnlyDictionary<Type,ReadOnlyDictionary<string,SimObjectAsInventoryItemTransformForParentBodyPartName>>>>inventoryTransformSettings;
         public HandsUsage handsUsage;
         public WeaponTypes weaponType;
            internal SimObjectSettings(HandsUsage handsUsage,WeaponTypes weaponType,SimInventoryItemInContainerData[]inventorySettings){
             Dictionary<Type,int>inventorySpaces=new Dictionary<Type,int>();
             var inventoryTransformSettingsDictionary=new Dictionary<Type,ReadOnlyDictionary<ActorMotion,ReadOnlyDictionary<Type,ReadOnlyDictionary<string,SimObjectAsInventoryItemTransformForParentBodyPartName>>>>();
             for(int i=0;i<inventorySettings.Length;++i){
              SimInventoryItemInContainerData inContainerData=inventorySettings[i];
              Type simInventoryType=ReflectionUtil.GetTypeByName(inContainerData.simInventoryType,typeof(SimInventory));
              if(simInventoryType!=null){
               Log.DebugMessage("simInventoryType:"+simInventoryType);
               int spacesUsed=inContainerData.simInventorySpaceUse;
               inventorySpaces.Add(simInventoryType,spacesUsed);
               var transformSettingsForMotion=new Dictionary<ActorMotion,ReadOnlyDictionary<Type,ReadOnlyDictionary<string,SimObjectAsInventoryItemTransformForParentBodyPartName>>>();
               foreach(var settings1 in inContainerData.transformSettingsForMotion){
                ActorMotion motion=settings1.motion;
                var transformSettingsForSimType=new Dictionary<Type,ReadOnlyDictionary<string,SimObjectAsInventoryItemTransformForParentBodyPartName>>();
                foreach(var settings2 in settings1.transformSettingsForSimType){
                 Type simType=ReflectionUtil.GetTypeByName(settings2.simTypeName);
                 if(simType!=null){
                  var transformSettingsForParentBodyPartName=new Dictionary<string,SimObjectAsInventoryItemTransformForParentBodyPartName>();
                  foreach(var settings3 in settings2.transformSettingsForParentBodyPartName){
                   string parentBodyPartName=settings3.parentBodyPartName;
                   transformSettingsForParentBodyPartName[parentBodyPartName]=settings3;
                  }
                  transformSettingsForSimType[simType]=new(transformSettingsForParentBodyPartName);
                 }
                }
                transformSettingsForMotion[motion]=new(transformSettingsForSimType);
               }
               inventoryTransformSettingsDictionary.Add(simInventoryType,new(transformSettingsForMotion));
              }
             }
             this.inventorySpaces=new ReadOnlyDictionary<Type,int>(inventorySpaces);
             this.inventoryTransformSettings=new(inventoryTransformSettingsDictionary);
             this.handsUsage=handsUsage;
             this.weaponType=weaponType;
            }
        }
     internal readonly Dictionary<Type,SimObjectSettings>allSettings=new Dictionary<Type,SimObjectSettings>();
        internal SimInventoryItemsInContainerSettings(){
        }
        internal void Set(){
         Log.DebugMessage("Set SimInventoryItemsInContainerSettings");
         foreach(var simObjectPrefab in SimObjectSpawner.singleton.simObjectPrefabs){
          SimObjectAsInventoryItemSettings simObjectAsInventoryItemSettings=simObjectPrefab.Value.GetComponent<SimObjectAsInventoryItemSettings>();
          if(simObjectAsInventoryItemSettings!=null){
           SimObject simObject=simObjectPrefab.Value.GetComponent<SimObject>();
           Type simObjectType=simObjectPrefab.Key;
           if(typeDictionary.ContainsKey(simObjectType)){
            switch(typeDictionary[simObjectType]){
             case("RemingtonModel700BDL"):{
              break;
             }
            }
           }
           Log.DebugMessage("simObjectType:"+simObjectType+";added to SimInventoryItemsInContainerSettings");
           allSettings.Add(simObjectType,
            new SimObjectSettings(
             handsUsage:simObjectAsInventoryItemSettings.handsUsage,
             weaponType:simObjectAsInventoryItemSettings.weaponType,
             inventorySettings:simObjectAsInventoryItemSettings.inventorySettings)
           );
          }
         }
        }
     internal static readonly Dictionary<Type,string>typeDictionary=new Dictionary<Type,string>{
      {typeof(RemingtonModel700BDL),"RemingtonModel700BDL"},
     };
    }
}