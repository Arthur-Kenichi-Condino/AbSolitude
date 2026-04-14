using AKCondinoO.UIObjects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static AKCondinoO.Bootstrap.InputHandler;
namespace AKCondinoO.Bootstrap{
    internal class InputInterpreter{
     private readonly InputHandler inputHandler;
        internal InputInterpreter(InputHandler inputHandler){
         this.inputHandler=inputHandler;
        }
     private Camera mainCamera;
     private EventSystem currentEventSystem;
     private PointerEventData pointerEventData;
        internal bool Resolve(InputAction action,InputBinding binding,EnabledState enabledState,Vector3 mousePosition,out InputIntent intent){
         if(mainCamera==null){
          Logs.Debug(()=>"'set Camera.main'");
          mainCamera=Camera.main;
         }
         if((object)currentEventSystem!=EventSystem.current){
          Logs.Debug(()=>"'set current EventSystem'");
          currentEventSystem=EventSystem.current;
          pointerEventData=new PointerEventData(currentEventSystem);
         }
         intent=default;
         intent.action=action;
         intent.enabledState=enabledState;
         for(int i=0;i<binding.inputCombination.Length;i++){
          var input=binding.inputCombination[i];
          switch(input.axisRole){
           case AxisRole.LookX:
            intent.lookX=enabledState.curStateFloat[i];
           break;
           case AxisRole.LookY:
            intent.lookY=enabledState.curStateFloat[i];
           break;
           case AxisRole.Zoom:
            intent.zoom=enabledState.curStateFloat[i];
           break;
          }
         }
         bool valid=true;
         DetectPointerOverUI(ref intent);
         DoRaycastFromCamera(ref intent,mousePosition);
         intent.mouseTarget=null;
         var mouseOverObject=intent.mouseOverObject;
         if(mouseOverObject){
          var mouseHit=intent.mouseHit;
          intent.mouseTarget=mouseHit.collider.gameObject;
         }
         if(intent.isPointerOverUI){
          valid=false;
         }
         return valid;
        }
        private void DetectPointerOverUI(ref InputIntent intent){
         if((object)currentEventSystem!=null){
          intent.isPointerOverUI=currentEventSystem.IsPointerOverGameObject();
         }
        }
        private void DoRaycastFromCamera(ref InputIntent intent,Vector3 mousePosition){
         if((object)mainCamera!=null){
          if(IsValidMousePosition(mousePosition)){
           Ray ray=mainCamera.ScreenPointToRay(mousePosition);
           intent.mouseOverObject=Physics.Raycast(ray,out intent.mouseHit);
          }
          Vector3 screenCenter=new Vector3(
           Screen.width *0.5f,
           Screen.height*0.5f,
           0f
          );
          Ray cameraRay=mainCamera.ScreenPointToRay(screenCenter);
          intent.cameraHitObject=Physics.Raycast(cameraRay,out intent.cameraHit);
          intent.cameraForwardEndPoint=mainCamera.transform.position+mainCamera.transform.forward*mainCamera.farClipPlane;
         }
        }
        internal bool IsValidMousePosition(Vector3 v){
         return!(float.IsNaN(v.x)||float.IsNaN(v.y)||float.IsNaN(v.z)||
          float.IsInfinity(v.x)||float.IsInfinity(v.y)||float.IsInfinity(v.z));
        }
     private readonly List<RaycastResult>eventSystemRaycastResults=new();
        private void GetPointerUIObjects(Vector3 mousePosition){
         if((object)currentEventSystem!=null){
          pointerEventData.position=mousePosition;
          eventSystemRaycastResults.Clear();
          currentEventSystem.RaycastAll(pointerEventData,eventSystemRaycastResults);
          foreach(var raycastResult in eventSystemRaycastResults){
           if(raycastResult.gameObject.layer==UISystem.singleton.uiLayer){
           }
          }
         }
        }
        internal struct InputIntent{
         internal InputAction action;
         internal EnabledState enabledState;
         internal float lookX;
         internal float lookY;
         internal float zoom;
         internal bool isPointerOverUI;
         internal bool mouseOverObject;
         internal RaycastHit mouseHit;
         internal GameObject mouseTarget;
         internal bool cameraHitObject;
         internal RaycastHit cameraHit;
         internal Vector3 cameraForwardEndPoint;
        }
    }
}