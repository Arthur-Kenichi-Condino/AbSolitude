using AKCondinoO.Sims.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid.Human{
    internal partial class HumanAI:HumanoidAI{
        internal override void OnActivated(){
         if(Core.singleton.isServer){
          if(!inventory.ContainsKey(typeof(SimHands))||inventory[typeof(SimHands)].Count<=0){
           SimInventoryManager.singleton.AddInventoryTo(this,typeof(SimHands));
          }
         }
         base.OnActivated();
        }
    }
}