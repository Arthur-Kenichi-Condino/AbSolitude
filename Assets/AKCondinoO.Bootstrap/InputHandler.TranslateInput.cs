using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal partial class InputHandler{
     private Vector3 lastMousePosition;
     private bool hasLastMousePosition;
     private GameMode currentGameMode;
     private List<(InputBinding binding,InputAction action)>currentBindingsList;
        private partial void TranslateInput(){
         consumedKeys           .Clear();
         consumedMouseButtons   .Clear();
         consumedMouseInputs    .Clear();
         consumedControllerInput.Clear();
         var gameMode=GameOrchestrator.singleton.gameMode;
         if(currentBindingsList==null||currentGameMode!=gameMode){
          currentGameMode=gameMode;
          bindings.TryGetValue(currentGameMode,out currentBindingsList);
         }
         if(!hasLastMousePosition||mousePosition!=lastMousePosition){
         }
         if(currentBindingsList!=null){
          for(int i=0;i<currentBindingsList.Count;i++){
           var pair=currentBindingsList[i];
           var action=pair.action;
           var binding=pair.binding;
           //Logs.Debug("binding:"+binding);
           ProcessBinding(binding,action);
          }
         }
         lastMousePosition=mousePosition;hasLastMousePosition=true;
        }
        internal void ProcessBinding(InputBinding binding,InputAction action){
         var enabledState=this.enabledState[action];
         enabledState.OnPreInputRead();
         bool triggered=binding.combinationType switch{
          InputCombinationType.Chord =>ProcessChord (binding,enabledState),
          InputCombinationType.Single=>ProcessSingle(binding,enabledState),
          _=>false
         };
         enabledState.OnPostInputRead();
         if(triggered){
          ConsumeCombination(binding.inputCombination);
         }
         TrySendIntent(action,binding,enabledState);
        }
        private void TrySendIntent(InputAction action,InputBinding binding,EnabledState state){
         if(!state.stateChanged)
          return;
         Logs.Debug(()=>"action:"+action+":input state changed:"+binding);
         if(inputInterpreter.Resolve(action,state,mousePosition,out var intent)){
          GameOrchestrator.singleton.OnInputReceived(intent);
         }
        }
        private bool ProcessChord(InputBinding binding,EnabledState state){
         var inputs=binding.inputCombination;
         int len=inputs.Length;
         for(int i=0;i<len;i++){
          if(IsConsumed(inputs[i])){
           ClearState(state,len);
           return false;
          }
         }
         for(int i=0;i<len;i++){
          ReadInput(state,inputs[i],i);
         }
         for(int i=0;i<len;i++){
          if(!state.curState[i]){
           ClearState(state,len);
           return false;
          }
         }
         FillState(state,len);
         return true;
        }
        private void ClearState(EnabledState state,int len){
         for(int i=0;i<len;i++){
          state.curState[i]=false;
         }
        }
        private void FillState(EnabledState state,int len){
         for(int i=0;i<len;i++){
          state.curState[i]=true;
         }
        }
        private bool ProcessSingle(InputBinding binding,EnabledState state){
         var input=binding.inputCombination[0];
         if(IsConsumed(input)){
          state.curState[0]=false;
          return false;
         }
         ReadInput(state,input,0);
         return state.curState[0];
        }
     private readonly HashSet<KeyCode>consumedKeys           =new();
     private readonly HashSet<int    >consumedMouseButtons   =new();
     private readonly HashSet<string >consumedMouseInputs    =new();
     private readonly HashSet<string >consumedControllerInput=new();
        private bool IsConsumed(DeviceInput input){
         switch(input.source){
          case DeviceInputSource.Keyboard:
           if(consumedKeys.Contains(input.key))
            return true;
          break;
          case DeviceInputSource.Mouse:
           if(!string.IsNullOrEmpty(input.mouseInput)){
            if(consumedMouseInputs.Contains(input.mouseInput))
             return true;
           }else{
            if(consumedMouseButtons.Contains(input.mouseButton))
             return true;
           }
          break;
          case DeviceInputSource.Controller:
           if(consumedControllerInput.Contains(input.controllerInput))
            return true;
          break;
         }
         return false;
        }
        private void ConsumeCombination(DeviceInput[]inputCombination){
         for(int i=0;i<inputCombination.Length;i++){
          var input=inputCombination[i];
          switch(input.source){
           case DeviceInputSource.Keyboard:
            consumedKeys.Add(input.key);
           break;
           case DeviceInputSource.Mouse:
            if(!string.IsNullOrEmpty(input.mouseInput)){
             consumedMouseInputs.Add(input.mouseInput);
            }else{
             consumedMouseButtons.Add(input.mouseButton);
            }
           break;
           case DeviceInputSource.Controller:
            if(!string.IsNullOrEmpty(input.controllerInput))
             consumedControllerInput.Add(input.controllerInput);
           break;
          }
         }
        }
        private void ReadInput(EnabledState enabledState,DeviceInput deviceInput,int combinationIndex){
         switch(deviceInput.mode){
          case(InputDetectionMode.WhenDown):{
           ref var isDown   =ref enabledState.isDown   [combinationIndex];
           ref var curState =ref enabledState.curState [combinationIndex];
           ref var lastState=ref enabledState.lastState[combinationIndex];
           bool wasDown=isDown;
           switch(deviceInput.source){
            case(DeviceInputSource.Keyboard):{
             isDown=Input.GetKey(deviceInput.key);
             break;
            }
            case(DeviceInputSource.Mouse):{
             isDown=Input.GetMouseButton(deviceInput.mouseButton);
             break;
            }
           }
           if(isDown&&!wasDown){
            curState=true;
           }else{
            curState=false;
           }
           return;
          }
          case(InputDetectionMode.WhileHeld):{
           switch(deviceInput.source){
            case(DeviceInputSource.Keyboard):{
             enabledState.curState[combinationIndex]=Input.GetKey(deviceInput.key);
             break;
            }
            case(DeviceInputSource.Mouse):{
             enabledState.curState[combinationIndex]=Input.GetMouseButton(deviceInput.mouseButton);
             break;
            }
           }
           return;
          }
          case(InputDetectionMode.Continuous):{
           switch(deviceInput.source){
            case(DeviceInputSource.Mouse):{
             if(!string.IsNullOrEmpty(deviceInput.mouseInput)){
              enabledState.curStateFloat[combinationIndex]=Input.GetAxis(deviceInput.mouseInput);
              enabledState.curState[combinationIndex]=true;
              //Logs.Debug(()=>"deviceInput.mouseInput:"+deviceInput.mouseInput+":'enabledState.curStateFloat':"+enabledState.curStateFloat[combinationIndex]);
             }
             break;
            }
           }
           return;
          }
         }
         return;
        }
        internal class EnabledState{
         internal bool[]isDown;
         internal bool[]curState;
         internal bool[]lastState;
         internal float[]curStateFloat;
         internal float[]lastStateFloat;
         internal bool stateChanged;
            internal EnabledState(int maxCombinationLength){
             isDown   =new bool[maxCombinationLength];
             curState =new bool[maxCombinationLength];
             lastState=new bool[maxCombinationLength];
             curStateFloat =new float[maxCombinationLength];
             lastStateFloat=new float[maxCombinationLength];
            }
            internal void OnPreInputRead(){
             var swapState=lastState;
             lastState=curState;
             curState=swapState;
             var swapStateFloat=lastStateFloat;
             lastStateFloat=curStateFloat;
             curStateFloat=swapStateFloat;
            }
            internal void OnPostInputRead(){
             stateChanged=false;
             for(int i=0;i<curStateFloat.Length;i++){
              if(curState[i]!=lastState[i]){
               stateChanged=true;
               break;
              }
              if(curStateFloat[i]!=lastStateFloat[i]){
               stateChanged=true;
               break;
              }
             }
            }
        }
    }
}