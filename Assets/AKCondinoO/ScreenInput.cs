#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static AKCondinoO.InputHandler;
namespace AKCondinoO{
    internal class ScreenInput:MonoBehaviour,ISingletonInitialization{
     internal static ScreenInput singleton{get;set;}
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
     internal GameObject currentSelectedGameObject;
     internal bool isInputFieldSelected;
     internal bool isPointerOverUIElement;
     internal PointerEventData pointerEventData;
      internal readonly List<RaycastResult>eventSystemRaycastResults=new List<RaycastResult>();
        public void Init(){
         pointerEventData=new PointerEventData(EventSystem.current);
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("ScreenInput:OnDestroyingCoreEvent");
        }
        void Update(){
         pointerEventData.position=Input.mousePosition;
         isPointerOverUIElement=false;
         eventSystemRaycastResults.Clear();
         EventSystem.current.RaycastAll(pointerEventData,eventSystemRaycastResults);
         foreach(var raycastResult in eventSystemRaycastResults){
          if(raycastResult.gameObject.layer==Util.UILayer){
           isPointerOverUIElement=true;
          }
         }
         if(currentSelectedGameObject!=EventSystem.current.currentSelectedGameObject){
          Log.DebugMessage("selected:"+EventSystem.current.currentSelectedGameObject);
          currentSelectedGameObject=EventSystem.current.currentSelectedGameObject;
         }
         //Log.DebugMessage("currentSelectedGameObject:"+currentSelectedGameObject);
         if(currentSelectedGameObject!=null){
          Enabled.RELEASE_MOUSE.curState=true;
         }
         if(!Enabled.RELEASE_MOUSE.curState){
          if(Cursor.lockState!=CursorLockMode.Locked){
           Cursor.visible=false;
           Cursor.lockState=CursorLockMode.Locked;
          }
         }else{
          if(Cursor.lockState==CursorLockMode.Locked){
           Cursor.visible=true;
           Cursor.lockState=CursorLockMode.None;
          }
         }
        }
    }
}