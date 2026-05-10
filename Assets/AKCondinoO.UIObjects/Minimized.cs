using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class Minimized:MonoBehaviour,
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
        }
        public void OnDrag(PointerEventData eventData){
         ((RectTransform)transform).anchoredPosition+=(eventData.delta/root.canvas.scaleFactor);
        }
        public void OnEndDrag(PointerEventData eventData){
        }
        public void OnPointerClick(PointerEventData eventData){
         if(wasDragged)
          return;
         UISystem.singleton.windowDockManager.Restore(this,window);
        }
    }
}