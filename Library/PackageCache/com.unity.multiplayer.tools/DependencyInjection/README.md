# Unity.Multiplayer.Tools.DependencyInjection

## What is dependency injection
Dependency injection is a design pattern used in software development to improve the flexibility and maintainability of code.
In a nutshell, it allows classes to be created and configured externally, and then passed into other classes as dependencies
rather than being directly instantiated within those classes. This enables a separation of concerns, making it easier to manage
dependencies and modify the behavior of an application without needing to modify its core code.

The goal of this library is to provide a simple and easy-to-use implementation of dependency injection for the
different tools assemblies.

## Registering dependencies

Dependency configuration is typically centralized for each different tool in a Context or Module class. Dependencies should be
abstracted behind interfaces, and registration allows you to inform the application which implementation of that interface to use
wherever it is needed.

For example, given the following components:

```csharp
public interface IDependency
{
    void DoSomething();
}
```

```csharp
public class Dependency
{
    public void DoSomething()
    {
        Debug.Log("Hello world!");
    }
}
```

```csharp
public class TestDependency
{
    public int NumberOfTimesDoSomethingWasCalled { get; }

    public void DoSomething()
    {
        DoSomethingWasCalled++;
    }
}
```

A regular application could register the type `Dependency` to be injected in classes that need `IDependency`, but a different registration
could be used in the testing suite of that application to make sure that `TestDependency` is used instead.

Dependency registration also allows us to configure the lifetime of each dependencies. For example, we might want to ensure that a 
singleton instead is used for a particular dependency. A dependency configured this way is said to have a `Singleton` lifetime. Conversely, we might want a new dependency instantiated every time a consumer
needs it. A dependency configured this way is said to have a `Transient` lifetime.

The component responsible for holding this information is called the `Container`. Dependency registration can be configured on the
container directly.

```csharp
    // Register MyDependency for classes requiring an instance of IDependency, constrained to a singleton instance
    Container.AddSingleton<IDependency, MyDependency>();
     
     // Same as singleton above, but we specify which instance to use
    Container.AddSingleton<IDependency>(new MyDependency());
    
    // Same as singleton above, but classes can use MyDependency directly
    Container.AddSingleton<MyDependency>();
    Container.AddSingleton(new MyDependency());
```

```csharp
    // Register MyDependency for classes requiring an instance of IDependency, constructing a new instance each time
    Container.AddTransient<IDependency, MyDependency>();
     
     // Same as singleton above, but we specify how to construct that instance
    Container.AddTransient<IDependency>(() => new MyDependency());
    
    // Same as transient above, but classes can use MyDependency directly
    Container.AddTransient<MyDependency>();
    Container.AddTransient(new MyDependency());
```

## Resolving dependencies

Dependencies are resolved using the `DependencyResolver` class.

Dependencies are resolved with the following priority:
1. If the last dependency definition for the required type is defined as singleton, and an instance has already been constructed, use
that instance
2. If the last dependency definition for the required type is defined as singleton, and an instance has not yet been constructed,
construct that dependency and use that instance
3. If the last dependency definition for the required type is defined as transient, construct that dependency and use that instance
4. If the dependency has no other usable dependency definition, but has a parameterless constructor, construct and use it

## Assorted notes
- Dependency injection breaks down when the application "knows" it's using dependency injection. Thus, it's best used when registration
logic is well constrained to app/module/context initialization, and entirely absent from other parts of the application. Domain-level
classes should not have direct access to the container or the dependency resolver.
- We currently have an implementation of a class that gets its dependencies injected that derives from `VisualElement` in the
`Unity.Multiplayer.Tools.DependencyInjection.UIElements` assembly. This works well for views, but a more generic approach needs to be
implemented if we want to use dependency injection in a non-UI Elements context. However, UI Elements views are the entry point of
most of our applications, this might not be necessary in the short-term.
- Ideally, we'd be using constructors for dependency injection. This allows for a lot of different advantages, the main one being easier
to use in a context where dependency injection is not used / required. The primary example of this is in tests. For now, we can override
dependencies in the container if we need to test dependency-injected classes.
- This DI mini-framework is custom, but heavily inspired by more fully-fledged DI frameworks out there. It builds on well established
existing principles and behavior. Before adding any functionalities or changing the way the framework works, make sure you review
existing work. This utility should feel very familiar to work with for anyone who has used DI in an application previously.
