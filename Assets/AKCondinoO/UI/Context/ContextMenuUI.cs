#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.Sims.Actors;
using AKCondinoO.UI.Fixed;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static AKCondinoO.GameMode;
using static AKCondinoO.InputHandler;
namespace AKCondinoO.UI.Context{
    internal class ContextMenuUI:MonoBehaviour,ISingletonInitialization{
     internal static ContextMenuUI singleton{get;set;}
     [SerializeField]internal RectTransform panel;
     [SerializeField]internal RectTransform selectSimObjectButton;
     internal Canvas canvas{get;private set;}
     internal CanvasScaler canvasScaler{get;private set;}
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         canvas      =transform.root.GetComponentInChildren<Canvas      >();
         canvasScaler=transform.root.GetComponentInChildren<CanvasScaler>();
        }
        public void Init(){
         panel.gameObject.SetActive(false);
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("ContextMenuUI:OnDestroyingCoreEvent");
        }
     internal SimObject selectedSimObject=null;
        public void OnSelectButtonPress(){
         Log.DebugMessage("ContextMenuUI:OnSelectButtonPress");
         selectedSimObject=contextSimObject;
        }
        void Update(){
         if(Cursor.lockState==CursorLockMode.Locked){
          Close();
         }else{
          if(ScreenInput.singleton.screenPointRay==null){
           Close();
          }else{
           if(Enabled.ACTION_2.curState!=Enabled.ACTION_2.lastState){
            SimObject openFor=null;
            bool open=false;
            if(ScreenInput.singleton.screenPointRaycastResultsCount>0){
             for(int i=0;i<ScreenInput.singleton.screenPointRaycastResultsCount;++i){
              RaycastHit hit=ScreenInput.singleton.screenPointRaycastResults[i];
              if(hit.collider==null||!hit.collider.CompareTag("SimObjectVolume")){
               continue;
              }
              SimObject sim=hit.collider.transform.root.GetComponentInChildren<SimObject>();
              if(sim!=null){
               openFor=sim;
               open=true;
               break;
              }
             }
            }
            if(open){
             Open(openFor);
            }else{
             Close();
            }
           }else{
            if(Enabled.ACTION_1.curState!=Enabled.ACTION_1.lastState){
             if(!ScreenInput.singleton.isPointerOverUIElement&&(ScreenInput.singleton.currentSelectedGameObject==null||ScreenInput.singleton.currentSelectedGameObject.transform.root!=this.transform.root)){
              Close();
             }
            }
           }
          }
         }
        }
        void Close(){
         if(panel.gameObject.activeSelf){
          panel.gameObject.SetActive(false);
         }
        }
     internal SimObject contextSimObject=null;
        void Open(SimObject openFor){
         Log.DebugMessage("open panel for sim:"+openFor);
         contextSimObject=openFor;
         Vector3 pos=ScreenInput.singleton.mouse;
         //Vector3[]v=null;
         //Vector2 size=panel.ActualSize2(ref v);
         //Log.DebugMessage("canvasScaler:"+canvasScaler);
         Vector2 size=panel.ActualSize(canvas);
         Log.DebugMessage("panel size:"+size);
         pos.x+=size.x/2f;
         pos.y-=size.y/2f;
         panel.position=new Vector2(pos.x,pos.y);
         if(!panel.gameObject.activeSelf){
          panel.gameObject.SetActive(true);
         }
        }
    }
}