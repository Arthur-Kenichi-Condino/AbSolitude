using AKCondinoO.SimActors.SimInteractions;
using AKCondinoO.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.SimObjects.StateMachines{
    internal class StateDefinition:MonoBehaviour{
     internal virtual(Type type,string definitionId)poolId=>(typeof(State),$"{GetInstanceID()}");
        internal virtual void RegisterState(){
        }
        internal virtual State SetupState(IInteractable interactable){
         return null;
        }
    }
    internal interface IStateEvaluator{
        bool Evaluate(IInteractable interactable);
    }
    internal interface IState<TContext>{
        void Setup(in TContext context);
    }
    internal class State:IState{
     protected readonly StateDefinition definition;
        internal State(StateDefinition definition){
         this.definition=definition;
        }
     protected readonly List<IStateTask>tasks=new();
     public bool IsComplete=>tasks.Count<=0||tasks.All(t=>t.IsComplete);
        public virtual void OnReturnToPoolRecycle(){
        }
        public virtual void Enter(){
        }
        public virtual void Tick(float dt){
         foreach(var t in tasks)
          t.Tick(dt);
        }
        public virtual void Exit(){
         tasks.Clear();
        }
    }
    internal interface IState{
     internal static readonly Dictionary<(Type,string),ObjectPoolBase>pool=new();
        internal static void Register((Type type,string definitionId)poolId,Func<IState>factory){
         if(pool.ContainsKey(poolId)){return;}
         pool.Add(poolId,Pool.GetPool<IState>(poolId.definitionId,factory,(IState item)=>{item.OnReturnToPoolRecycle();}));
        }
        internal static IState Rent((Type type,string definitionId)poolId){
         return(IState)pool[poolId].ObjectRent();
        }
        internal static void Return((Type type,string definitionId)poolId,IState state){
         pool[poolId].ObjectReturn(state);
        }
        void OnReturnToPoolRecycle();
        void Enter();
        void Tick(float dt);
     bool IsComplete{get;}
        void Exit();
    }
    internal class StateTask:IStateTask{
     protected readonly StateDefinition definition;
        internal StateTask(StateDefinition definition){
         this.definition=definition;
        }
        public virtual void OnReturnToPoolRecycle(){
        }
        public virtual void Tick(float dt){
        }
        public virtual bool IsComplete=>true;
    }
    internal interface IStateTask<TContext>{
        void Setup(in TContext context);
    }
    interface IStateTask{
     internal static readonly Dictionary<(Type,string),ObjectPoolBase>pool=new();
        internal static void Register((Type type,string definitionId)poolId,Func<IStateTask>factory){
         if(pool.ContainsKey(poolId)){return;}
         pool.Add(poolId,Pool.GetPool<IStateTask>(poolId.definitionId,factory,(IStateTask item)=>{item.OnReturnToPoolRecycle();}));
        }
        internal static IStateTask Rent((Type type,string definitionId)poolId){
         return(IStateTask)pool[poolId].ObjectRent();
        }
        internal static void Return((Type type,string definitionId)poolId,IStateTask state){
         pool[poolId].ObjectReturn(state);
        }
        void OnReturnToPoolRecycle();
        void Tick(float dt);
        bool IsComplete{get;}
    }
}