#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UI{
    internal class StatBar:MonoBehaviour{
     [SerializeField]internal Image backgroundBar;
     [SerializeField]internal Image bar;
     [SerializeField]internal Color[]colorsFromFullToEmpty;
     [SerializeField]internal float[]colorsFromFullToEmptyWeight;
     float maxValue=1f;
        internal void SetMaxValue(float maxValue){
         this.maxValue=maxValue;
        }
     float value=.25f;
        internal void SetValue(float value){
         this.value=value;
        }
     float percentage;
        void OnGUI(){
         percentage=value/maxValue;
         Vector2 sizeDelta=bar.rectTransform.sizeDelta;
         sizeDelta.x=percentage*backgroundBar.rectTransform.sizeDelta.x;
         bar.rectTransform.sizeDelta=sizeDelta;
         Vector2 anchoredPosition=bar.rectTransform.anchoredPosition;
         anchoredPosition.x=(backgroundBar.rectTransform.sizeDelta.x/2f)*percentage;
         bar.rectTransform.anchoredPosition=anchoredPosition;
        }
    }
}