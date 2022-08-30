#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal enum ActorMotion:int{
         MOTION_STAND=0,
         MOTION_MOVE =1,
        }
    }
}