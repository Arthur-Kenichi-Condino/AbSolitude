#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal enum State:int{
         IDLE_ST  =0,
         FOLLOW_ST=1,
        }
    }
}