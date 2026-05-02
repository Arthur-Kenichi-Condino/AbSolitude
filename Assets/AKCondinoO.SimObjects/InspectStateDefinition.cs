using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors.SimInteractions;
using System;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
namespace AKCondinoO.SimObjects.StateMachines{
    internal struct InspectionContext{
     internal IInteractable interactable;
    }
    internal class InspectStateDefinition:StateDefinition{
     internal override(Type type,string definitionId)poolId=>(typeof(OpenObjectState),$"{GetInstanceID()}");
        internal override void RegisterState(){
         Logs.Debug(()=>"register:"+poolId);
         IState    .Register(poolId,()=>new InspectState(this));
        }
        internal override State SetupState(IInteractable interactable){
         var state=(InspectState)IState.Rent(poolId);
         var context=new InspectionContext(){
          interactable=interactable,
         };
         state.Setup(in context);
         return state;
        }
    }
    internal class InspectState:State,IState<InspectionContext>{
        internal InspectState(StateDefinition def):base(def){
        }
     protected InspectionContext context;
        public override void OnReturnToPoolRecycle(){
         context=default;
        }
        public virtual void Setup(in InspectionContext context){
         this.context=context;
        }
        public override void Enter(){
         base.Enter();
         Logs.Debug(()=>"open inspection window");
        }
        public override void Exit(){
         Logs.Debug(()=>"close inspection window");
         base.Exit();
        }
    }
}