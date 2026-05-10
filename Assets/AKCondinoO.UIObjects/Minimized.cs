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
         btn.onClick.AddListener(()=>UISystem.singleton.windowDockManager.Restore(this,window));
        }
     bool dragging;
        public void OnPointerDown(PointerEventData eventData){
        }
        public void OnBeginDrag(PointerEventData eventData){
        }
        public void OnDrag(PointerEventData eventData){
        }
        public void OnEndDrag(PointerEventData eventData){
        }
        public void OnPointerClick(PointerEventData eventData){
        }
    }
}