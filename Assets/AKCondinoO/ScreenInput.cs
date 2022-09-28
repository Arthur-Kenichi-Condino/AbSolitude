#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace AKCondinoO{
    internal class ScreenInput:MonoBehaviour,ISingletonInitialization{
     internal static ScreenInput singleton{get;set;}
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
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
        }
    }
}