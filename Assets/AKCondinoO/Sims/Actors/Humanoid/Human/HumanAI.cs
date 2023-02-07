using AKCondinoO.Sims.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid.Human{
    internal class HumanAI:HumanoidAI{
        internal override void OnActivated(){
         if(Core.singleton.isServer){
          if(!inventory.ContainsKey(typeof(SimHands))){
           inventory.Add(typeof(SimHands),new List<SimInventory>());
           inventory[typeof(SimHands)].Add(new SimHands(this));
          }
         }
         base.OnActivated();
        }
    }
}