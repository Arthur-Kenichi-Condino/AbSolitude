using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class SimActorCharacterController:MonoBehaviour{
     internal SimActor actor;
     internal CharacterController characterController;
        void Awake(){
         characterController=GetComponentInChildren<CharacterController>();
        }
        internal void ManualUpdate(){
        }
    }
}