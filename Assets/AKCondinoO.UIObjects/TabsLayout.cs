using System;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal abstract class TabsLayout:MonoBehaviour{
     internal TabsGroup tabsGroup;
     [SerializeField]internal RectTransform tabsHeader;
     internal LayoutElement tabsHeaderLayoutElement;
     internal TabsContainer container;
        internal virtual void OnAwake(){
         tabsHeaderLayoutElement=tabsHeader.GetComponent<LayoutElement>();
         container=GetComponentInChildren<TabsContainer>();
         container.tabsLayout=this;
        }
    }
    [Serializable]
    internal class TabDefinition{
     public string title;
     public GameObject contentPrefab;
    }
}