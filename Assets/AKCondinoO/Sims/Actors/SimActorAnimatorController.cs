#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Humanoid;
using AKCondinoO.Sims.Actors.Humanoid.Human;
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.SimActor;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActorAnimatorController:MonoBehaviour{
     internal SimActor actor;
      internal Vector3 actorLeft;
      internal Vector3 actorRight;
     internal Animator animator;
     internal SimActorAnimatorIKController animatorIKController;
     [SerializeField]internal           QuaternionRotLerpHelper rotLerp=new           QuaternionRotLerpHelper();
     [SerializeField]internal Vector3PosComponentwiseLerpHelper posLerp=new Vector3PosComponentwiseLerpHelper();
        void Awake(){
        }
        protected virtual void GetAnimator(){
         if(animator==null){
          animator=GetComponentInChildren<Animator>();
          if(animator!=null){
           Log.DebugMessage("add SimActorAnimatorIKController");
           animatorIKController=animator.gameObject.AddComponent<SimActorAnimatorIKController>();
           animatorIKController.simActorAnimatorController=this;
           animatorIKController.headLookAtPositionLerp.tgtPosLerpSpeed=Mathf.Max(
            rotLerp.tgtRotLerpSpeed+1f,
            posLerp.xLerp.tgtValLerpSpeed+1f,
            posLerp.yLerp.tgtValLerpSpeed+1f,
            posLerp.zLerp.tgtValLerpSpeed+1f,
            46.875f
           );
           layerCount=animator.layerCount;
           weaponLayer=new Dictionary<WeaponTypes,int>(layerCount);
           animationTime=new Dictionary<int,float>(layerCount);
            animationTimeInCurrentLoop=new Dictionary<int,float>(layerCount);
           normalizedTime=new Dictionary<int,float>(layerCount);
            normalizedTimeInCurrentLoop=new Dictionary<int,float>(layerCount);
           loopCount=new Dictionary<int,int>(layerCount);
            lastLoopCount=new Dictionary<int,int>(layerCount);
             looped=new Dictionary<int,bool>(layerCount);
           animatorClip=new Dictionary<int,List<AnimatorClipInfo>>(layerCount);
           currentClipInstanceID=new Dictionary<int,int>(layerCount);
            currentClipName=new Dictionary<int,string>(layerCount);
           for(int i=0;i<layerCount;++i){
            animationTime[i]=0f;
             animationTimeInCurrentLoop[i]=0f;
            normalizedTime[i]=0f;
             normalizedTimeInCurrentLoop[i]=0f;
            loopCount[i]=0;
             lastLoopCount[i]=0;
              looped[i]=false;
            animatorClip[i]=new List<AnimatorClipInfo>();
            currentClipInstanceID[i]=0;
             currentClipName[i]="";
           }
           weaponLayer[WeaponTypes.None       ]=animator.GetLayerIndex("Base Layer");
           Log.DebugMessage("weaponLayer[WeaponTypes.None]:"+weaponLayer[WeaponTypes.None]);
           weaponLayer[WeaponTypes.SniperRifle]=animator.GetLayerIndex("Rifle");
           Log.DebugMessage("weaponLayer[WeaponTypes.SniperRifle]:"+weaponLayer[WeaponTypes.SniperRifle]);
           layerTransitionCoroutine=StartCoroutine(LayerTransition());
           if(actor.simUMA!=null){
            actor.simUMA.transform.parent.SetParent(null);
            GetTransformTgtValuesFromCharacterController();
            rotLerp.tgtRot_Last=rotLerp.tgtRot;
            posLerp.tgtPos_Last=posLerp.tgtPos;
            actor.simUMA.transform.parent.rotation=rotLerp.tgtRot;
            actor.simUMA.transform.parent.position=posLerp.tgtPos;
           }
          }
         }
        }
     bool synced=true;
     BaseAI.ActorMotion lastMotion=BaseAI.ActorMotion.MOTION_STAND;
     internal int layerCount{get;private set;}
     internal Dictionary<WeaponTypes,int>weaponLayer{get;private set;}
      internal WeaponTypes lastWeaponType=WeaponTypes.None;
     internal Dictionary<int,float>animationTime{get;private set;}
      internal Dictionary<int,float>animationTimeInCurrentLoop{get;private set;}
     Dictionary<int,float>normalizedTime;
      Dictionary<int,float>normalizedTimeInCurrentLoop;
     Dictionary<int,int>loopCount;//  use integer part of normalizedTime [https://answers.unity.com/questions/1317841/how-to-find-the-normalised-time-of-a-looping-anima.html]
      Dictionary<int,int>lastLoopCount;
       Dictionary<int,bool>looped;
     Dictionary<int,List<AnimatorClipInfo>>animatorClip;
     Dictionary<int,int>currentClipInstanceID;
      Dictionary<int,string>currentClipName;
        internal virtual void ManualUpdate(){
         GetAnimator();
         if(animator!=null&&actor is BaseAI baseAI){
          GetTransformTgtValuesFromCharacterController();
          SetTransformTgtValuesUsingActorAndPhysicData();
          if(actor.simUMA!=null){
           SetSimUMADataTransform();
          }
          GetAnimatorStateInfo();
          UpdateMotion(baseAI);
         }
        }
        protected void OnAnimationLooped(int layerIndex,string currentClipName){
         if(actor is BaseAI baseAI){
          baseAI.OnShouldSetNextMotionAnimatorAnimationLooped(layerIndex:layerIndex,currentClipName:currentClipName);
         }
        }
        protected void OnAnimationChanged(int layerIndex,string lastClipName,string currentClipName){
         if(actor is BaseAI baseAI){
          baseAI.OnShouldSetNextMotionAnimatorAnimationChanged(layerIndex:layerIndex,lastClipName:lastClipName,currentClipName:currentClipName);
         }
        }
        protected virtual void GetTransformTgtValuesFromCharacterController(){
         rotLerp.tgtRot=Quaternion.Euler(actor.simActorCharacterController.characterController.transform.eulerAngles+new Vector3(0f,180f,0f));
         posLerp.tgtPos=actor.simActorCharacterController.characterController.transform.position+actor.simUMAPosOffset;
        }
        protected virtual void SetTransformTgtValuesUsingActorAndPhysicData(){
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
          if(Physics.Raycast(maxRight,Vector3.down,out RaycastHit rightFloorHit,2f,PhysUtil.considerGroundLayer)){
           Debug.DrawRay(rightFloorHit.point,rightFloorHit.normal);
           Vector3 bottom=actor.simActorCharacterController.characterController.bounds.center;
                   bottom.y=actor.simActorCharacterController.characterController.bounds.min.y;
           Plane floorPlane=new Plane(rightFloorHit.normal,bottom);
           Ray leftRay=new Ray(minLeft,Vector3.down);
           Debug.DrawRay(leftRay.origin,leftRay.direction,Color.blue);
           if(floorPlane.Raycast(leftRay,out float enter)){
            Vector3 leftFloorHitPoint=leftRay.GetPoint(enter);
            float minY=Mathf.Min(bottom.y,leftFloorHitPoint.y,rightFloorHit.point.y);
            posLerp.tgtPos.y+=minY-bottom.y;
            Debug.DrawLine(bottom,posLerp.tgtPos,Color.yellow);
           }
          }
         }
        }
        protected virtual void SetSimUMADataTransform(){
         actor.simUMA.transform.parent.rotation=rotLerp.UpdateRotation(actor.simUMA.transform.parent.rotation,Core.magicDeltaTimeNumber);
         actor.simUMA.transform.parent.position=posLerp.UpdatePosition(actor.simUMA.transform.parent.position,Core.magicDeltaTimeNumber);
        }
        protected virtual void GetAnimatorStateInfo(){
         //  [https://answers.unity.com/questions/1035587/how-to-get-current-time-of-an-animator.html]
         foreach(var layer in animatorClip){
          int layerIndex=layer.Key;
          List<AnimatorClipInfo>clipList=layer.Value;
          clipList.Clear();
          AnimatorStateInfo animatorState=animator.GetCurrentAnimatorStateInfo(layerIndex);
                                          animator.GetCurrentAnimatorClipInfo (layerIndex,clipList);
          if(clipList.Count>0){
           if(currentClipInstanceID[layerIndex]!=(currentClipInstanceID[layerIndex]=clipList[0].clip.GetInstanceID())||currentClipName[layerIndex]!=clipList[0].clip.name){
            //Log.DebugMessage("changed to new clipList[0].clip.name:"+clipList[0].clip.name+";clipList[0].clip.GetInstanceID():"+clipList[0].clip.GetInstanceID());
            OnAnimationChanged(layerIndex:layerIndex,lastClipName:currentClipName[layerIndex],currentClipName:clipList[0].clip.name);
            currentClipName[layerIndex]=clipList[0].clip.name;
            looped[layerIndex]=false;
           }
           lastLoopCount[layerIndex]=loopCount[layerIndex];
           if(loopCount[layerIndex]<(loopCount[layerIndex]=Mathf.FloorToInt(animatorState.normalizedTime))){
            //Log.DebugMessage("current animation (layerIndex:"+layerIndex+") looped:"+loopCount[layerIndex]);
            looped[layerIndex]=true;
            OnAnimationLooped(layerIndex:layerIndex,currentClipName:currentClipName[layerIndex]);
           }
           normalizedTime[layerIndex]=animatorState.normalizedTime;
            normalizedTimeInCurrentLoop[layerIndex]=Mathf.Repeat(animatorState.normalizedTime,1.0f);
           //Log.DebugMessage("current clipList[0].clip.name:"+clipList[0].clip.name);
           animationTime[layerIndex]=clipList[0].clip.length*normalizedTime[layerIndex];
            animationTimeInCurrentLoop[layerIndex]=clipList[0].clip.length*normalizedTimeInCurrentLoop[layerIndex];
           //Log.DebugMessage("current animationTime:"+animationTime[layerIndex]);
          }
         }
        }
     internal BaseAnimatorControllerMotionUpdater motionUpdater=null;
        protected virtual void UpdateMotion(BaseAI baseAI){
          if(lastMotion!=baseAI.motion){
           //Log.DebugMessage("actor motion will be set from:"+lastMotion+" to:"+baseAI.motion);
          }
          if(motionUpdater==null){
           if(actor.simUMA!=null){
            motionUpdater=actor.simUMA.transform.root.GetComponentInChildren<BaseAnimatorControllerMotionUpdater>();
           }
           if(motionUpdater!=null){
            motionUpdater.controller=this;
           }
          }
          if(motionUpdater!=null){
             motionUpdater.UpdateAnimatorWeaponLayer();
             motionUpdater.UpdateAnimatorMotionValue();
          }
          if(lastMotion!=baseAI.motion){
           //Log.DebugMessage("actor changed motion from:"+lastMotion+" to:"+baseAI.motion);
          }
          lastMotion=baseAI.motion;
        }
     Coroutine layerTransitionCoroutine;
      internal readonly Dictionary<int,float>layerTargetWeight=new Dictionary<int,float>();
       internal readonly Dictionary<int,float>layerWeight=new Dictionary<int,float>();
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
                weight-=8.0f*Core.magicDeltaTimeNumber;
                if(weight<=targetWeight){
                 weight=targetWeight;
                }
               }else if(weight<targetWeight){
                weight+=8.0f*Core.magicDeltaTimeNumber;
                if(weight>=targetWeight){
                 weight=targetWeight;
                }
               }
               animator.SetLayerWeight(layerIndex,layerWeight[layerIndex]=weight);
              }
             }
             yield return null;
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