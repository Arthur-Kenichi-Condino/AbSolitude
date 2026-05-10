using LibNoise.Operator;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class TabsContainer:MonoBehaviour{
     internal TabsLayout tabsLayout;
     private LayoutElement layoutElement;
     internal Tab[]tabs;
     private RectTransform[]tabRects;
     private Button[]tabButtons;
     internal bool hasMultipleTabs;
        internal void Build(TabDefinition[]tabsInGroup){
         layoutElement=GetComponent<LayoutElement>();
         tabs=new Tab[tabsInGroup.Length];
         tabRects=new RectTransform[tabsInGroup.Length];
         tabButtons=new Button[tabsInGroup.Length];
         for(int i=0;i<tabsInGroup.Length;i++){
          int idx=tabsLayout.tabsGroup.tabsOrderInverted?(tabsInGroup.Length-1-i):i;
          var headerButtonPrefab=tabsInGroup[idx].headerButtonPrefab;
          var tab=Instantiate(tabsInGroup[idx].contentPrefab,transform);
          tabs[idx]=tab.GetComponent<Tab>();
          tabRects[idx]=tab.GetComponent<RectTransform>();
          var headerButtonGameObject=Instantiate(headerButtonPrefab,tabsLayout.tabsHeader);
          var button=headerButtonGameObject.GetComponent<Button>();
          int index=idx;
          button.onClick.AddListener(()=>Show(index));
          tabButtons[idx]=button;
         }
         hasMultipleTabs=tabsInGroup.Length>1;
         tabsLayout.tabsHeader.gameObject.SetActive(hasMultipleTabs);
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
         if(tabsLayout.tabsGroup.window!=null){
          tabsLayout.tabsGroup.window.OnContentChanged((RectTransform)currentTab.transform);
         }
        }
    }
}