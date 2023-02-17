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
         Log.DebugMessage("Set SimInventoryItemsInContainerSettings");
         foreach(var simObjectPrefab in SimObjectSpawner.singleton.simObjectPrefabs){
          SimObjectAsInventoryItemSettings simObjectAsInventoryItemSettings=simObjectPrefab.Value.GetComponent<SimObjectAsInventoryItemSettings>();
          if(simObjectAsInventoryItemSettings!=null){
           if(simObjectAsInventoryItemSettings. leftHandGrabPos!=null&&
              simObjectAsInventoryItemSettings.rightHandGrabPos!=null
           ){
            SimObject simObject=simObjectPrefab.Value.GetComponent<SimObject>();
            Type simObjectType=simObjectPrefab.Key;
            Dictionary<Type,int>inventorySpaces=new Dictionary<Type,int>();
            for(int i=0;i<simObjectAsInventoryItemSettings.inventorySettings.Length;++i){
             SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData inContainerData=simObjectAsInventoryItemSettings.inventorySettings[i];
             Type simInventoryType=ReflectionUtil.GetTypeByName(inContainerData.simInventoryType);
             if(simInventoryType!=null){
              Log.DebugMessage("simInventoryType:"+simInventoryType);
              int spacesUsed=inContainerData.simInventorySpaceUse;
              inventorySpaces.Add(simInventoryType,spacesUsed);
             }
            }
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
              inventorySpaces:inventorySpaces,
              handsUsage:simObjectAsInventoryItemSettings.handsUsage,
               leftHandGrabPos:simObjectAsInventoryItemSettings. leftHandGrabPos.localPosition,
              rightHandGrabPos:simObjectAsInventoryItemSettings.rightHandGrabPos.localPosition)
            );
           }
          }
         }
        }
     internal static readonly Dictionary<Type,string>typeDictionary=new Dictionary<Type,string>{
      {typeof(RemingtonModel700BDL),"RemingtonModel700BDL"},
     };
    }
}