using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class VerticalTabs:TabsLayout{
     internal VerticalLayoutGroup tabsHeaderVerticalLayoutGroup;
        internal override void OnAwake(){
         base.OnAwake();
         tabsHeaderVerticalLayoutGroup=tabsHeader.GetComponent<VerticalLayoutGroup>();
         container.Build(tabsGroup.tabsInGroup,tabsGroup.headerButtonPrefab);
         container.Show(0);
        }
    }
}