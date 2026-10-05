using AKCondinoO.Bootstrap;
using LibNoise.Operator;
using TMPro;
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
     Tab[]previousTabs;
     TabButton[]previousTabButtons;
        internal void Build(TabDefinition[]tabsInGroup){
         layoutElement=GetComponent<LayoutElement>();
         previousTabs=GetComponentsInChildren<Tab>(true);
         previousTabButtons=tabsLayout.tabsHeader.GetComponentsInChildren<TabButton>(true);
         //Logs.Debug(()=>"tabs:"+tabs);
         tabs=new Tab[tabsInGroup.Length];
         tabRects=new RectTransform[tabsInGroup.Length];
         tabButtons=new Button[tabsInGroup.Length];
         for(int i=0;i<tabsInGroup.Length;i++){
          int idx=tabsLayout.tabsGroup.tabsOrderInverted?(tabsInGroup.Length-1-i):i;
          var tab=FindTab(tabsInGroup[idx]);
          if(tab==null){
           tab=InstantiateTab(tabsInGroup[idx]);
          }
          tabs[idx]=tab;
          tabRects[idx]=tab.GetComponent<RectTransform>();
          int index=idx;
          var button=FindHeaderButton(tabsInGroup[idx]);
          if(button==null){
           var headerButtonPrefab=tabsInGroup[idx].headerButtonPrefab;
           var headerButtonGameObject=InstantiateHeaderButton(tabsInGroup[idx],headerButtonPrefab);
           button=headerButtonGameObject.GetComponent<Button>();
           var TMP_Text=button.GetComponentInChildren<TMP_Text>(true);
           if(TMP_Text!=null){
            TMP_Text.text=tabsInGroup[idx].title;
           }
           button.onClick.AddListener(()=>Show(index));
          }
          tabButtons[idx]=button;
         }
         hasMultipleTabs=tabsInGroup.Length>1;
         tabsLayout.tabsHeader.gameObject.SetActive(hasMultipleTabs);
        }
        Tab InstantiateTab(TabDefinition tabDef){
         Tab tab;
         var contentPrefab=tabDef.contentPrefab;
         #if UNITY_EDITOR
         if(!Application.isPlaying){
          tab=((GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(contentPrefab,transform)).GetComponent<Tab>();
         }else
         #endif
         {
          tab=Instantiate(contentPrefab,transform).GetComponent<Tab>();
         }
         tab.definitionId=tabDef.id;
         return tab;
        }
        GameObject InstantiateHeaderButton(TabDefinition tabDef,GameObject headerButtonPrefab){
         GameObject headerButton;
         #if UNITY_EDITOR
         if(!Application.isPlaying){
          headerButton=(GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(headerButtonPrefab,tabsLayout.tabsHeader);
         }else
         #endif
         {
          headerButton=Instantiate(headerButtonPrefab,tabsLayout.tabsHeader);
         }
         headerButton.GetComponent<TabButton>().definitionId=tabDef.id;
         return headerButton;
        }
        Tab FindTab(TabDefinition tabDef){
         foreach(var tab in previousTabs){
          if(tab.definitionId==tabDef.id){
           return tab;
          }
         }
         return null;
        }
        Button FindHeaderButton(TabDefinition tabDef){
         foreach(var button in previousTabButtons){
          if(button.definitionId==tabDef.id){
           return button.GetComponent<Button>();
          }
         }
         return null;
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