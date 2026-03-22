using AKCondinoO.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    [DefaultExecutionOrder(-99999)]
    internal class InputHandler:MonoSingleton<InputHandler>{
     [SerializeField]internal InputBindingsAsset inputBindingsSetting;
     [SerializeField]private int debugTryLogTranslateInputThisManyFrames=0;
        public override void Initialize(){
         base.Initialize();
         if(this!=null){
          Init();
         }
        }
        public override void PreShutdown(){
         if(this!=null){
         }
         base.PreShutdown();
        }
        public override void Shutdown(){
         if(this!=null){
         }
         base.Shutdown();
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         TranslateInput();
        }
     internal readonly List<(InputBinding binding,InputAction action)>bindings=new();
        private void Init(){
         if(inputBindingsSetting!=null){
          foreach(var bindingSetting in inputBindingsSetting.bindings){
           bindings.Add((new(bindingSetting),bindingSetting.action));
          }
          bindings.Sort(InputBinding.Compare);
         }
        }
        private void TranslateInput(){
         for(int i=0;i<bindings.Count;i++){
          var pair=bindings[i];
          var action=pair.action;
          var binding=pair.binding;
          //Logs.Debug("binding:"+binding,null,debugTryLogTranslateInputThisManyFrames>0);
          switch(binding.combinationType){
           case(InputCombinationType.Single):{
            bool result;
            result=ReadInput(binding.inputCombination[0]);
            if(result){
             Logs.Debug("action:"+action+":input received:"+binding,null,debugTryLogTranslateInputThisManyFrames>0);
            }
            break;
           }
          }
         }
         if(debugTryLogTranslateInputThisManyFrames>0){debugTryLogTranslateInputThisManyFrames--;}
        }
        private bool ReadInput(DeviceInput deviceInput){
         switch(deviceInput.source){
          case(DeviceInputSource.Keyboard):{
           bool readValue=false;
           switch(deviceInput.mode){
            case(InputDetectionMode.WhenDown):{
             readValue=Input.GetKeyDown(deviceInput.key);
             break;
            }
           }
           return readValue;
          }
         }
         return false;
        }
    }
    internal struct InputBinding{
     public InputCombinationType combinationType;
     public readonly DeviceInput[]inputCombination;
        internal InputBinding(InputBindingEntry bindingSetting){
         combinationType=bindingSetting.combinationType;
         inputCombination=bindingSetting.inputCombination.ToArray();
         Array.Sort(inputCombination,DeviceInput.Compare);
        }
        internal static int Compare((InputBinding binding,InputAction action)itemA,(InputBinding binding,InputAction action)itemB){
         InputBinding a=itemA.binding;
         InputBinding b=itemB.binding;
         int c;
         c=GetCombinationPriority(a.combinationType)-
           GetCombinationPriority(b.combinationType);
         if(c!=0)return c;
         c=b.inputCombination.Length-a.inputCombination.Length;
         if(c!=0)return c;
         c=GetBindingModePriority(a)-
           GetBindingModePriority(b);
         if(c!=0)return c;
            static int GetBindingModePriority(InputBinding b){
             int best=int.MaxValue;
             for(int i=0;i<b.inputCombination.Length;i++){
              int p=GetModePriority(b.inputCombination[i].mode);
              if(p<best)best=p;
             }
             return best;
            }
         return CompareInputs(a,b);
            static int CompareInputs(InputBinding a,InputBinding b){
             int len=Math.Min(a.inputCombination.Length,b.inputCombination.Length);
             for(int i=0;i<len;i++){
              int c=DeviceInput.Compare(a.inputCombination[i],b.inputCombination[i]);
              if(c!=0)return c;
             }
             return 0;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetCombinationPriority(InputCombinationType t){
         return t switch{
          InputCombinationType.Sequence=>0,
          InputCombinationType.Chord   =>1,
          InputCombinationType.Single  =>2,
                                      _=>3,
         };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetModePriority(InputDetectionMode m){
         return m switch{
          InputDetectionMode.AfterHoldingDelay=>0,
          InputDetectionMode.WhileHeld        =>1,
          InputDetectionMode.WhenDown         =>2,
                                             _=>3,
         };
        }
     internal static readonly Utilities.ObjectPool<StringBuilder>sbPool=
      Pool.GetPool<StringBuilder>(
       "",
       ()=>new(),
       (StringBuilder item)=>{
        item.Clear();
       }
      );
        public override string ToString(){
         var sb=sbPool.Rent();
         sb.Append(base.ToString()).Append(":").Append(combinationType).Append(":");
         foreach(var input in inputCombination){
          switch(input.source){
           case(DeviceInputSource.Keyboard):{
            sb.Append(input.key);
            break;
           }
           case(DeviceInputSource.Mouse):{
            sb.Append(input.mouseButton);
            break;
           }
           case(DeviceInputSource.Controller):{
            sb.Append(input.controllerInput);
            break;
           }
          }
          sb.Append("(").Append(input.mode).Append(")");
         }
         string result=sb.ToString();
         sbPool.Return(sb);
         return result;
        }
    }
    internal enum InputCombinationType{
     Single,
     Chord,
     Sequence,
    }
    [Serializable]
    internal struct DeviceInput{
     public DeviceInputSource source;
     public InputDetectionMode mode;
     public KeyCode key;
     public int mouseButton;
     public string controllerInput;
        internal static int Compare(DeviceInput a,DeviceInput b){
         int c;
         c=(int)a.source-(int)b.source;
         if(c!=0)return c;
         if(a.source==DeviceInputSource.Keyboard){
          c=((int)a.key)-((int)b.key);
          if(c!=0)return c;
         }
         if(a.source==DeviceInputSource.Mouse){
          c=a.mouseButton-b.mouseButton;
          if(c!=0)return c;
         }
         return string.Compare(a.controllerInput,b.controllerInput,StringComparison.Ordinal);
        }
    }
    internal enum InputDetectionMode{
     WhenDown,
     WhileHeld,
     AfterHoldingDelay,
    }
    internal enum DeviceInputSource:byte{
     Keyboard,
     Mouse,
     Controller,
    }
}