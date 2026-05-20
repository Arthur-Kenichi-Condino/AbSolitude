using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class Minimized:MonoBehaviour,IUIWindowElement,
     IPointerDownHandler,
     IBeginDragHandler,
     IDragHandler,
     IEndDragHandler,
     IPointerClickHandler
    {
     internal UIWindowRoot root;
     private Button btn;
        internal void OnAwake(UIWindowRoot root){
         this.root=root;
         btn=GetComponent<Button>();
        }
     internal Window window;
        internal void RegisterWindow(Window window){
         this.window=window;
        }
     bool wasDragged;
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
         minimizedRect.anchoredPosition=WindowDockManager.ClampInsideCanvas(minimizedRect.anchoredPosition,gameObject,root.canvas);
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
        public void BringToFront(){
         root.transform.SetAsLastSibling();
        }
    }
}