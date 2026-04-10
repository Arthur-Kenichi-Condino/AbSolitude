using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class Window:MonoBehaviour{
     internal UIWindow root;
     internal VerticalLayoutGroup verticalLayoutGroup;
     internal Header header;
     internal ScrollView scrollView;
     internal TabsGroup tabsGroup;
        internal void OnAwake(){
         verticalLayoutGroup=GetComponent<VerticalLayoutGroup>();
         header=GetComponentInChildren<Header>();
         header.window=this;
         scrollView=GetComponentInChildren<ScrollView>();
         scrollView.window=this;
         tabsGroup=GetComponentInChildren<TabsGroup>();
         if(tabsGroup!=null){
          tabsGroup.window=this;
          tabsGroup.OnAwake();
         }
        }
        internal void OnContentChanged(RectTransform rectTransform){
         float headerHeight=((RectTransform)header.transform).rect.height;
         float scrollbarHorizontalHeight=scrollView.scrollbarHorizontal.rect.height;
         float scrollbarVerticalWidth   =scrollView.scrollbarVertical  .rect.width ;
         float tabsHeaderHeight=tabsGroup.tabsHeader.rect.height;
         Vector2 size=rectTransform.rect.size;
         size.x+=verticalLayoutGroup.padding.left  +verticalLayoutGroup.padding.right;
         size.y+=verticalLayoutGroup.padding.bottom+verticalLayoutGroup.padding.top  +verticalLayoutGroup.spacing+headerHeight+tabsHeaderHeight;
         var windowRectTransform=(RectTransform)transform;
         windowRectTransform.sizeDelta=size;
        }
    }
}