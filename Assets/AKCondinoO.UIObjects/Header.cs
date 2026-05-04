using AKCondinoO.Bootstrap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class Header:MonoBehaviour{
     internal Window window;
     internal LayoutElement layoutElement;
     internal bool hidden=false;
        internal void OnAwake(){
         layoutElement=GetComponent<LayoutElement>();
        }
    }
}