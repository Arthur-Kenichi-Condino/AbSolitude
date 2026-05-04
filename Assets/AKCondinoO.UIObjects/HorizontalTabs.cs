using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class HorizontalTabs:TabsLayout{
     internal HorizontalLayoutGroup tabsHeaderHorizontalLayoutGroup;
        internal override void OnAwake(){
         base.OnAwake();
         tabsHeaderHorizontalLayoutGroup=tabsHeader.GetComponent<HorizontalLayoutGroup>();
         container.Build(tabsInGroup,headerButtonPrefab);
         container.Show(0);
        }
    }
}