using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimHands:SimInventory{
     internal Transform  leftHand{get{if(asSimObject!=null){return asSimObject. leftHand;}return null;}}
     internal Transform rightHand{get{if(asSimObject!=null){return asSimObject.rightHand;}return null;}}
        internal SimHands():base(2){
        }
    }
}