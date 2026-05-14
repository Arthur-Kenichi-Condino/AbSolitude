namespace Unity.Multiplayer.Tools.DependencyInjection
{
    class Context
    {
        public static Container Current { get; } = new Container();

        public static IDependencyResolver Resolver { get; } = new DependencyResolver(Current);
    }
}
