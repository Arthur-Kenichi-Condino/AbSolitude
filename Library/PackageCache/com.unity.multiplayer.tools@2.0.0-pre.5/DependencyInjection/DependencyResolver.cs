using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Multiplayer.Tools.DependencyInjection
{
    interface IDependencyResolver
    {
        object Resolve(Type type);
    }

    class DependencyResolver : IDependencyResolver
    {
        readonly Container m_Container;
        readonly Dictionary<Type, object> m_SingletonInstances = new Dictionary<Type, object>();

        public DependencyResolver(Container container)
        {
            m_Container = container;
        }

        public object Resolve(Type type)
        {
            var resolved = TryResolve(type, out var dependency);
            if (!resolved)
            {
                throw new MissingDependencyException(type);
            }

            return dependency;
        }

        bool TryResolve(Type type, out object dependency)
        {
            dependency = default;

            var singletonDefinitionCandidate = m_Container.DependencyDefinitions
                .LastOrDefault(x => x.DependencyType == type &&
                                     x.Lifetime == Lifetime.Singleton);
            if (singletonDefinitionCandidate != default)
            {
                if (m_SingletonInstances.TryGetValue(type, out var singletonInstance))
                {
                    dependency = singletonInstance;
                }
                else
                {
                    dependency = Instantiate(singletonDefinitionCandidate);
                    m_SingletonInstances[type] = dependency;
                }

                return true;
            }

            var transientDefinitionCandidate = m_Container.DependencyDefinitions
                .LastOrDefault(x => x.DependencyType == type &&
                                    x.Lifetime == Lifetime.Transient);
            if (transientDefinitionCandidate != default)
            {
                dependency = Instantiate(transientDefinitionCandidate);
                return true;
            }

            return false;
        }

        static object Instantiate(DependencyDefinition definition)
        {
            if (definition.ImplementationInstance != null)
            {
                return definition.ImplementationInstance;
            }

            if (definition.ImplementationFactory != null)
            {
                return definition.ImplementationFactory.Invoke();
            }

            var parameterlessConstructor = definition.ImplementationType.GetConstructor(Type.EmptyTypes);
            if (parameterlessConstructor != null)
            {
                return parameterlessConstructor.Invoke(Array.Empty<object>());
            }

            throw new DependencyInstantiationException(definition.ImplementationType);
        }

        class MissingDependencyException : Exception
        {
            public MissingDependencyException(Type type)
                : base($"Could not resolve dependency for type '{type.AssemblyQualifiedName}'.")
            {
            }
        }

        class DependencyInstantiationException : Exception
        {
            public DependencyInstantiationException(Type type)
                : base($"Could not instantiate dependency of type '{type.AssemblyQualifiedName}'.")
            {

            }
        }
    }
}
