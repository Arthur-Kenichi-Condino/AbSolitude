using AKCondinoO.Bootstrap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class Header:MonoBehaviour,IPointerDownHandler,IDragHandler{
     internal Window window;
     internal LayoutElement layoutElement;
        internal void OnAwake(){
         layoutElement=GetComponent<LayoutElement>();
        }
        public void OnPointerDown(PointerEventData eventData){
         window.root.transform.SetAsLastSibling();
        }
        public void OnDrag(PointerEventData eventData){
         ((RectTransform)window.transform).anchoredPosition+=eventData.delta;
         //Logs.Debug(()=>"eventData.delta");
        }
    }
}