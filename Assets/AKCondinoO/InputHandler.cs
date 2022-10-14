#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static AKCondinoO.InputHandler.Command;
using static AKCondinoO.InputHandler.Enabled;
namespace AKCondinoO{
    internal partial class InputHandler:MonoBehaviour,ISingletonInitialization{
     internal static InputHandler singleton{get;set;}
     internal readonly Dictionary<string,CommandState>CommandDictionary=new Dictionary<string,CommandState>();
     internal readonly Dictionary<string,EnabledState>EnabledDictionary=new Dictionary<string,EnabledState>();
        enum GetterReturnMode:int{
         HeldDown=0,
         Up      =1,
         Down    =2,
        }
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
         foreach(FieldInfo field in typeof(Command).GetFields(BindingFlags.NonPublic|BindingFlags.Static)){
          if(field.GetValue(null)is CommandState command){
           CommandDictionary.Add(field.Name,command);
          }
         }
         foreach(FieldInfo field in typeof(Enabled).GetFields(BindingFlags.NonPublic|BindingFlags.Static)){
          if(field.GetValue(null)is EnabledState enabled){
           EnabledDictionary.Add(field.Name,enabled);
          }
         }
         getters.Add(typeof(KeyCode),  keyboardGetters);
         getters.Add(typeof(int    ),     mouseGetters);
         getters.Add(typeof(string ),controllerGetters);
         foreach(MethodInfo method in GetType().GetMethods(BindingFlags.NonPublic|BindingFlags.Instance)){
          if(method.Name=="Get"){
           Type inputType=method.GetParameters()[1].ParameterType;
           Delegate result;
           if(inputType==typeof(KeyCode))result=method.CreateDelegate(typeof(Func<Func<KeyCode,bool>,KeyCode,bool>),this);else
           if(inputType==typeof(int    ))result=method.CreateDelegate(typeof(Func<Func<int    ,bool>,int    ,bool>),this);else
                                         result=method.CreateDelegate(typeof(Func<Func<string ,bool>,string ,bool>),this);
           delegates[inputType]=result;
          }
         }
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("InputHandler:OnDestroyingCoreEvent");
        }
        bool InvokeDelegate(KeyValuePair<string,CommandState>command,Type inputType,GetterReturnMode returnMode){
         if(inputType==typeof(KeyCode))return((Func<Func<KeyCode,bool>,KeyCode,bool>)delegates[inputType]).Invoke((Func<KeyCode,bool>)getters[inputType][(int)returnMode],(KeyCode)command.Value.input);else
         if(inputType==typeof(int    ))return((Func<Func<int    ,bool>,int    ,bool>)delegates[inputType]).Invoke((Func<int    ,bool>)getters[inputType][(int)returnMode],(int    )command.Value.input);else
                                       return((Func<Func<string ,bool>,string ,bool>)delegates[inputType]).Invoke((Func<string ,bool>)getters[inputType][(int)returnMode],(string )command.Value.input);
        }
        internal bool focus=true;
        void OnApplicationFocus(bool focus){
         this.focus=focus;
        }
        internal bool escape;
        //  [https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/]
        void Update(){
         escape=Input.GetKey(KeyCode.Escape)||Input.GetKeyUp(KeyCode.Escape)||Input.GetKeyDown(KeyCode.Escape);
         foreach(var command in CommandDictionary){
          string        name=command.Key;
          Type          type=command.Value.input.GetType();
          Command.Modes mode=command.Value.mode;
          EnabledState enabled=EnabledDictionary[name];
          enabled.lastState=enabled.curState;
             if(mode==Command.Modes.HoldDelayAfterInRange){
                 enabled.curState=false;
                 if(command.Value.inRange&&InvokeDelegate(command,type,GetterReturnMode.HeldDown)){
                  float heldTime=enabled.heldTime;
                        heldTime+=Time.deltaTime;
                  if(heldTime>=command.Value.holdDelay){
                   heldTime=0;
                   enabled.curState=true;
                  }
                  enabled.heldTime=heldTime;
                 }else{
                  enabled.heldTime=0f;
                 }
                 command.Value.inRange=false;
             }else if(mode==Command.Modes.HoldDelay){
                 enabled.curState=false;
                 if(InvokeDelegate(command,type,GetterReturnMode.HeldDown)){
                  float heldTime=enabled.heldTime;
                        heldTime+=Time.deltaTime;
                  if(heldTime>=command.Value.holdDelay){
                   heldTime=0;
                   enabled.curState=true;
                  }
                  enabled.heldTime=heldTime;
                 }else{
                  enabled.heldTime=0f;
                 }
             }else if(mode==Command.Modes.ActiveHeld){
                 enabled.curState=InvokeDelegate(command,type,GetterReturnMode.HeldDown);
             }else if(mode==Command.Modes.AlternateDown){
                 if(InvokeDelegate(command,type,GetterReturnMode.Down)){
                  enabled.curState=!enabled.curState;
                 }
             }
         }
         Enabled.RELEASE_MOUSE.curState=Enabled.RELEASE_MOUSE.curState||escape||!focus;
         Enabled.MOUSE_ROTATION_DELTA_X[1]=Enabled.MOUSE_ROTATION_DELTA_X[0];Enabled.MOUSE_ROTATION_DELTA_X[0]=Command.ROTATION_SENSITIVITY_X*Input.GetAxis("Mouse X");
         Enabled.MOUSE_ROTATION_DELTA_Y[1]=Enabled.MOUSE_ROTATION_DELTA_Y[0];Enabled.MOUSE_ROTATION_DELTA_Y[0]=Command.ROTATION_SENSITIVITY_Y*Input.GetAxis("Mouse Y");
        }
    }
}