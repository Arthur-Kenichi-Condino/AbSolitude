using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors.SimInteractions;
using System;
using UnityEngine;
namespace AKCondinoO.SimObjects.StateMachines{
    internal class CloseObjectStateDefinition:OpenObjectStateDefinition{
     internal override(Type type,string definitionId)poolId=>(typeof(CloseObjectState),$"{GetInstanceID()}_{closed.GetInstanceID()}_{open.GetInstanceID()}");
        internal override void RegisterState(){
         Logs.Debug(()=>"register:"+poolId+";closed:"+closed+";open:"+open);
         IState    .Register(poolId,()=>new CloseObjectState(this));
         IStateTask.Register(poolId,()=>new CloseObjectTask (this));
        }
        internal override State SetupState(IInteractable interactable){
         var state=(CloseObjectState)IState.Rent(poolId);
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
        public override bool IsTheStateOf(IInteractable interactable){
         if(interactable is MonoBehaviour monoBehaviour){
          var t=monoBehaviour.transform;
          return IsClosed(t,this);
         }
         return false;
        }
        internal static bool IsClosed(Transform target,CloseObjectStateDefinition def,float posThreshold=0.0001f,float scaleThreshold=0.0001f,float rotThreshold=0.0001f){
         if(Vector3.Distance(def.closed.localPosition,target.localPosition)>posThreshold  )return false;
         if(Vector3.Distance(def.closed.localScale   ,target.localScale   )>scaleThreshold)return false;
         if(Quaternion.Angle(def.closed.localRotation,target.localRotation)>rotThreshold  )return false;
         Logs.Debug(()=>"'IsClosed'");
         return true;
        }
    }
    internal class CloseObjectState:OpenObjectState{
        internal CloseObjectState(CloseObjectStateDefinition definition):base(definition){
        }
        public override void Enter(){
         if(context.target!=null){
          var task=(CloseObjectTask)IStateTask.Rent(definition.poolId);
          task.Setup(in context);
          tasks.Add(task);
         }
        }
    }
    internal class CloseObjectTask:OpenObjectTask{
        internal CloseObjectTask(CloseObjectStateDefinition definition):base(definition){
        }
        public override void Setup(in TransformTransitionContext context){
         this.context=context;
         startPos=context.target.localPosition;
         startRot=context.target.localRotation;
         startScale=context.target.localScale;
         endPos=context.closed.localPosition;
         endRot=context.closed.localRotation;
         endScale=context.closed.localScale;
         Logs.Debug(()=>"startPos:"+startPos+";endPos:"+endPos);
        }
        public override bool IsComplete=>CloseObjectStateDefinition.IsClosed(context.target,(CloseObjectStateDefinition)definition);
    }
}