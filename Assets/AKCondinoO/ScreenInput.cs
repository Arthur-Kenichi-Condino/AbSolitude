#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static AKCondinoO.InputHandler;
namespace AKCondinoO{
    internal partial class ScreenInput:MonoBehaviour,ISingletonInitialization{
     internal static ScreenInput singleton{get;set;}
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
     internal BaseAI currentActiveSim=null;
     internal GameObject currentSelectedGameObject;
     internal bool isInputFieldSelected;
     internal bool isPointerOverUIElement;
     internal PointerEventData pointerEventData;
      internal readonly List<RaycastResult>eventSystemRaycastResults=new List<RaycastResult>();
     internal Ray?screenPointRay;
     internal RaycastHit[]screenPointRaycastResults=new RaycastHit[1];
     internal int screenPointRaycastResultsCount=0;
        public void Init(){
         pointerEventData=new PointerEventData(EventSystem.current);
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("ScreenInput:OnDestroyingCoreEvent");
        }
     internal Vector3 mouse{get;private set;}
        void Update(){
         mouse=Input.mousePosition;
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
         if(Camera.main!=null){
          screenPointRay=Camera.main.ScreenPointToRay(Input.mousePosition);
          if(ScreenInput.singleton.screenPointRay!=null){
           _DoRaycast:{}
           screenPointRaycastResultsCount=Physics.RaycastNonAlloc(ScreenInput.singleton.screenPointRay.Value,screenPointRaycastResults);
           if(screenPointRaycastResultsCount>0&&screenPointRaycastResultsCount>=screenPointRaycastResults.Length){
            Array.Resize(ref screenPointRaycastResults,screenPointRaycastResultsCount*2);
            goto _DoRaycast;
           }
           Array.Sort(screenPointRaycastResults,HitsArraySortComparer);
          }
         }
        }
        //  ordena 'a' relativo a 'b', e retorna 'a' antes de 'b' se 'a' for menor que 'b'
        private int HitsArraySortComparer(RaycastHit a,RaycastHit b){
         if(a.collider==null&&b.collider==null){
          return 0;
         }
         if(a.collider==null&&b.collider!=null){
          return 1;
         }
         if(a.collider!=null&&b.collider==null){
          return -1;
         }
         return Vector3.Distance(transform.root.position,a.point).CompareTo(Vector3.Distance(transform.root.position,b.point));
        }
    }
}