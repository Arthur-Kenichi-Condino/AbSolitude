using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimHands:SimInventory{
     internal Transform  leftHand{get{if(owner!=null){return owner. leftHand;}return null;}}
     internal Transform rightHand{get{if(owner!=null){return owner.rightHand;}return null;}}
        internal SimHands(SimObject owner):base(owner,2){
        }
    }
}