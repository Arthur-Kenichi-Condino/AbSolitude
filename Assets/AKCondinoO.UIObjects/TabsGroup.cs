using System;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class TabsGroup:MonoBehaviour{
     internal Window window;
     [SerializeField]internal TabsOrientation tabsOrientation;
     [SerializeField]internal bool tabsOrderInverted;
     [SerializeField]HorizontalTabs horizontalTabsPrefab;
     [SerializeField]VerticalTabs verticalTabsPrefab;
     internal TabsLayout tabsLayout;
        internal enum TabsOrientation{
         Horizontal=0,
         Vertical=1,
        }
        internal void OnAwake(){
         switch(tabsOrientation){
          case TabsOrientation.Vertical:{
           tabsLayout=Instantiate(verticalTabsPrefab,transform);
           break;
          }
          default:{
           tabsLayout=Instantiate(horizontalTabsPrefab,transform);
           break;
          }
         }
         tabsLayout.tabsGroup=this;
         tabsLayout.OnAwake();
        }
        internal void OnStart(){
        }
    }
}