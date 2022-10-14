using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal partial class InputHandler{
        internal static class Enabled{
            internal class EnabledState{
             internal bool curState;internal bool lastState;internal float heldTime;
            }
         internal static readonly float[]MOUSE_ROTATION_DELTA_X={0,0};
         internal static readonly float[]MOUSE_ROTATION_DELTA_Y={0,0};
         internal static readonly EnabledState RELEASE_MOUSE=new EnabledState{curState=true,lastState=true};
         internal static readonly EnabledState FORWARD =new EnabledState{curState=false,lastState=false};
         internal static readonly EnabledState BACKWARD=new EnabledState{curState=false,lastState=false};
         internal static readonly EnabledState RIGHT   =new EnabledState{curState=false,lastState=false};
         internal static readonly EnabledState LEFT    =new EnabledState{curState=false,lastState=false};
        }
    }
}