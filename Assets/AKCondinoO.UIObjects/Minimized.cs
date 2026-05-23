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
     internal Vector2 previousWindowPos;
     internal bool minimizedFromCloseButton;
     internal bool draggedAfterCloseButton;
        public void OnPointerClick(PointerEventData eventData){
         if(wasDragged)
          return;
         UISystem.singleton.windowDockManager.Restore(this,window);
        }
        internal void OnMinimizedButtonEnabled(Vector2 previousWindowPos,bool minimizedFromCloseButton){
         this.previousWindowPos=previousWindowPos;
         this.minimizedFromCloseButton=minimizedFromCloseButton;
         draggedAfterCloseButton=false;
        }
    }
}