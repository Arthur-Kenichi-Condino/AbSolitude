using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal partial class InputHandler{
        internal static class Command{
            internal enum Modes{
             holdDelayAfterInRange,
             holdDelay,
             activeHeld,
             alternateDown,
             whenUp,
            }
         internal static float ROTATION_SENSITIVITY_X=360.0f;
         internal static float ROTATION_SENSITIVITY_Y=360.0f;
         internal static object[]PAUSE={KeyCode.Tab,Modes.alternateDown};
         internal static object[]FORWARD ={KeyCode.W,Modes.activeHeld};
         internal static object[]BACKWARD={KeyCode.S,Modes.activeHeld};
         internal static object[]RIGHT   ={KeyCode.D,Modes.activeHeld};
         internal static object[]LEFT    ={KeyCode.A,Modes.activeHeld};
        }
    }
}