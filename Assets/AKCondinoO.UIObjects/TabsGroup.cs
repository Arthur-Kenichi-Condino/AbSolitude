using AKCondinoO.Bootstrap;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class TabsGroup:MonoBehaviour{
     internal Window window;
     [SerializeField]internal TabsOrientation tabsOrientation;
     [SerializeField]internal bool tabsOrderInverted;
     [SerializeField]internal TabDefinition[]tabsInGroup;
     [SerializeField]HorizontalTabs horizontalTabsPrefab;
     [SerializeField]VerticalTabs verticalTabsPrefab;
     internal TabsLayout tabsLayout;
        internal enum TabsOrientation{
         Horizontal=0,
         Vertical=1,
        }
        internal void OnAwake(Window window){
         this.window=window;
         bool hadTabsLayout=false;
         var anyTabsLayout=GetComponentsInChildren<TabsLayout>(true);
         if(anyTabsLayout.Length>0){
          tabsLayout=anyTabsLayout[0];
          hadTabsLayout=true;
          Logs.Debug(()=>"'tabsLayout already found in children (maybe it was instantiated on Editor)':"+tabsLayout.GetType().Name);
          for(int i=1;i<anyTabsLayout.Length;++i){
           DestroyImmediate(anyTabsLayout[i].gameObject);
           Logs.Warning("'destroyed duplicated tabsLayout':"+tabsLayout.GetType().Name);
          }
         }else{
          switch(tabsOrientation){
           case TabsOrientation.Vertical:{
            tabsLayout=InstantiateTabsLayout(verticalTabsPrefab);
            break;
           }
           default:{
            tabsLayout=InstantiateTabsLayout(horizontalTabsPrefab);
            break;
           }
          }
         }
         tabsLayout.tabsGroup=this;
         tabsLayout.OnAwake(hadTabsLayout);
        }
        TabsLayout InstantiateTabsLayout(TabsLayout tabsLayoutPrefab){
         #if UNITY_EDITOR
         if(!Application.isPlaying){
          return(TabsLayout)UnityEditor.PrefabUtility.InstantiatePrefab(tabsLayoutPrefab,transform);
         }else
         #endif
         {
          return Instantiate(tabsLayoutPrefab,transform);
         }
        }
    }
}