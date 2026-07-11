using AKCondinoO.Bootstrap;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static AKCondinoO.UIObjects.UISystem;
using static AKCondinoO.UIObjects.Window;
namespace AKCondinoO.UIObjects{
    internal class Minimized:UIObjectModule,
     IPointerDownHandler,
     IBeginDragHandler,
     IDragHandler,
     IEndDragHandler,
     IPointerClickHandler
    {
     private Button btn;
        public override void OnAwake(UIObject root){
         base.OnAwake(root);
         btn=GetComponent<Button>();
        }
     internal Window window;
        internal void RegisterWindow(Window window){
         this.window=window;
        }
     protected override bool shouldAutoKeepSafe{
      get{
       if(!wasDragged){
        return true;
       }
       return false;
      }
     }
        public override void OnManualUpdate(){
         base.OnManualUpdate();
        }
     internal bool wasDragged{get;private set;}
        public void OnPointerDown(PointerEventData eventData){
         wasDragged=false;
        }
        public void OnBeginDrag(PointerEventData eventData){
         wasDragged=true;
         window.OnUndocking();
         movedAfterMinimize=true;
        }
        public void OnDrag(PointerEventData eventData){
         ((RectTransform)transform).anchoredPosition+=(eventData.delta/root.canvas.scaleFactor);
        }
     internal bool dockedFromButtonDrag;
        public void OnEndDrag(PointerEventData eventData){
         var minimizedRect=((RectTransform)transform);
         SetSafePos(minimizedRect.anchoredPosition);
         wasDragged=false;
         if(IsNearCanvasEdgeLocalSpace(minimizedRect.anchoredPosition,root.canvas,out bool left,out bool right,out bool bottom,out bool top,new(64f,64f))){
          dockedFromButtonDrag=true;
          window.OnDocking();
          Logs.Debug(()=>"'IsNearCanvasEdgeLocalSpace':"+window.dockingState);
          redocked=true;
         }
        }
        public void OnPointerClick(PointerEventData eventData){
         if(wasDragged)
          return;
         UISystem.singleton.windowDockManager.Restore(this,window);
        }
     internal Vector2 previousWindowPos;
     internal bool wasDocked;
     internal bool redocked;
     internal bool minimizedFromCloseButton;
     internal Vector2 minimizedPos;
     internal bool wasPinned;
     internal bool firstUnpin=true;
     internal bool movedAfterMinimize=true;
        internal void OnMinimize(bool closeButton,Vector2 rawPosition){
         gameObject.SetActive(true);
         Vector2 windowPos=window.rectTransform.anchoredPosition;
         previousWindowPos=windowPos;
         Vector2 windowSize=window.GetSize();
         float windowWidth =windowSize.x;
         float windowHeight=windowSize.y;
         Vector2 btnPos=rectTransform.anchoredPosition;
         Vector2 btnSize=GetSize();
         float btnWidth =btnSize.x;
         float btnHeight=btnSize.y;
         Logs.Debug(()=>"windowPos:"+windowPos+";windowSize:"+windowSize+";btnSize:"+btnSize);
         minimizedFromCloseButton=closeButton;
         switch(window.dockingState){
          case DockingState.Pinned:{
           if(!wasPinned){
            SetMinimizedPos(MinimizedPosFrom.WindowPos);
           }else{
            SetMinimizedPos(MinimizedPosFrom.Unchanged);
           }
           wasPinned=true;
           break;
          }
          case DockingState.Free:{
           if(!window.movedAfterRestore){
            SetMinimizedPos(MinimizedPosFrom.Unchanged);
           }else if(justUnpinned){
            if(!firstUnpin){
             SetMinimizedPos(MinimizedPosFrom.Unchanged);
            }else{
             SetMinimizedPos(MinimizedPosFrom.WindowPos);
            }
            firstUnpin=false;
           }else{
            SetMinimizedPos(MinimizedPosFrom.WindowPos);
           }
           break;
          }
          case DockingState.Docked:{
           if(wasDocked){
            SetMinimizedPos(MinimizedPosFrom.Unchanged);
           }else if(redocked){
            SetMinimizedPos(MinimizedPosFrom.Unchanged);
           }else{
            SetMinimizedPos(MinimizedPosFrom.RawPosition);
           }
           break;
          }
          default:{
           SetMinimizedPos(MinimizedPosFrom.Unchanged);
           break;
          }
         }
         SetSafePos(minimizedPos);
         void SetMinimizedPos(MinimizedPosFrom mode){
          switch(mode){
           case MinimizedPosFrom.RawPosition:{
            minimizedPos=ScreenToCanvasPosition(rawPosition,root.canvas);
            break;
           }
           case MinimizedPosFrom.WindowPos:{
            minimizedPos=new(
             windowPos.x+windowWidth *0.5f-btnWidth *0.5f,
             windowPos.y+windowHeight*0.5f-btnHeight*0.5f
            );
            break;
           }
           default:{
            minimizedPos=rectTransform.anchoredPosition;
            break;
           }
          }
         }
        }
        enum MinimizedPosFrom{
         Unchanged,
         WindowPos,
         RawPosition,
        }
        internal void OnMinimized(){
         wasDocked=window.dockingState==DockingState.Docked;
         redocked=false;
         justUnpinned=false;
         BringToFront();
        }
        internal void OnRestore(){
        }
        internal void OnRestored(){
         switch(window.dockingState){
          case DockingState.Docked:{
           if(window.redocked){
            if(window.redockUpdatePos){
             previousWindowPos=window.restoredPos;
            }
           }
           break;
          }
         }
         movedAfterMinimize=false;
         gameObject.SetActive(false);
        }
        internal void OnDocked(){
         wasDocked=false;
        }
        internal void OnUndocked(){
         dockedFromButtonDrag=false;
        }
        internal void OnWindowPinned(){
        }
     internal bool justUnpinned;
        internal void OnWindowUnpinned(){
         wasPinned=false;
         justUnpinned=true;
        }
    }
}