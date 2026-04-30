using System;
using JetBrains.Annotations;

namespace Unity.Multiplayer.Tools.DependencyInjection
{
    class DependencyDefinition
    {
        DependencyDefinition(
            Type dependencyType,
            Type implementationType,
            Lifetime lifetime,
            object implementationInstance = null,
            Func<object> implementationFactory = null)
        {
            DependencyType = dependencyType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
            ImplementationInstance = implementationInstance;
            ImplementationFactory = implementationFactory;
        }

        public Type DependencyType { get; }

        public Type ImplementationType { get; }

        public Lifetime Lifetime { get; }

        [CanBeNull]
        public object ImplementationInstance { get; }

        [CanBeNull]
        public Func<object> ImplementationFactory { get; }

        public static DependencyDefinition Transient<TDependency, TImplementation>()
            where TImplementation : class, TDependency
        {
            return Define(typeof(TDependency), typeof(TImplementation), Lifetime.Transient);
        }

        public static DependencyDefinition Transient<TDependency, TImplementation>(Func<TImplementation> implementationFactory)
            where TImplementation : class, TDependency
        {
            return Define(typeof(TDependency), typeof(TImplementation), Lifetime.Transient, implementationFactory);
        }

        public static DependencyDefinition Singleton<TDependency, TImplementation>()
            where TImplementation : class, TDependency
        {
            return Define(typeof(TDependency), typeof(TImplementation), Lifetime.Singleton);
        }

        public static DependencyDefinition Singleton<TDependency, TImplementation>(TImplementation implementationInstance)
            where TImplementation : class, TDependency
        {
            return Define(typeof(TDependency), typeof(TImplementation), Lifetime.Singleton, implementationInstance);
        }

        static DependencyDefinition Define(Type dependencyType, Type implementationType, Lifetime lifetime)
        {
            return new DependencyDefinition(dependencyType, implementationType, lifetime);
        }

        static DependencyDefinition Define(Type dependencyType, Type implementationType, Lifetime lifetime, object implementationInstance)
        {
            return new DependencyDefinition(dependencyType, implementationType, lifetime, implementationInstance);
        }

        static DependencyDefinition Define(Type dependencyType, Type implementationType, Lifetime lifetime, Func<object> implementationFactory)
        {
            return new DependencyDefinition(dependencyType, implementationType, lifetime, implementationFactory);
        }
    }
}
