using System;

namespace Unity.Multiplayer.Tools.Adapters
{
    /// <summary>
    /// Exception for accessing information from the adapter
    /// when there are no subscribers for this piece of information.
    /// </summary>
    class NoSubscribersException : Exception
    {
        public NoSubscribersException(string resourceName, string subscriptionMethodName)
            : base(
                $"Attempt to use {resourceName} without any subscribers. " +
                $"Subscribe using {subscriptionMethodName}, so that the " +
                $"adapter knows to compute and store this information.")
        {}
    }
}
