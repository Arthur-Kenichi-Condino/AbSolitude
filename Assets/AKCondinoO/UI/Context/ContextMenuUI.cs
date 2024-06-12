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
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AKCondinoO.GameMode;
using static AKCondinoO.InputHandler;
namespace AKCondinoO.UI.Context{
    internal class ContextMenuUI:MonoBehaviour,ISingletonInitialization{
     internal static ContextMenuUI singleton{get;set;}
     [SerializeField]internal RectTransform panel;
     [SerializeField]internal RectTransform content;
     [SerializeField]internal RectTransform simObjectNamePanelRect;
     [SerializeField]internal TMP_Text simObjectNamePanelText;
     [SerializeField]internal RectTransform selectSimObjectButtonRect;
     internal Button selectSimObjectButton;
     internal Canvas canvas{get;private set;}
     internal CanvasScaler canvasScaler{get;private set;}
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         canvas      =transform.root.GetComponentInChildren<Canvas      >();
         canvasScaler=transform.root.GetComponentInChildren<CanvasScaler>();
         selectSimObjectButton=selectSimObjectButtonRect.GetComponent<Button>();
         interactionButtonPrefab=interactionButtonPrefabRect.GetComponent<Button>();
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
         SetSelectedSimObject(contextSimObject);
         selectButtonPressed=true;
        }
        internal void SetSelectedSimObject(SimObject simObject){
         selectedSimObject=simObject;
         ScreenInput.singleton.SetActiveSim(selectedSimObject);
        }
     internal bool isOpen;
        void OnGUI(){
         if(isOpen){
          if(!panel.gameObject.activeSelf){
           panel.gameObject.SetActive(true);
          }
         }else{
          if(panel.gameObject.activeSelf){
           panel.gameObject.SetActive(false);
          }
         }
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
         isOpen=false;
        }
     internal SimObject contextSimObject=null;
        void Open(SimObject openFor){
         OnOpen();
         Log.DebugMessage("open panel for sim:"+openFor,openFor);
         contextSimObject=openFor;
         DoOpen();
        }
     internal VoxelTerrainChunk contextTerrainChunk=null;
        void Open(VoxelTerrainChunk openForTerrain){
         OnOpen();
         Log.DebugMessage("open panel for terrain:"+openForTerrain,openForTerrain);
         contextTerrainChunk=openForTerrain;
         DoOpen();
        }
        void OnOpen(){
         //  limpeza...
         contextSimObject=null;
         contextTerrainChunk=null;
        }
        void DoOpen(){
         SetInteractionsScrollViewContent();
         if(contextSimObject!=null){
          selectSimObjectButton.interactable=true;
         }else{
          selectSimObjectButton.interactable=false;
         }
         if(contextSimObject!=null){
          simObjectNamePanelText.text=contextSimObject.ContextName();
         }else if(contextTerrainChunk!=null){
          simObjectNamePanelText.text=contextTerrainChunk.ContextName();
         }else{
          simObjectNamePanelText.text="...";
         }
         Vector3 pos=ScreenInput.singleton.mouse;
         Vector2 size=panel.ActualSize(canvas);
         Log.DebugMessage("panel size:"+size);
         pos.x+=size.x/2f;
         pos.y-=size.y/2f;
         panel.position=new Vector2(pos.x,pos.y);
         isOpen=true;
        }
     [SerializeField]internal RectTransform interactionButtonPrefabRect;
     internal Button interactionButtonPrefab;
     internal readonly List<Button>interactionButtons=new List<Button>();
        void SetInteractionsScrollViewContent(){
         List<Interaction>interactions=null;
         if(contextSimObject!=null){
          contextSimObject.GetInteractions(out interactions);
         }else if(contextTerrainChunk!=null){
          contextTerrainChunk.GetInteractions(out interactions);
         }
         int interactionsCount=0;
         if(interactions!=null){
          interactionsCount=interactions.Count;
         }
         int count=Mathf.Max(interactionsCount,interactionButtons.Count);
         for(int i=0;i<count;i++){
          Interaction interaction=null;
          if(i<interactionsCount){
           interaction=interactions[i];
          }
          if(interaction!=null){
           Button button;
           if(i<interactionButtons.Count){
            button=interactionButtons[i];
           }else{
            button=Instantiate(interactionButtonPrefab,content);
            RectTransform rectTransform=button.GetComponent<RectTransform>();
            Log.DebugMessage("rectTransform.anchoredPosition:"+rectTransform.anchoredPosition);
            float posY=rectTransform.anchoredPosition.y;
            posY-=i*rectTransform.rect.height;
            rectTransform.anchoredPosition=new Vector2(rectTransform.anchoredPosition.x,posY);
            interactionButtons.Add(button);
           }
           var text=button.GetComponentInChildren<TMP_Text>();
           text.text=interaction.ToString();
           button.gameObject.SetActive(true);
          }else if(i<interactionButtons.Count){
           Button button=interactionButtons[i];
           button.gameObject.SetActive(false);
          }
         }
        }
    }
}