using AKCondinoO.Bootstrap;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class Window:MonoBehaviour,IUIWindowElement{
     [SerializeField]internal bool autoResize=true;
     [SerializeField]internal bool hideHeader=false;
     internal UIWindowRoot root;
     internal VerticalLayoutGroup verticalLayoutGroup;
     internal Header header;
     internal ScrollView scrollView;
     internal TabsGroup tabsGroup;
     internal RectOffset verticalLayoutDefaultPadding;
     internal WindowDragArea dragArea;
     internal CloseButton closeButton;
        internal void OnAwake(UIWindowRoot root){
         this.root=root;
         verticalLayoutGroup=GetComponent<VerticalLayoutGroup>();
         verticalLayoutDefaultPadding=new RectOffset(
          verticalLayoutGroup.padding.left,
          verticalLayoutGroup.padding.right,
          verticalLayoutGroup.padding.top,
          verticalLayoutGroup.padding.bottom
         );
         header=GetComponentInChildren<Header>();
         header.OnAwake(this);
         scrollView=GetComponentInChildren<ScrollView>();
         scrollView.OnAwake(this);
         tabsGroup=GetComponentInChildren<TabsGroup>();
         if(tabsGroup!=null){
          tabsGroup.OnAwake(this);
         }
         dragArea=GetComponentInChildren<WindowDragArea>();
         dragArea.OnAwake(this);
         closeButton=GetComponentInChildren<CloseButton>();
         closeButton.OnAwake(this);
         SetHeaderVisible(!hideHeader);
        }
     internal Minimized minimizedBtn;
        internal void RegisterMinimizedBtn(Minimized minimizedBtn){
         this.minimizedBtn=minimizedBtn;
        }
        internal void OnManualUpdate(){
         SetHeaderVisible(!hideHeader);
        }
        internal void SetHeaderVisible(bool visible){
         if(visible){
          if(header.hidden){
           header.hidden=false;
           header.gameObject.SetActive(true);
           verticalLayoutGroup.padding.top=verticalLayoutDefaultPadding.top;
           dragArea.OnSetHeaderVisible(visible);
           UpdateSize();
          }
         }else{
          if(!header.hidden){
           header.hidden=true;
           header.gameObject.SetActive(false);
           verticalLayoutGroup.padding.top=0;
           dragArea.OnSetHeaderVisible(visible);
           UpdateSize();
          }
         }
        }
     internal Vector2 contentSize;
        internal void UpdateSize(){
         float headerHeight=header.hidden?dragArea.layoutElement.minHeight:header.layoutElement.minHeight;
         float scrollbarHorizontalHeight=scrollView.scrollbarHorizontal.rect.height;
         float scrollbarVerticalWidth   =scrollView.scrollbarVertical  .rect.width ;
         float tabsHeaderHeight=0f;
         float tabsHeaderWidth=0f;
         if(tabsGroup!=null){
          if(tabsGroup.tabsLayout.container.hasMultipleTabs){
           switch(tabsGroup.tabsOrientation){
            case TabsGroup.TabsOrientation.Horizontal:{
             tabsHeaderHeight=tabsGroup.tabsLayout.tabsHeaderLayoutElement.minHeight;
             break;
            }
            case TabsGroup.TabsOrientation.Vertical:{
             tabsHeaderWidth=tabsGroup.tabsLayout.tabsHeaderLayoutElement.minWidth;
             break;
            }
           }
          }
         }
         Logs.Debug(()=>"headerHeight:"+headerHeight+";scrollbarHorizontalHeight:"+scrollbarHorizontalHeight+";scrollbarVerticalWidth:"+scrollbarVerticalWidth+";tabsHeaderHeight:"+tabsHeaderHeight+";tabsHeaderWidth:"+tabsHeaderWidth);
         Vector2 size=contentSize;
         size.x+=verticalLayoutGroup.padding.left  +verticalLayoutGroup.padding.right+tabsHeaderWidth;
         size.y+=verticalLayoutGroup.padding.bottom+verticalLayoutGroup.padding.top  +verticalLayoutGroup.spacing+headerHeight+tabsHeaderHeight;
         var windowRectTransform=(RectTransform)transform;
         windowRectTransform.sizeDelta=size;
        }
        public void BringToFront(){
         root.transform.SetAsLastSibling();
        }
        internal void OnContentChanged(RectTransform rectTransform){
         contentSize=rectTransform.rect.size;
         if(autoResize){
          UpdateSize();
         }
        }
    }
}