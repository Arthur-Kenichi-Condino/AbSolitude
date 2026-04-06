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
        internal bool Resolve(InputAction action,EnabledState enabledState,Vector3 mousePosition,out InputIntent intent){
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
         bool valid=false;
         DetectPointerOverUI(ref intent);
         DoRaycastFromCamera(ref intent,mousePosition);
         intent.target=null;
         var mouseOverObject=intent.mouseOverObject;
         if(mouseOverObject){
          var mouseHit=intent.mouseHit;
          intent.target=mouseHit.collider.gameObject;
          valid=true;
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
          Ray ray=mainCamera.ScreenPointToRay(mousePosition);
          intent.mouseOverObject=Physics.Raycast(ray,out intent.mouseHit);
         }
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
         internal bool isPointerOverUI;
         internal bool mouseOverObject;
         internal RaycastHit mouseHit;
         internal GameObject target;
        }
    }
}