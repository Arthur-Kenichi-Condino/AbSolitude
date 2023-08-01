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
    internal class SimInventoryItemsInContainerSettings{
        internal class SimObjectSettings{
         public readonly ReadOnlyDictionary<Type,int>inventorySpaces;
          public readonly ReadOnlyDictionary<Type,ReadOnlyDictionary<ActorMotion,SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData.SimObjectAsInventoryItemTransformForMotion>>inventoryTransformSettingsForMotion;
         public HandsUsage handsUsage;
         public WeaponTypes weaponType;
            internal SimObjectSettings(HandsUsage handsUsage,WeaponTypes weaponType,SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData[]inventorySettings){
             Dictionary<Type,int>inventorySpaces=new Dictionary<Type,int>();
             Dictionary<Type,ReadOnlyDictionary<ActorMotion,SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData.SimObjectAsInventoryItemTransformForMotion>>inventoryTransformSettingsForMotion=new Dictionary<Type,ReadOnlyDictionary<ActorMotion,SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData.SimObjectAsInventoryItemTransformForMotion>>();
             for(int i=0;i<inventorySettings.Length;++i){
              SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData inContainerData=inventorySettings[i];
              Type simInventoryType=ReflectionUtil.GetTypeByName(inContainerData.simInventoryType,typeof(SimInventory));
              if(simInventoryType!=null){
               Log.DebugMessage("simInventoryType:"+simInventoryType);
               int spacesUsed=inContainerData.simInventorySpaceUse;
               inventorySpaces.Add(simInventoryType,spacesUsed);
               Dictionary<ActorMotion,SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData.SimObjectAsInventoryItemTransformForMotion>transformSettingsForMotion=new Dictionary<ActorMotion,SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData.SimObjectAsInventoryItemTransformForMotion>();
               foreach(var transformSettings in inContainerData.transformSettingsForMotion){
                transformSettingsForMotion[transformSettings.motion]=transformSettings;
               }
               inventoryTransformSettingsForMotion.Add(simInventoryType,new ReadOnlyDictionary<ActorMotion,SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData.SimObjectAsInventoryItemTransformForMotion>(transformSettingsForMotion));
              }
             }
             this.inventorySpaces=new ReadOnlyDictionary<Type,int>(inventorySpaces);
             this.inventoryTransformSettingsForMotion=new ReadOnlyDictionary<Type,ReadOnlyDictionary<ActorMotion,SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData.SimObjectAsInventoryItemTransformForMotion>>(inventoryTransformSettingsForMotion);
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