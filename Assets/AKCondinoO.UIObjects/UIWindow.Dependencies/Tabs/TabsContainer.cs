using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class TabsContainer:MonoBehaviour{
     internal TabsGroup tabsGroup;
     private LayoutElement layoutElement;
     internal Tab[]tabs;
     private RectTransform[]tabRects;
     private Button[]tabButtons;
        internal void Build(TabDefinition[]tabsInGroup,GameObject headerButtonPrefab){
         layoutElement=GetComponent<LayoutElement>();
         tabs=new Tab[tabsInGroup.Length];
         tabRects=new RectTransform[tabsInGroup.Length];
         tabButtons=new Button[tabsInGroup.Length];
         for(int i=0;i<tabsInGroup.Length;i++){
          var tab=Instantiate(tabsInGroup[i].contentPrefab,transform);
          tabs[i]=tab.GetComponent<Tab>();
          tabRects[i]=tab.GetComponent<RectTransform>();
          var headerButtonGameObject=Instantiate(headerButtonPrefab,tabsGroup.tabsHeader);
          var button=headerButtonGameObject.GetComponent<Button>();
          int index=i;
          button.onClick.AddListener(()=>Show(index));
          tabButtons[i]=button;
         }
        }
     private int currentIndex;
     private Tab currentTab;
        internal void Show(int index){
         currentIndex=index;
         for(int i=0;i<tabs.Length;i++){
          var tab=tabs[i];
          if(i==index){
           currentTab=tab;
           var tabRect=tabRects[i];
           layoutElement.minWidth =tabRect.rect.width ;
           layoutElement.minHeight=tabRect.rect.height;
           tabRect.anchoredPosition=new(tabRect.rect.width/2f,-tabRect.rect.height/2f);
          }
          tab.gameObject.SetActive(i==index);
          tabButtons[i].interactable=(i!=index);
         }
         if(tabsGroup.window!=null){
          tabsGroup.window.OnContentChanged((RectTransform)currentTab.transform);
         }
        }
    }
}