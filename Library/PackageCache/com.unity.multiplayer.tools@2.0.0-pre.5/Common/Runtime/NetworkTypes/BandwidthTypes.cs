using System;

using static Unity.Multiplayer.Tools.Common.BandwidthTypes;

namespace Unity.Multiplayer.Tools.Common
{
    [Flags]
    enum BandwidthTypes
    {
        None = 0,

        /// <summary>
        /// Bandwidth that doesn't fall into any of the other categories
        /// </summary>
        Other  = 1 << 0,
        NetVar = 1 << 1,
        Rpc    = 1 << 2,

        All = Other | NetVar | Rpc
    }

    static class BandwidthTypesExtensions
    {
        public static string DisplayName(this BandwidthTypes direction)
        {
            return direction switch
            {
                None => "None",
                All => "All",
                Other => "Other",
                NetVar => "NetVars",
                Rpc => "RPCs",

                Other | NetVar => "NetVars and Other",
                Other | Rpc => "RPCs and Other",
                NetVar | Rpc => "NetVars and RPCs",

                _ => throw new ArgumentOutOfRangeException($"Unknow {nameof(NetworkDirection)} {direction}")
            };
        }
    }
}

