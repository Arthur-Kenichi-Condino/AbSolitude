using System;
using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal class TabsGroup:MonoBehaviour{
     internal Window window;
     [SerializeField]internal RectTransform tabsHeader;
     [SerializeField]private GameObject headerButtonPrefab;
     [SerializeField]internal TabDefinition[]tabsInGroup;
     internal TabsContainer container;
        internal void OnAwake(){
         container=GetComponentInChildren<TabsContainer>();
         container.tabsGroup=this;
         container.Build(tabsInGroup,headerButtonPrefab);
         container.Show(0);
        }
    }
    [Serializable]
    internal class TabDefinition{
     public string title;
     public GameObject contentPrefab;
    }
}