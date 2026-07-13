using AKCondinoO.Bootstrap;
using System;
using UnityEngine;
using UnityEngine.UI;
using static AKCondinoO.UIObjects.UISystem;
namespace AKCondinoO.UIObjects{
    internal class Window:UIObjectModule{
     [SerializeField]internal bool autoResize=true;
     [SerializeField]internal bool hideHeader=false;
     internal VerticalLayoutGroup verticalLayoutGroup;
     internal Header header;
     internal ScrollView scrollView;
     internal TabsGroup tabsGroup;
     internal RectOffset verticalLayoutDefaultPadding;
     internal WindowDragArea dragArea;
     internal CloseButton closeButton;
     internal PinToggle pinToggle;
        public override void OnAwake(UIObject root){
         base.OnAwake(root);
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
         pinToggle=GetComponentInChildren<PinToggle>();
         pinToggle.OnAwake(this);
         SetHeaderVisible(!hideHeader);
        }
     internal Minimized minimizedBtn;
        internal void RegisterMinimizedBtn(Minimized minimizedBtn){
         this.minimizedBtn=minimizedBtn;
        }
     protected override bool shouldAutoKeepSafe{
      get{
       if(!dragArea.wasDragged){
        return true;
       }
       return false;
      }
     }
        public override void OnManualUpdate(){
         base.OnManualUpdate();
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
        internal void OnContentChanged(RectTransform contentRectTransform){
         contentSize=contentRectTransform.rect.size;
         if(autoResize){
          UpdateSize();
         }
        }
     internal DockingState dockingState=DockingState.Free;
        internal enum DockingState{
         Free,
         Docked,
         Pinned,
        };
     internal bool minimizedFromCloseButton;
        internal void OnMinimize(bool closeButton){
         minimizedFromCloseButton=closeButton;
         if(!closeButton){
          OnDocking();
         }
        }
     internal bool shouldUpdatePreviousWindowPosOnRestore;
        internal void OnMinimized(){
         shouldUpdatePreviousWindowPosOnRestore=minimizedBtn.redocked;
         windowMovedAfterRestore=false;
         gameObject.SetActive(false);
        }
     internal bool windowMovedAfterRestore=true;
     internal bool wasDockedBeforeMinimize;
     internal bool justUndockedFromDock;
     internal bool restoredAfterRedock;
     internal Vector2 restoredPos;
        internal void OnRestore(){
         gameObject.SetActive(true);
         restoredPos=GetRestoredPosition();
         SetSafePos(restoredPos);
        }
        Vector2 GetRestoredPosition(){
         switch(dockingState){
          case DockingState.Pinned:{
           return pinnedPos;
          }
          case DockingState.Free:{
           return minimizedBtn.buttonMovedAfterMinimize?RestoredPosition(RestoredPosFrom.MinimizedBtn):RestoredPosition(RestoredPosFrom.Unchanged);
          }
          case DockingState.Docked:{
           if(minimizedBtn.dockedByButtonDrag)return RestoredPosition(RestoredPosFrom.MinimizedBtn);
           if(restoredAfterRedock&&shouldUpdatePreviousWindowPosOnRestore)return RestoredPosition(RestoredPosFrom.MinimizedBtn);
           return RestoredPosition(RestoredPosFrom.MinimizedBtnPreviousWindowPos);
          }
          default:{
           return RestoredPosition(RestoredPosFrom.Unchanged);
          }
         }
        }
        Vector2 RestoredPosition(RestoredPosFrom mode){
         Vector2 windowPos=rectTransform.anchoredPosition;
         Vector2 windowSize=GetSize();
         float windowWidth =windowSize.x;
         float windowHeight=windowSize.y;
         Vector2 btnPos=minimizedBtn.rectTransform.anchoredPosition;
         Vector2 btnSize=minimizedBtn.GetSize();
         float btnWidth =btnSize.x;
         float btnHeight=btnSize.y;
         Logs.Debug(()=>"btnPos:"+btnPos+";btnSize:"+btnSize+";windowSize:"+windowSize);
         switch(mode){
          case RestoredPosFrom.MinimizedBtnPreviousWindowPos:{
           return minimizedBtn.previousWindowPos;
          }
          case RestoredPosFrom.MinimizedBtn:{
           return new(
            btnPos.x+btnWidth *0.5f-windowWidth *0.5f,
            btnPos.y+btnHeight*0.5f-windowHeight*0.5f
           );
          }
          default:{
           return rectTransform.anchoredPosition;
          }
         }
        }
        enum RestoredPosFrom{
         Unchanged,
         MinimizedBtn,
         MinimizedBtnPreviousWindowPos,
        }
        internal void OnRestored(){
         justUndockedFromDock=false;
         restoredAfterRedock=false;
         shouldUpdatePreviousWindowPosOnRestore=false;
         BringToFront();
        }
        internal void OnDocking(){
         if(OnChangeDockingState(DockingState.Docked)){
          minimizedBtn.OnDocked();
          if(justUndockedFromDock&&wasDockedBeforeMinimize){
           restoredAfterRedock=true;
          }
         }
        }
        internal void OnUndocking(){
         wasDockedBeforeMinimize=dockingState==DockingState.Docked;
         if(OnChangeDockingState(DockingState.Free)){
          minimizedBtn.OnUndocked();
          justUndockedFromDock=true;
         }
        }
     internal bool pinOn;
     internal Vector2 pinnedPos;
        internal void OnPin(){
         pinOn=true;
         pinnedPos=rectTransform.anchoredPosition;
         if(OnChangeDockingState(DockingState.Pinned)){
          minimizedBtn.OnWindowPinned();
         }
         Logs.Debug(()=>"'OnPin':"+dockingState);
        }
        internal void OnUnpin(){
         pinOn=false;
         if(OnChangeDockingState(DockingState.Free)){
          minimizedBtn.OnWindowUnpinned();
         }
         Logs.Debug(()=>"'OnUnpin':"+dockingState);
        }
        internal bool OnChangeDockingState(DockingState newState){
         if(pinOn){
          if(newState==DockingState.Pinned){
           DoChangeDockingState();
           return true;
          }
         }else{
          DoChangeDockingState();
          return true;
         }
         return false;
         void DoChangeDockingState(){
          dockingState=newState;
         }
        }
    }
}