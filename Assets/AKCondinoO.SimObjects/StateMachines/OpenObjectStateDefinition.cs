using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors.SimInteractions;
using System;
using UnityEngine;
namespace AKCondinoO.SimObjects.StateMachines{
    internal class OpenObjectStateDefinition:StateDefinition{
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
         state.interactable=interactable;
         state.closed=closed;
         state.open=open;
         state.duration=duration;
         return state;
        }
    }
    internal class OpenObjectState:State{
        internal OpenObjectState(OpenObjectStateDefinition definition):base(definition){
        }
     internal IInteractable interactable;
     internal Transform closed;
     internal Transform open;
     internal float duration;
     private Transform interactableTransform;
        public override void OnReturnToPoolRecycle(){
         interactable=null;
         closed=null;
         open=null;
         duration=0f;
         interactableTransform=null;
        }
        public override void Enter(){
         if(interactable is MonoBehaviour monoBehaviour){
          interactableTransform=monoBehaviour.transform;
          var task=(OpenObjectTask)IStateTask.Rent(definition.poolId);
          task.interactable=interactable;
          task.closed=closed;
          task.open=open;
          task.duration=duration;
          task.interactableTransform=interactableTransform;
          tasks.Add(task);
         }
        }
    }
    internal class OpenObjectTask:StateTask{
        internal OpenObjectTask(OpenObjectStateDefinition definition):base(definition){
        }
     internal IInteractable interactable;
     internal Transform closed;
     internal Transform open;
     internal float duration;
     internal Transform interactableTransform;
        public override void OnReturnToPoolRecycle(){
         interactable=null;
         closed=null;
         open=null;
         duration=0f;
         interactableTransform=null;
         started=false;
         t=0f;
        }
     bool started=false;
     float t;
     Vector3 startPos;
        public override void Tick(float dt){
         if(!started){
          startPos=interactableTransform.localPosition;
          started=true;
         }
         t=Mathf.MoveTowards(t,1f,Time.deltaTime*.5f);
         interactableTransform.localPosition=Vector3.Lerp(
          startPos,
          open.localPosition,
          t
         );
        }
        public override bool IsComplete=>IsOpen();
        private bool IsOpen(float threshold=0.0001f){
         if((open.localPosition-interactableTransform.localPosition).sqrMagnitude>threshold)return false;
         if((open.localScale   -interactableTransform.localScale   ).sqrMagnitude>threshold)return false;
         if(Mathf.Abs(Quaternion.Dot(open.localRotation,interactableTransform.localRotation))<1.0f-threshold)return false;
         return true;
        }
    }
}