#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UI.Fixed{
    internal class ConsoleInputField:MonoBehaviour{
     internal TMP_InputField inputField;
     internal TextMeshProUGUI placeholder;
     internal TextMeshProUGUI textInput;
        void Awake(){
         inputField=GetComponent<TMP_InputField>();
         GameObject placeholderObject=Util.FindChildRecursively(transform,"Placeholder").gameObject;
         GameObject   textInputObject=Util.FindChildRecursively(transform,"Text"       ).gameObject;
         placeholder=placeholderObject.GetComponent<TextMeshProUGUI>();
         textInput  =  textInputObject.GetComponent<TextMeshProUGUI>();
        }
        public void OnEndEdit(){
         Log.DebugMessage("console:"+textInput.text);
         if(textInput.text.StartsWith("DEBUG_STOP_FOLLOWING")){
          var gO=GameObject.Find("Main Camera");
          if(gO!=null){
           var mainCamera=gO.GetComponentInChildren<MainCamera>();
           if(mainCamera!=null){
            mainCamera.DEBUG_STOP_FOLLOWING=true;
           }
          }
         }
         inputField.text="";
        }
    }
}