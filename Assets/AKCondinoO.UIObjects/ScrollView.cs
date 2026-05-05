using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal class ScrollView:MonoBehaviour{
     private Window window;
     [SerializeField]internal RectTransform scrollbarHorizontal;
     [SerializeField]internal RectTransform scrollbarVertical;
        internal void OnAwake(Window window){
         this.window=window;
        }
    }
}