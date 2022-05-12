using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UI.Fixed{
    internal class Stretcher:MonoBehaviour{
     [SerializeField]float screenWidthSubtraction;
     RectTransform rectTransform;
     Vector2 sizeDelta;
     Vector2 anchoredPosition;
        void Awake(){
         rectTransform=GetComponent<RectTransform>();
        }
        void LateUpdate(){
         anchoredPosition=rectTransform.anchoredPosition;
         sizeDelta=rectTransform.sizeDelta;
         float previousSizeDeltaX=sizeDelta.x;
         sizeDelta.x=Screen.width-screenWidthSubtraction;
         rectTransform.sizeDelta=sizeDelta;
         anchoredPosition.x-=(previousSizeDeltaX-sizeDelta.x)/2f;
         rectTransform.anchoredPosition=anchoredPosition;
        }
    }
}