using System;

namespace Unity.Multiplayer.Tools.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    sealed class InjectAttribute : Attribute
    {
    }
}
