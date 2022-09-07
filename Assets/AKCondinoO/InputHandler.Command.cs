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
            }
         internal static float ROTATION_SENSITIVITY_X=360.0f;
         internal static float ROTATION_SENSITIVITY_Y=360.0f;
         internal static object[]PAUSE={KeyCode.Tab,Modes.AlternateDown};
         internal static object[]FORWARD ={KeyCode.W,Modes.ActiveHeld};
         internal static object[]BACKWARD={KeyCode.S,Modes.ActiveHeld};
         internal static object[]RIGHT   ={KeyCode.D,Modes.ActiveHeld};
         internal static object[]LEFT    ={KeyCode.A,Modes.ActiveHeld};
        }
    }
}