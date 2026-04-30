using System;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class TabsGroup:MonoBehaviour{
     internal Window window;
     [SerializeField]internal RectTransform tabsHeader;
     internal LayoutElement tabsHeaderLayoutElement;
     [SerializeField]private GameObject headerButtonPrefab;
     [SerializeField]internal TabDefinition[]tabsInGroup;
     internal TabsContainer container;
        internal void OnAwake(){
         tabsHeaderLayoutElement=tabsHeader.GetComponent<LayoutElement>();
         container=GetComponentInChildren<TabsContainer>();
         container.tabsGroup=this;
         container.Build(tabsInGroup,headerButtonPrefab);
         container.Show(0);
        }
        internal void OnStart(){
        }
    }
    [Serializable]
    internal class TabDefinition{
     public string title;
     public GameObject contentPrefab;
    }
}