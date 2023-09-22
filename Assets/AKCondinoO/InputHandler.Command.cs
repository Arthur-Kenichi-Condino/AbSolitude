using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal partial class InputHandler{
        internal static class Command{
            internal enum Modes{
             HoldDelayAfterInRange,
             HoldDelay,
             ActiveHeld,
             AlternateDown,
             WhenUp,
             WhenDown,
            }
            internal class CommandState{
             internal object input;internal Modes mode;internal float holdDelay;internal bool inRange;
            }
         internal static float ROTATION_SENSITIVITY_X=360.0f;
         internal static float ROTATION_SENSITIVITY_Y=360.0f;
         internal static CommandState RELEASE_MOUSE=new CommandState{input=KeyCode.Tab,mode=Modes.AlternateDown};
         internal static CommandState FORWARD =new CommandState{input=KeyCode.W,mode=Modes.ActiveHeld};
         internal static CommandState BACKWARD=new CommandState{input=KeyCode.S,mode=Modes.ActiveHeld};
         internal static CommandState RIGHT   =new CommandState{input=KeyCode.D,mode=Modes.ActiveHeld};
         internal static CommandState LEFT    =new CommandState{input=KeyCode.A,mode=Modes.ActiveHeld};
         internal static CommandState ACTION_1=new CommandState{input=(int)0,mode=Modes.ActiveHeld};
         internal static CommandState ACTION_2=new CommandState{input=(int)1,mode=Modes.AlternateDown};
         internal static CommandState WALK    =new CommandState{input=KeyCode.CapsLock,mode=Modes.AlternateDown};
         internal static CommandState RELOAD  =new CommandState{input=KeyCode.R,mode=Modes.WhenDown};
        }
    }
}