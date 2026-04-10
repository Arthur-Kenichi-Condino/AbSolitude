using AKCondinoO.Bootstrap;
using UnityEngine;
using UnityEngine.EventSystems;
namespace AKCondinoO.UIObjects{
    internal class Header:MonoBehaviour,IPointerDownHandler,IDragHandler{
     internal Window window;
        public void OnPointerDown(PointerEventData eventData){
         window.root.transform.SetAsLastSibling();
        }
        public void OnDrag(PointerEventData eventData){
         ((RectTransform)window.transform).anchoredPosition+=eventData.delta;
         //Logs.Debug(()=>"eventData.delta");
        }
    }
}