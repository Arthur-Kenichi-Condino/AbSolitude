#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.Sims.Actors;
using AKCondinoO.UI.Fixed;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain;
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
     [SerializeField]internal RectTransform selectSimObjectButtonRect;
     internal Button selectSimObjectButton;
     internal Canvas canvas{get;private set;}
     internal CanvasScaler canvasScaler{get;private set;}
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         canvas      =transform.root.GetComponentInChildren<Canvas      >();
         canvasScaler=transform.root.GetComponentInChildren<CanvasScaler>();
         selectSimObjectButton=selectSimObjectButtonRect.GetComponent<Button>();
        }
        public void Init(){
         panel.gameObject.SetActive(false);
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("ContextMenuUI:OnDestroyingCoreEvent");
        }
     internal SimObject selectedSimObject=null;
     bool selectButtonPressed;
        public void OnSelectButtonPress(){
         Log.DebugMessage("ContextMenuUI:OnSelectButtonPress");
         selectedSimObject=contextSimObject;
         ScreenInput.singleton.currentActiveSim=null;
         if(selectedSimObject is BaseAI baseAI){
          ScreenInput.singleton.currentActiveSim=baseAI;
         }
         selectButtonPressed=true;
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
            VoxelTerrainChunk openForTerrain=null;
            bool open=false;
            if(ScreenInput.singleton.screenPointRaycastResultsCount>0){
             for(int i=0;i<ScreenInput.singleton.screenPointRaycastResultsCount;++i){
              RaycastHit hit=ScreenInput.singleton.screenPointRaycastResults[i];
              if(hit.collider==null){
               continue;
              }
              Log.DebugMessage("hit.collider.name:"+hit.collider.name+";hit.collider.gameObject.layer:"+LayerMask.LayerToName(hit.collider.gameObject.layer)+"(VoxelSystem.voxelTerrainLayer mask:"+VoxelSystem.voxelTerrainLayer+")");
              if(hit.collider.CompareTag("SimObjectVolume")){
               SimObject sim=hit.collider.transform.root.GetComponentInChildren<SimObject>();
               if(sim!=null){
                openFor=sim;
                open=true;
                break;
               }
              }
              if((VoxelSystem.voxelTerrainLayer&(1<<hit.collider.gameObject.layer))!=0){
               VoxelTerrainChunk voxelTerrain=hit.collider.transform.root.GetComponentInChildren<VoxelTerrainChunk>();
               if(voxelTerrain!=null){
                openForTerrain=voxelTerrain;
                open=true;
                break;
               }
              }
             }
            }
            if(open){
             if(openFor!=null){
              Open(openFor);
             }else if(openForTerrain!=null){
              Open(openForTerrain);
             }else{
              Close();
             }
            }else{
             Close();
            }
           }else{
            if(Enabled.ACTION_1.curState!=Enabled.ACTION_1.lastState){
             if(!ScreenInput.singleton.isPointerOverUIElement&&(ScreenInput.singleton.currentSelectedGameObject==null||ScreenInput.singleton.currentSelectedGameObject.transform.root!=this.transform.root)){
              Close();
             }else if(selectButtonPressed){
              Close();
             }
            }
           }
          }
         }
         selectButtonPressed=false;
        }
        void Close(){
         if(panel.gameObject.activeSelf){
          panel.gameObject.SetActive(false);
         }
        }
     internal SimObject contextSimObject=null;
        void Open(SimObject openFor){
         OnOpen();
         Log.DebugMessage("open panel for sim:"+openFor,openFor);
         contextSimObject=openFor;
         DoOpen();
        }
        void Open(VoxelTerrainChunk openForTerrain){
         OnOpen();
         Log.DebugMessage("open panel for terrain:"+openForTerrain,openForTerrain);
         DoOpen();
        }
        void OnOpen(){
         contextSimObject=null;
        }
        void DoOpen(){
         if(contextSimObject!=null){
          selectSimObjectButton.interactable=true;
         }else{
          selectSimObjectButton.interactable=false;
         }
         Vector3 pos=ScreenInput.singleton.mouse;
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