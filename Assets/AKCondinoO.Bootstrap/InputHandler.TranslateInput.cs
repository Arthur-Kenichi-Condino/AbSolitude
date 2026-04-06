using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal partial class InputHandler{
     private Vector3 lastMousePosition;
     private bool hasLastMousePosition;
        private partial void TranslateInput(){
         if(!hasLastMousePosition||!Mathf.Approximately((mousePosition-lastMousePosition).sqrMagnitude,0f)){
         }
         for(int i=0;i<bindings.Count;i++){
          var pair=bindings[i];
          var action=pair.action;
          var binding=pair.binding;
          //Logs.Debug("binding:"+binding);
          var enabledState=this.enabledState[action];
          enabledState.lastState=enabledState.curState;
          switch(binding.combinationType){
           case(InputCombinationType.Single):{
            bool result;
            result=ReadInput(enabledState,binding.inputCombination[0]);
            if(result){
             Logs.Debug(()=>"action:"+action+":input received:"+binding);
             enabledState.curState=true;
            }else{
             enabledState.curState=false;
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
        private bool ReadInput(EnabledState enabledState,DeviceInput deviceInput){
         switch(deviceInput.mode){
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
           bool result=false;
           if(readValue&&readValue!=enabledState.isDown){
            result=true;
           }
           enabledState.isDown=readValue;
           return result;
          }
         }
         return false;
        }
        internal class EnabledState{
         internal bool isDown;
         internal bool curState;internal bool lastState;
        }
    }
}