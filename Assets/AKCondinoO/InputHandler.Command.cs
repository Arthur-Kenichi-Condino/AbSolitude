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
        }
    }
}