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
     internal bool isGrounded{get{return characterController.isGrounded;}}
     Vector3 tgtRot,tgtRot_Last;
      float tgtRotLerpTime;
       float tgtRotLerpVal;
        Quaternion tgtRotLerpA,tgtRotLerpB;
         [SerializeField]float tgtRotLerpSpeed=18.75f;
          [SerializeField]float tgtRotLerpMaxTime=.025f;
      Vector3 inputViewRotationEuler;
       [SerializeField]float viewRotationSmoothValue=.025f;
     //Vector3 tgtPos,tgtPos_Last;
      Vector3 inputMoveVelocity=Vector3.zero;
       [SerializeField]Vector3 moveAcceleration=new Vector3(.4f,.4f,.4f);
        [SerializeField]Vector3 maxMoveSpeed=new Vector3(4.0f,4.0f,4.0f);
     [SerializeField]float jumpTimeLength=.125f;
      [SerializeField]float jumpSpeed=8.0f;
       float jumpStrength;
        internal Vector3 beforeMovePos;
         internal Vector3 afterMovePos;
          internal Vector3 moveDelta;
        internal Vector3 aimingAt;
        internal void ManualUpdate(){
        }
    }
}