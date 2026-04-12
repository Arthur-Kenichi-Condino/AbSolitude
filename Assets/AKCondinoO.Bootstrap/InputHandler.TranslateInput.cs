using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal partial class InputHandler{
     private Vector3 lastMousePosition;
     private bool hasLastMousePosition;
        private partial void TranslateInput(){
         if(!hasLastMousePosition||mousePosition!=lastMousePosition){
         }
         for(int i=0;i<bindings.Count;i++){
          var pair=bindings[i];
          var action=pair.action;
          var binding=pair.binding;
          //Logs.Debug("binding:"+binding);
          var enabledState=this.enabledState[action];
          enabledState.lastState=enabledState.curState;
          enabledState.lastStateFloat=enabledState.curStateFloat;
          switch(binding.combinationType){
           case(InputCombinationType.Chord):{
            for(int j=0;j<binding.inputCombination.Length;j++){
             var result=ReadInput(enabledState,binding.inputCombination[j]);
            }
            break;
           }
           case(InputCombinationType.Single):{
            var result=ReadInput(enabledState,binding.inputCombination[0]);
            if(result.value){
             Logs.Debug(()=>"action:"+action+":input received:"+binding);
             enabledState.curState=true;
             enabledState.curStateFloat=result.valueFloat;
            }else{
             enabledState.curState=false;
             enabledState.curStateFloat=0f;
            }
            break;
           }
          }
          if(enabledState.curState){
           if(inputInterpreter.Resolve(action,enabledState,mousePosition,out var intent)){
            GameOrchestrator.singleton.OnInputReceived(intent);
           }
          }
         }
         lastMousePosition=mousePosition;hasLastMousePosition=true;
        }
        private(bool value,float valueFloat)ReadInput(EnabledState enabledState,DeviceInput deviceInput){
         (bool value,float valueFloat)result=(false,0f);
         switch(deviceInput.mode){
          case(InputDetectionMode.Continuous):{
           float readValueFloat=0f;
           switch(deviceInput.source){
            case(DeviceInputSource.Mouse):{
             if(!string.IsNullOrEmpty(deviceInput.mouseInput)){
              readValueFloat=Input.GetAxis(deviceInput.mouseInput);
             }
             break;
            }
           }
           if(readValueFloat!=0f){
            Logs.Debug(()=>"deviceInput.mouseInput:"+deviceInput.mouseInput+":readValueFloat:"+readValueFloat);
            result.value=true;
            result.valueFloat=readValueFloat;
           }
           enabledState.axisValue=readValueFloat;
           return result;
          }
          case(InputDetectionMode.WhenDown):{
           bool readValue=false;
           switch(deviceInput.source){
            case(DeviceInputSource.Keyboard):{
             readValue=Input.GetKeyDown(deviceInput.key);
             break;
            }
            case(DeviceInputSource.Mouse):{
             readValue=Input.GetMouseButtonDown(deviceInput.mouseButton);
             break;
            }
           }
           if(readValue&&readValue!=enabledState.isDown){
            result.value=true;
           }
           enabledState.isDown=readValue;
           return result;
          }
         }
         return result;
        }
        internal class EnabledState{
         internal bool isDown;
         internal float axisValue;
         internal bool curState;internal bool lastState;
         internal float curStateFloat;internal float lastStateFloat;
        }
    }
}