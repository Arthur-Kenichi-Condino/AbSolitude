#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventoryItemsInContainerSettings{
        internal struct SimObjectSettings{
         public readonly ReadOnlyDictionary<Type,int>inventorySpaces;
         public HandsUsage handsUsage;
         public Vector3 leftHandGrabPos;public Vector3 rightHandGrabPos;
            internal SimObjectSettings(Dictionary<Type,int>inventorySpaces,HandsUsage handsUsage,Vector3 leftHandGrabPos,Vector3 rightHandGrabPos){
             this.inventorySpaces=new ReadOnlyDictionary<Type,int>(inventorySpaces);
             this.handsUsage=handsUsage;
             this.leftHandGrabPos=leftHandGrabPos;this.rightHandGrabPos=rightHandGrabPos;
            }
        }
        public enum HandsUsage:int{
         None=0,
         OneHanded=1,
         TwoHanded=2,
        }
     internal readonly Dictionary<Type,SimObjectSettings>allSettings=new Dictionary<Type,SimObjectSettings>();
        internal SimInventoryItemsInContainerSettings(){
        }
        internal void Set(){
         Log.DebugMessage("Set SimInventoryItemsSettings");
         foreach(var simObjectPrefab in SimObjectSpawner.singleton.simObjectPrefabs){
          SimObjectAsInventoryItemSettings simObjectAsInventoryItemSettings=simObjectPrefab.Value.GetComponent<SimObjectAsInventoryItemSettings>();
          if(simObjectAsInventoryItemSettings!=null){
           SimObject simObject=simObjectPrefab.Value.GetComponent<SimObject>();
           Type simObjectType=simObject.GetType();
           Dictionary<Type,int>inventorySpaces=new Dictionary<Type,int>();
           for(int i=0;i<simObjectAsInventoryItemSettings.inventorySettings.Length;++i){
            SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData inContainerData=simObjectAsInventoryItemSettings.inventorySettings[i];
            Type simInventoryType=ReflectionUtil.GetTypeByName(inContainerData.SimInventoryType);
            if(simInventoryType!=null){
             Log.DebugMessage("simInventoryType:"+simInventoryType);
             int spacesUsed=inContainerData.SimInventorySpaceUse;
             inventorySpaces.Add(simInventoryType,spacesUsed);
            }
           }
           Log.DebugMessage("simObjectType:"+simObjectType);
           allSettings.Add(simObjectType,
            new SimObjectSettings(
             inventorySpaces:inventorySpaces,
             handsUsage:simObjectAsInventoryItemSettings.handsUsage,
             leftHandGrabPos:Vector3.zero,
             rightHandGrabPos:Vector3.zero)
           );
          }
          //allSettings.Add(simObjectPrefab.Key,new Dictionary<Type,SimObjectSettings>());
          //if(!typeDictionary.ContainsKey(simObjectPrefab.Key)){
          // continue;
          //}
          //switch(typeDictionary[simObjectPrefab.Key]){
          // case("RemingtonModel700BDL"):{
          //  break;
          // }
          //}
         }
         //allSettings.Add(
         // typeof(RemingtonModel700BDL),
         //  new SimObjectSettings(
         //   inventorySpaces:2,
         //    handsUsage:HandsUsage.TwoHanded
         //  )
         //);
        }
     internal static readonly Dictionary<Type,string>typeDictionary=new Dictionary<Type,string>{
      {typeof(RemingtonModel700BDL),"RemingtonModel700BDL"},
     };
    }
}