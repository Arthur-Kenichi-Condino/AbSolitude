using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class HorizontalTabs:TabsLayout{
     internal HorizontalLayoutGroup tabsHeaderHorizontalLayoutGroup;
        internal override void OnAwake(bool wasAwake=false){
         base.OnAwake(wasAwake);
        }
        internal override void SetLayout(bool wasAwake=false){
         base.SetLayout(wasAwake);
         tabsHeaderHorizontalLayoutGroup=tabsHeader.GetComponent<HorizontalLayoutGroup>();
        }
    }
}