using AKCondinoO.Bootstrap;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static AKCondinoO.UIObjects.UISystem;
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
         if(minimizedFromCloseButton){
          draggedAfterCloseButton=true;
         }
        }
        public void OnDrag(PointerEventData eventData){
         ((RectTransform)transform).anchoredPosition+=(eventData.delta/root.canvas.scaleFactor);
        }
        public void OnEndDrag(PointerEventData eventData){
         var minimizedRect=((RectTransform)transform);
         SetSafePos(minimizedRect.anchoredPosition);
         wasDragged=false;
        }
        public void OnPointerClick(PointerEventData eventData){
         if(wasDragged)
          return;
         UISystem.singleton.windowDockManager.Restore(this,window);
        }
     internal Vector2 previousWindowPos;
     internal bool minimizedFromCloseButton;
     internal bool draggedAfterCloseButton;
     internal Vector2 minimizedPos;
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
         minimizedPos=rectTransform.anchoredPosition;
         if(!closeButton){
          draggedAfterCloseButton=false;
         }
         if(window.docked){
          return;
         }
         if(draggedAfterCloseButton&&!window.dragged){
          return;
         }
         if(closeButton){
          minimizedPos=new(
           windowPos.x+windowWidth *0.5f-btnWidth *0.5f,
           windowPos.y+windowHeight*0.5f-btnHeight*0.5f
          );
          return;
         }
         minimizedPos=ScreenToCanvasPosition(rawPosition,root.canvas);
        }
        internal void OnMinimized(){
         SetSafePos(minimizedPos);
         BringToFront();
        }
        internal void OnRestore(){
        }
        internal void OnRestored(){
         gameObject.SetActive(false);
        }
    }
}