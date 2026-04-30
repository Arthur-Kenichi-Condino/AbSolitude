using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors.SimInteractions;
using System;
using UnityEngine;
namespace AKCondinoO.SimObjects.StateMachines{
    internal struct TransformTransitionContext{
     internal IInteractable interactable;
     public Transform closed;
     public Transform open;
     public float duration;
     public Transform target;
    }
    internal class OpenObjectStateDefinition:StateDefinition,IStateEvaluator{
     [SerializeField]internal Transform closed;
     [SerializeField]internal Transform open;
     [SerializeField]internal float duration;
     internal override(Type type,string definitionId)poolId=>(typeof(OpenObjectState),$"{GetInstanceID()}_{closed.GetInstanceID()}_{open.GetInstanceID()}");
     
        internal override void RegisterState(){
         Logs.Debug(()=>"register:"+poolId+";closed:"+closed+";open:"+open);
         IState    .Register(poolId,()=>new OpenObjectState(this));
         IStateTask.Register(poolId,()=>new OpenObjectTask (this));
        }
        internal override State SetupState(IInteractable interactable){
         var state=(OpenObjectState)IState.Rent(poolId);
         var context=new TransformTransitionContext(){
          interactable=interactable,
          closed=closed,
          open=open,
          duration=duration,
         };
         if(interactable is MonoBehaviour monoBehaviour){
          context.target=monoBehaviour.transform;
         }
         state.Setup(in context);
         return state;
        }
        public virtual bool Evaluate(IInteractable interactable){
         if(interactable is MonoBehaviour monoBehaviour){
          var t=monoBehaviour.transform;
          return IsOpen(t,this);
         }
         return false;
        }
        internal static bool IsOpen(Transform target,OpenObjectStateDefinition def,float posThreshold=0.0001f,float scaleThreshold=0.0001f,float rotThreshold=0.0001f){
         if(Vector3.Distance(def.open.localPosition,target.localPosition)>posThreshold  )return false;
         if(Vector3.Distance(def.open.localScale   ,target.localScale   )>scaleThreshold)return false;
         if(Quaternion.Angle(def.open.localRotation,target.localRotation)>rotThreshold  )return false;
         Logs.Debug(()=>"'IsOpen'");
         return true;
        }
    }
    internal class OpenObjectState:State,IState<TransformTransitionContext>{
        internal OpenObjectState(OpenObjectStateDefinition definition):base(definition){
        }
     protected TransformTransitionContext context;
        public override void OnReturnToPoolRecycle(){
         context=default;
        }
        public virtual void Setup(in TransformTransitionContext context){
         this.context=context;
        }
        public override void Enter(){
         if(context.target!=null){
          var task=(OpenObjectTask)IStateTask.Rent(definition.poolId);
          task.Setup(in context);
          tasks.Add(task);
         }
        }
    }
    internal class OpenObjectTask:StateTask,IStateTask<TransformTransitionContext>{
        internal OpenObjectTask(OpenObjectStateDefinition definition):base(definition){
        }
     protected TransformTransitionContext context;
        public override void OnReturnToPoolRecycle(){
         context=default;
         started=false;
         t=0f;
         startPos=default;
         startRot=default;
         startScale=default;
         endPos=default;
         endRot=default;
         endScale=default;
        }
        public virtual void Setup(in TransformTransitionContext context){
         this.context=context;
         startPos=context.target.localPosition;
         startRot=context.target.localRotation;
         startScale=context.target.localScale;
         endPos=context.open.localPosition;
         endRot=context.open.localRotation;
         endScale=context.open.localScale;
         Logs.Debug(()=>"startPos:"+startPos+";endPos:"+endPos);
        }
     protected bool started=false;
     protected float t;
     protected Vector3    startPos;
     protected Quaternion startRot;
     protected Vector3    startScale;
     protected Vector3    endPos;
     protected Quaternion endRot;
     protected Vector3    endScale;
        public override void Tick(float dt){
         if(!started){
          started=true;
         }
         if(context.duration<=0f){
          t=1f;
         }else{
          t+=dt/context.duration;
         }
         t=Mathf.Clamp01(t);
         context.target.localPosition=Vector3.Lerp(
          startPos,
          endPos,
          t
         );
         context.target.localRotation=Quaternion.Slerp(
          startRot,
          endRot,
          t
         );
         context.target.localScale=Vector3.Lerp(
          startScale,
          endScale,
          t
         );
        }
        public override bool IsComplete=>OpenObjectStateDefinition.IsOpen(context.target,(OpenObjectStateDefinition)definition);
    }
}