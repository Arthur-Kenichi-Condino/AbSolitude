using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class VerticalTabs:TabsLayout{
     internal VerticalLayoutGroup tabsHeaderVerticalLayoutGroup;
        internal override void OnAwake(bool wasAwake=false){
         base.OnAwake(wasAwake);
        }
        internal override void SetLayout(bool wasAwake=false){
         base.SetLayout(wasAwake);
         tabsHeaderVerticalLayoutGroup=tabsHeader.GetComponent<VerticalLayoutGroup>();
        }
    }
}