using AKCondinoO.Sims.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid.Human{
    internal class HumanAI:HumanoidAI{
        internal override void OnActivated(){
         if(Core.singleton.isServer){
          if(!inventory.ContainsKey(typeof(SimHands2Items))){
           inventory.Add(typeof(SimHands2Items),new List<SimInventory>());
           inventory[typeof(SimHands2Items)].Add(new SimHands2Items(this));
          }
         }
         base.OnActivated();
        }
    }
}