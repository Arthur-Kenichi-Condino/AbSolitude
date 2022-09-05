using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class SimActorCharacterController:MonoBehaviour{
     internal CharacterController characterController;
        void Awake(){
         characterController=GetComponent<CharacterController>();
        }
        internal void ManualUpdate(){
        }
    }
}