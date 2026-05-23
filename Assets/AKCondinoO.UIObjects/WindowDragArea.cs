using System.Runtime.Remoting.Messaging;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static AKCondinoO.UIObjects.UISystem;
namespace AKCondinoO.UIObjects{
    internal class WindowDragArea:MonoBehaviour,
     IPointerDownHandler,
     IBeginDragHandler,
     IDragHandler,
     IEndDragHandler
    {
     private Window window;
     internal int siblingIndex;
     internal LayoutElement layoutElement;
        internal void OnAwake(Window window){
         this.window=window;
         siblingIndex=transform.GetSiblingIndex();
         layoutElement=GetComponent<LayoutElement>();
        }
     internal bool wasDragged{get;private set;}
        public void OnPointerDown(PointerEventData eventData){
         wasDragged=false;
         window.BringToFront();
        }
        public void OnBeginDrag(PointerEventData eventData){
         wasDragged=true;
        }
        public void OnDrag(PointerEventData eventData){
         ((RectTransform)window.transform).anchoredPosition+=(eventData.delta/window.root.canvas.scaleFactor);
         //Logs.Debug(()=>"eventData.delta");
        }
        public void OnEndDrag(PointerEventData eventData){
         var mousePosition=eventData.position;
         if(IsNearScreenEdgeScreenSpace(mousePosition,window.root.canvas,64f)){
          UISystem.singleton.windowDockManager.Minimize(window.minimizedBtn,window,false,mousePosition);
         }else{
          var windowRect=((RectTransform)window.transform);
          window.SetSafePos(windowRect.anchoredPosition);
         }
         wasDragged=false;
        }
        internal void OnSetHeaderVisible(bool visible){
         if(visible){
          int targetIndex=Mathf.Max(0,siblingIndex);
          transform.SetSiblingIndex(targetIndex);
          layoutElement.ignoreLayout=true;
          float h=window.header.layoutElement.minHeight;
          h+=window.verticalLayoutDefaultPadding.top;
          var rectTransform=(RectTransform)transform;
          rectTransform.anchorMin=new(0f,1f);
          rectTransform.anchorMax=new(1f,1f);
          rectTransform.pivot=new(0.5f,1f);
          rectTransform.sizeDelta=new(0,h);
          rectTransform.anchoredPosition=new(0f,0f);
         }else{
          transform.SetAsFirstSibling();
          layoutElement.ignoreLayout=false;
          float h=window.verticalLayoutDefaultPadding.top;
          layoutElement.minHeight=h;
          layoutElement.preferredHeight=h;
          layoutElement.flexibleHeight=0;
         }
        }
    }
}