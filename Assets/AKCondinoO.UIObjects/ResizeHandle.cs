using UnityEngine;
using UnityEngine.EventSystems;
namespace AKCondinoO.UIObjects{
    internal class ResizeHandle:MonoBehaviour,IDragHandler{
     [SerializeField]internal RectTransform resizable;
     [SerializeField]internal Vector2 minSize=new(300,200);
        public void OnDrag(PointerEventData eventData){
         Vector2 size=resizable.sizeDelta;
         size+=new Vector2(eventData.delta.x,-eventData.delta.y);
         size.x=Mathf.Max(size.x,minSize.x);
         size.y=Mathf.Max(size.y,minSize.y);
         resizable.sizeDelta=size;
        }
    }
}