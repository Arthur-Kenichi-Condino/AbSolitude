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
         buttonMovedAfterMinimize=true;
        }
        public void OnDrag(PointerEventData eventData){
         ((RectTransform)transform).anchoredPosition+=(eventData.delta/root.canvas.scaleFactor);
        }
        public void OnEndDrag(PointerEventData eventData){
         var minimizedRect=((RectTransform)transform);
         SetSafePos(minimizedRect.anchoredPosition);
         wasDragged=false;
         if(IsNearCanvasEdgeLocalSpace(minimizedRect.anchoredPosition,root.canvas,out bool left,out bool right,out bool bottom,out bool top,new(64f,64f))){
          dockedByButtonDrag=true;
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
     internal bool dockedByButtonDrag;
     internal Vector2 previousWindowPos;
     internal bool minimizedFromCloseButton;
     internal bool wasDocked;
     internal bool redocked;
     internal bool wasPinnedBeforeMinimize;
     internal bool justUnpinned;
     internal bool firstUnpin=true;
     internal bool buttonMovedAfterMinimize=true;
     internal Vector2 minimizedPos;
        internal void OnMinimize(bool closeButton,Vector2 rawPosition){
         gameObject.SetActive(true);
         Vector2 windowPos=window.rectTransform.anchoredPosition;
         previousWindowPos=windowPos;
         minimizedFromCloseButton=closeButton;
         minimizedPos=GetMinimizedPosition(rawPosition);
         SetSafePos(minimizedPos);
        }
        Vector2 GetMinimizedPosition(Vector2 rawPosition){
         switch(window.dockingState){
          case DockingState.Pinned:{
           return!wasPinnedBeforeMinimize?MinimizedPosition(rawPosition,MinimizedPosFrom.WindowPos):MinimizedPosition(rawPosition,MinimizedPosFrom.Unchanged);
          }
          case DockingState.Free:{
           if(!window.windowMovedAfterRestore)return MinimizedPosition(rawPosition,MinimizedPosFrom.Unchanged);
           if(justUnpinned)return!firstUnpin?MinimizedPosition(rawPosition,MinimizedPosFrom.Unchanged):MinimizedPosition(rawPosition,MinimizedPosFrom.WindowPos);
           return MinimizedPosition(rawPosition,MinimizedPosFrom.WindowPos);
          }
          case DockingState.Docked:{
           if(wasDocked)return MinimizedPosition(rawPosition,MinimizedPosFrom.Unchanged);
           if(redocked)return MinimizedPosition(rawPosition,MinimizedPosFrom.Unchanged);
           return MinimizedPosition(rawPosition,MinimizedPosFrom.RawPosition);
          }
          default:{
           return MinimizedPosition(rawPosition,MinimizedPosFrom.Unchanged);
          }
         }
        }
        Vector2 MinimizedPosition(Vector2 rawPosition,MinimizedPosFrom mode){
         Vector2 windowPos=window.rectTransform.anchoredPosition;
         Vector2 windowSize=window.GetSize();
         float windowWidth =windowSize.x;
         float windowHeight=windowSize.y;
         Vector2 btnPos=rectTransform.anchoredPosition;
         Vector2 btnSize=GetSize();
         float btnWidth =btnSize.x;
         float btnHeight=btnSize.y;
         Logs.Debug(()=>"windowPos:"+windowPos+";windowSize:"+windowSize+";btnSize:"+btnSize);
         switch(mode){
          case MinimizedPosFrom.RawPosition:{
           return ScreenToCanvasPosition(rawPosition,root.canvas);
          }
          case MinimizedPosFrom.WindowPos:{
           return new(
            windowPos.x+windowWidth *0.5f-btnWidth *0.5f,
            windowPos.y+windowHeight*0.5f-btnHeight*0.5f
           );
          }
          default:{
           return rectTransform.anchoredPosition;
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
         wasPinnedBeforeMinimize=window.dockingState==DockingState.Pinned;
         justUnpinned=false;
         firstUnpin=false;
         BringToFront();
        }
        internal void OnRestore(){
        }
        internal void OnRestored(){
         switch(window.dockingState){
          case DockingState.Docked:{
           if(window.restoredAfterRedock){
            if(window.shouldUpdatePreviousWindowPosOnRestore){
             previousWindowPos=window.restoredPos;
            }
           }
           break;
          }
         }
         buttonMovedAfterMinimize=false;
         gameObject.SetActive(false);
        }
        internal void OnDocked(){
         wasDocked=false;
        }
        internal void OnUndocked(){
         dockedByButtonDrag=false;
        }
        internal void OnWindowPinned(){
        }
        internal void OnWindowUnpinned(){
         wasPinnedBeforeMinimize=false;
         justUnpinned=true;
        }
    }
}