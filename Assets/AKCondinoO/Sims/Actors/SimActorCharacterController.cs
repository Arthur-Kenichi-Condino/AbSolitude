using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class SimActorCharacterController:MonoBehaviour{
     internal SimActor actor;
     internal CharacterController characterController;
      internal Vector3 center;
        void Awake(){
         characterController=GetComponentInChildren<CharacterController>();
         center=characterController.center;
        }
        internal void ManualUpdate(){
        }
    }
}