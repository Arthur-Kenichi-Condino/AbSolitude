#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.SimActor;
namespace AKCondinoO.Sims.Actors{
    internal class SimActorAnimatorController:MonoBehaviour{
     internal SimActor actor;
      internal Vector3 actorLeft;
      internal Vector3 actorRight;
     internal Animator animator;
     internal SimActorAnimatorIKController animatorIKController;
     Vector3 tgtRot,tgtRot_Last;
      float tgtRotLerpTime;
       float tgtRotLerpVal;
        Quaternion tgtRotLerpA,tgtRotLerpB;
         [SerializeField]float tgtRotLerpSpeed=18.75f;
          [SerializeField]float tgtRotLerpMaxTime=.025f;
     Vector3 tgtPos,tgtPos_Last;
      float tgtPosLerpTime;
       float tgtPosLerpVal;
        Vector3 tgtPosLerpA,tgtPosLerpB;
         [SerializeField]float tgtPosLerpSpeed=18.75f;
          [SerializeField]float tgtPosLerpMaxTime=.025f;
        void Awake(){
        }
     bool synced=true;
     BaseAI.ActorMotion lastMotion=BaseAI.ActorMotion.MOTION_STAND;
     internal int layerCount{get;private set;}
     internal Dictionary<WeaponTypes,int>weaponLayer{get;private set;}
      WeaponTypes lastWeaponType=WeaponTypes.None;
     internal Dictionary<int,float>animationTime{get;private set;}
     Dictionary<int,int>loopCount;//  use integer part of normalizedTime [https://answers.unity.com/questions/1317841/how-to-find-the-normalised-time-of-a-looping-anima.html]
      Dictionary<int,int>lastLoopCount;
     Dictionary<int,List<AnimatorClipInfo>>animatorClip;
     Dictionary<int,int>lastClipInstanceID;
      Dictionary<int,string>lastClipName;
        internal void ManualUpdate(){
         if(animator==null){
          animator=GetComponentInChildren<Animator>();
          if(animator!=null){
           Log.DebugMessage("add SimActorAnimatorIKController");
           animatorIKController=animator.gameObject.AddComponent<SimActorAnimatorIKController>();
           animatorIKController.simActorAnimatorController=this;
           layerCount=animator.layerCount;
           weaponLayer=new Dictionary<WeaponTypes,int>(layerCount);
           animationTime=new Dictionary<int,float>(layerCount);
           loopCount=new Dictionary<int,int>(layerCount);
            lastLoopCount=new Dictionary<int,int>(layerCount);
           animatorClip=new Dictionary<int,List<AnimatorClipInfo>>(layerCount);
           lastClipInstanceID=new Dictionary<int,int>(layerCount);
            lastClipName=new Dictionary<int,string>(layerCount);
           for(int i=0;i<layerCount;++i){
            animationTime[i]=0f;
            loopCount[i]=0;
             lastLoopCount[i]=0;
            animatorClip[i]=new List<AnimatorClipInfo>();
            lastClipInstanceID[i]=0;
             lastClipName[i]="";
           }
           weaponLayer[WeaponTypes.None       ]=animator.GetLayerIndex("Base Layer");
           Log.DebugMessage("weaponLayer[WeaponTypes.None]:"+weaponLayer[WeaponTypes.None]);
           weaponLayer[WeaponTypes.SniperRifle]=animator.GetLayerIndex("Rifle");
           Log.DebugMessage("weaponLayer[WeaponTypes.SniperRifle]:"+weaponLayer[WeaponTypes.SniperRifle]);
           transitionTimeInterval=new WaitForSeconds(0.05f);
           layerTransitionCoroutine=StartCoroutine(LayerTransition());
           actor.simUMAData.transform.parent.SetParent(null);
          }
         }
         if(animator!=null&&actor is BaseAI baseAI){
          tgtRot=actor.simActorCharacterController.characterController.transform.eulerAngles+new Vector3(0f,180f,0f);
          tgtPos=actor.simActorCharacterController.characterController.transform.position+actor.simUMADataPosOffset;
          actorLeft=-actor.transform.right;
          actorRight=actor.transform.right;
          Vector3 boundsMaxRight=actor.simActorCharacterController.characterController.bounds.max;
                  boundsMaxRight.y=actor.simActorCharacterController.characterController.transform.position.y;
                  boundsMaxRight.z=actor.simActorCharacterController.characterController.transform.position.z;
          float maxRightDis=Vector3.Distance(actor.simActorCharacterController.characterController.transform.position,boundsMaxRight);
          Vector3 maxRight=actor.simActorCharacterController.characterController.transform.position+actor.simActorCharacterController.characterController.transform.rotation*(Vector3.right*maxRightDis);
          Vector3 boundsMinLeft=actor.simActorCharacterController.characterController.bounds.min;
                  boundsMinLeft.y=actor.simActorCharacterController.characterController.transform.position.y;
                  boundsMinLeft.z=actor.simActorCharacterController.characterController.transform.position.z;
          float minLeftDis=Vector3.Distance(actor.simActorCharacterController.characterController.transform.position,boundsMinLeft);
          Vector3 minLeft=actor.simActorCharacterController.characterController.transform.position+actor.simActorCharacterController.characterController.transform.rotation*(Vector3.left*minLeftDis);
          if(actor.navMeshAgent.enabled||actor.simActorCharacterController.isGrounded){
           Debug.DrawRay(maxRight,Vector3.down,Color.blue);
           if(Physics.Raycast(maxRight,Vector3.down,out RaycastHit rightFloorHit)){
            Debug.DrawRay(rightFloorHit.point,rightFloorHit.normal);
            Vector3 bottom=actor.simActorCharacterController.characterController.bounds.center;
                    bottom.y=actor.simActorCharacterController.characterController.bounds.min.y;
            Plane floorPlane=new Plane(rightFloorHit.normal,bottom);
            Ray leftRay=new Ray(minLeft,Vector3.down);
            Debug.DrawRay(leftRay.origin,leftRay.direction,Color.blue);
            if(floorPlane.Raycast(leftRay,out float enter)){
             Vector3 leftFloorHitPoint=leftRay.GetPoint(enter);
             float minY=Mathf.Min(bottom.y,leftFloorHitPoint.y,rightFloorHit.point.y);
             tgtPos.y+=minY-bottom.y;
             Debug.DrawLine(bottom,tgtPos,Color.yellow);
            }
           }
          }
          if(actor.simUMAData!=null){
           if(tgtRotLerpTime==0f){
            if(tgtRot!=tgtRot_Last){
             //Log.DebugMessage("input rotation detected:start rotating to tgtRot:"+tgtRot);
             tgtRotLerpVal=0f;
             tgtRotLerpA=actor.simUMAData.transform.parent.rotation;
             tgtRotLerpB=Quaternion.Euler(tgtRot);
             tgtRotLerpTime+=Time.deltaTime;
             tgtRot_Last=tgtRot;
            }
           }else{
            tgtRotLerpTime+=Time.deltaTime;
           }
           if(tgtRotLerpTime!=0f){
            tgtRotLerpVal+=tgtRotLerpSpeed*Time.deltaTime;
            if(tgtRotLerpVal>=1f){
             tgtRotLerpVal=1f;
             tgtRotLerpTime=0f;
            }
            actor.simUMAData.transform.parent.rotation=Quaternion.Lerp(tgtRotLerpA,tgtRotLerpB,tgtRotLerpVal);
            if(tgtRotLerpTime>=tgtRotLerpMaxTime){
             if(tgtRot!=tgtRot_Last){
              tgtRotLerpTime=0;
             }
            }
           }
           if(tgtPosLerpTime==0){
            if(tgtPos!=tgtPos_Last){
             tgtPosLerpVal=0;
             tgtPosLerpA=actor.simUMAData.transform.parent.position;
             tgtPosLerpB=tgtPos;
             tgtPosLerpTime+=Time.deltaTime;
             tgtPos_Last=tgtPos;
            }
           }else{
            tgtPosLerpTime+=Time.deltaTime;
           }
           if(tgtPosLerpTime!=0){
            tgtPosLerpVal+=tgtPosLerpSpeed*Time.deltaTime;
            if(tgtPosLerpVal>=1){
             tgtPosLerpVal=1;
             tgtPosLerpTime=0;
            }
            actor.simUMAData.transform.parent.position=Vector3.Lerp(tgtPosLerpA,tgtPosLerpB,tgtPosLerpVal);
            if(tgtPosLerpTime>tgtPosLerpMaxTime){
             if(tgtPos!=tgtPos_Last){
              tgtPosLerpTime=0;
             }
            }
           }
          }
          //  [https://answers.unity.com/questions/1035587/how-to-get-current-time-of-an-animator.html]
          foreach(var layer in animatorClip){
           int layerIndex=layer.Key;
           List<AnimatorClipInfo>clipList=layer.Value;
           clipList.Clear();
           AnimatorStateInfo animatorState=animator.GetCurrentAnimatorStateInfo(layerIndex);
                                           animator.GetCurrentAnimatorClipInfo (layerIndex,clipList);
           if(clipList.Count>0){
            if(lastClipInstanceID[layerIndex]!=(lastClipInstanceID[layerIndex]=clipList[0].clip.GetInstanceID())||lastClipName[layerIndex]!=clipList[0].clip.name){
             Log.DebugMessage("changed to new clipList[0].clip.name:"+clipList[0].clip.name+";clipList[0].clip.GetInstanceID():"+clipList[0].clip.GetInstanceID());
             lastClipName[layerIndex]=clipList[0].clip.name;
            }
            //Log.DebugMessage("current clipList[0].clip.name:"+clipList[0].clip.name);
            animationTime[layerIndex]=clipList[0].clip.length*animatorState.normalizedTime;
           }
          }
          if(lastMotion!=baseAI.motion){
           Log.DebugMessage("actor motion will be set from:"+lastMotion+" to:"+baseAI.motion);
          }
             if(baseAI is ArthurCondinoAI arthurCondinoAI){
              if(lastWeaponType!=baseAI.weaponType){
               if(weaponLayer.TryGetValue(baseAI.weaponType,out int layerIndex)){
                layerTargetWeight[layerIndex]=1.0f;
                if(weaponLayer.TryGetValue(lastWeaponType,out int lastLayerIndex)){
                 layerTargetWeight[lastLayerIndex]=0.0f;
                }
                lastWeaponType=baseAI.weaponType;
               }
              }
              if(baseAI.weaponType==SimActor.WeaponTypes.SniperRifle){
               animator.SetBool("MOTION_RIFLE_STAND",arthurCondinoAI.motion==BaseAI.ActorMotion.MOTION_RIFLE_STAND);
               animator.SetBool("MOTION_RIFLE_MOVE" ,arthurCondinoAI.motion==BaseAI.ActorMotion.MOTION_RIFLE_MOVE );
                animator.SetFloat("MOTION_RIFLE_MOVE_VELOCITY",arthurCondinoAI.moveVelocity);
                 animator.SetFloat("MOTION_RIFLE_MOVE_TURN",arthurCondinoAI.turnAngle/180f);
              }else{
               animator.SetBool("MOTION_STAND",arthurCondinoAI.motion==BaseAI.ActorMotion.MOTION_STAND);
               animator.SetBool("MOTION_MOVE" ,arthurCondinoAI.motion==BaseAI.ActorMotion.MOTION_MOVE );
                animator.SetFloat("MOTION_MOVE_VELOCITY",arthurCondinoAI.moveVelocity);
                 animator.SetFloat("MOTION_MOVE_TURN",arthurCondinoAI.turnAngle/180f);
              }
             }
          if(lastMotion!=baseAI.motion){
           Log.DebugMessage("actor changed motion from:"+lastMotion+" to:"+baseAI.motion);
          }
          lastMotion=baseAI.motion;
         }
        }
     Coroutine layerTransitionCoroutine;
      WaitForSeconds transitionTimeInterval;
      readonly Dictionary<int,float>layerTargetWeight=new Dictionary<int,float>();
       readonly Dictionary<int,float>layerWeight=new Dictionary<int,float>();
        IEnumerator LayerTransition(){
            Loop:{
             foreach(var layer in layerTargetWeight){
              int layerIndex=layer.Key;
              float targetWeight=layer.Value;
              if(!layerWeight.TryGetValue(layerIndex,out float weight)){
               weight=layerWeight[layerIndex]=animator.GetLayerWeight(layerIndex);
              }
              if(weight!=targetWeight){
               if(weight>targetWeight){
                weight-=20.0f*Time.deltaTime;
                if(weight<=targetWeight){
                 weight=targetWeight;
                }
               }else if(weight<targetWeight){
                weight+=20.0f*Time.deltaTime;
                if(weight>=targetWeight){
                 weight=targetWeight;
                }
               }
               animator.SetLayerWeight(layerIndex,layerWeight[layerIndex]=weight);
              }
             }
             yield return transitionTimeInterval;
            }
            goto Loop;
        }
        /// <summary>
        ///  Check if animation locks another motion beforehand
        /// </summary>
        /// <param name="motion"></param>
        /// <returns></returns>
        internal bool CurrentAnimationAllowsMotionChangeTo(BaseAI.ActorMotion motion){
         return true;
        }
        /// <summary>
        ///  Wait for the end of the currently running animation
        /// </summary>
        /// <returns></returns>
        internal bool Sync(){
         return synced;
        }
        void LateUpdate(){
        }
    }
}