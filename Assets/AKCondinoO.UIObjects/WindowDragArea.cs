using System.Runtime.Remoting.Messaging;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
namespace AKCondinoO.UIObjects{
    internal class WindowDragArea:MonoBehaviour,IPointerDownHandler,IDragHandler{
     private Window window;
     internal int siblingIndex;
     internal LayoutElement layoutElement;
        internal void OnAwake(Window window){
         this.window=window;
         siblingIndex=transform.GetSiblingIndex();
         layoutElement=GetComponent<LayoutElement>();
        }
        public void OnPointerDown(PointerEventData eventData){
         window.root.transform.SetAsLastSibling();
        }
        public void OnDrag(PointerEventData eventData){
         ((RectTransform)window.transform).anchoredPosition+=eventData.delta;
         //Logs.Debug(()=>"eventData.delta");
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