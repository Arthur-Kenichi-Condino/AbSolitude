using System;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal abstract class TabsLayout:MonoBehaviour{
     internal TabsGroup tabsGroup;
     [SerializeField]internal RectTransform tabsHeader;
     internal LayoutElement tabsHeaderLayoutElement;
     internal TabsContainer container;
        internal virtual void OnAwake(bool wasAwake=false){
         tabsHeaderLayoutElement=tabsHeader.GetComponent<LayoutElement>();
         container=GetComponentInChildren<TabsContainer>(true);
         container.tabsLayout=this;
         if(!wasAwake){
          var testTabButtons=GetComponentsInChildren<TabButton>(true);
          foreach(var button in testTabButtons){
           DestroyImmediate(button.gameObject);
          }
         }
         SetLayout(wasAwake);
         container.Build(tabsGroup.tabsInGroup);
         container.Show(0);
        }
        internal virtual void SetLayout(bool wasAwake=false){
        }
    }
    [Serializable]
    internal class TabDefinition{
     public string title;
     public GameObject headerButtonPrefab;
     public GameObject contentPrefab;
     public string id;
    }
}