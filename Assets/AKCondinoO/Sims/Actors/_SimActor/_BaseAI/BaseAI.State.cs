#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     protected State MyState=State.IDLE_ST;internal State state{get{return MyState;}}
        internal enum State:int{
           IDLE_ST=0,
         FOLLOW_ST=1,
          CHASE_ST=2,
         ATTACK_ST=3,
        }
    }
}