#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal partial class InputHandler:MonoBehaviour,ISingletonInitialization{
     internal static InputHandler singleton{get;set;}
     readonly Func<KeyCode,bool>[]  keyboardGetters=new Func<KeyCode,bool>[3]{Input.GetKey        ,Input.GetKeyUp        ,Input.GetKeyDown        ,};
     readonly Func<int    ,bool>[]     mouseGetters=new Func<int    ,bool>[3]{Input.GetMouseButton,Input.GetMouseButtonUp,Input.GetMouseButtonDown,};
     readonly Func<string ,bool>[]controllerGetters=new Func<string ,bool>[3]{Input.GetButton     ,Input.GetButtonUp     ,Input.GetButtonDown     ,};
      readonly Dictionary<Type,object[]>getters=new Dictionary<Type,object[]>();
        #pragma warning disable IDE0051 //  Ignore "remover membros privados não utilizados"
        bool Get(Func<KeyCode,bool>  keyboardGet,KeyCode   key){return   keyboardGet(   key);}
        bool Get(Func<int    ,bool>     mouseGet,int    button){return      mouseGet(button);}
        bool Get(Func<string ,bool>controllerGet,string button){return controllerGet(button);}
        #pragma warning restore IDE0051
      readonly Dictionary<Type,Delegate>delegates=new Dictionary<Type,Delegate>();
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("InputHandler:OnDestroyingCoreEvent");
        }
    }
}