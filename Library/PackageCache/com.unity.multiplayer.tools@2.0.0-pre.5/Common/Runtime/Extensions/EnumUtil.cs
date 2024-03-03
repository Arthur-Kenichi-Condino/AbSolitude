using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Multiplayer.Tools.Common
{
    static class EnumUtil
    {
        public static T[] GetValues<T>() where T : unmanaged, Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static string[] GetNames<T>() where T : unmanaged, Enum
        {
            return Enum.GetNames(typeof(T));
        }

        public static IEnumerable<(string name, T value)> GetValuesAndNames<T>(params T[] skip) where T : unmanaged, Enum
        {
            foreach (var name in GetNames<T>())
            {
                var value = Enum.Parse<T>(name);

                if (skip != null && skip.Contains(value))
                {
                    continue;
                }
                yield return (name, value);
            }
        }

        /// <summary>
        /// Warning: this cast is unsafe. It does not perform any type checking you.
        /// You must ensure that the underlying type is correct.
        /// </summary>
        /// <remarks>
        /// Convert.ToInt32 is orders of magnitude slower than this method,
        /// and allocates with each call, so this method is needed in order
        /// for this <see cref="EnumMap{a,b}"/> to perform better than a Dictionary.
        /// <br/>
        /// Using Convert.ToInt32 this class will perform a little worse
        /// (maybe 20-30% worse) than a Dictionary, and will allocate with
        /// each read and write.
        /// <br/>
        /// Using the unsafe CastEnumToInt method, this class performs
        /// ~3.6 times faster than a dictionary, and allocates 8 times less.
        /// </remarks>
        public static unsafe TUnderlying UnsafeCastToUnderlying<TEnum, TUnderlying>(this TEnum enumValue)
            where TEnum: unmanaged, Enum
            where TUnderlying: unmanaged
        {
            return *(TUnderlying*)(&enumValue);
        }

        /// <summary>
        /// Warning: this cast is unsafe. It does not perform any type checking for you.
        /// You must ensure that the underlying type is correct.
        /// </summary>
        /// <remarks>
        /// Convert.ToInt32 is orders of magnitude slower than this method,
        /// and allocates with each call, so this method is needed in order
        /// for this <see cref="EnumMap{a,b}"/> to perform better than a Dictionary.
        /// <br/>
        /// Using Convert.ToInt32 this class will perform a little worse
        /// (maybe 20-30% worse) than a Dictionary, and will allocate with
        /// each read and write.
        /// <br/>
        /// Using the unsafe CastEnumToInt method, this class performs
        /// ~3.6 times faster than a dictionary, and allocates 8 times less.
        /// </remarks>
        public static unsafe int UnsafeCastToInt<TEnum>(this TEnum enumValue)
            where TEnum: unmanaged, Enum
        {
            return *(int*)(&enumValue);
        }

        /// <summary>
        /// Warning: this cast is unsafe. It does not perform any type checking for you.
        /// You must ensure that the underlying type is correct.
        /// </summary>
        public static unsafe TEnum UnsafeCastToEnum<TEnum>(this int value)
            where TEnum: unmanaged, Enum
        {
            return *(TEnum*)(&value);
        }

        /// <summary>
        /// Warning: this cast is unsafe. It does not perform any type checking for you.
        /// You must ensure that the underlying type is correct.
        /// </summary>
        public static unsafe TEnum UnsafeCastToEnum<TUnderlying, TEnum>(this TUnderlying value)
            where TUnderlying: unmanaged
            where TEnum: unmanaged, Enum
        {
            return *(TEnum*)(&value);
        }

        /// <returns>
        /// True if this enum value contains one or more of the flags in <i><b>b</b></i> and false otherwise
        /// </returns>
        /// <exception cref="EnumWithoutFlagsAttributeException{TEnum}">
        /// Is thrown if TEnum does not have the FlagsAttribute
        /// </exception>
        /// <exception cref="UnhandledEnumBackingTypeException{TEnum,TValue}">
        /// Is thrown if TEnum does not have the FlagsAttribute
        /// </exception>
        public static bool ContainsAny<TEnum>(this TEnum a, TEnum b)
            where TEnum: unmanaged, Enum
        {
            return IntFlagEnumUtils<TEnum>.ContainsAny(a, b);
        }

        /// <returns>
        /// True if this enum value contains all of the flags in <i><b>b</b></i> and false otherwise
        /// </returns>
        /// <exception cref="EnumWithoutFlagsAttributeException{TEnum}">
        /// Is thrown if TEnum does not have the FlagsAttribute
        /// </exception>
        public static bool ContainsAll<TEnum>(this TEnum a, TEnum b)
            where TEnum: unmanaged, Enum
        {
            return IntFlagEnumUtils<TEnum>.ContainsAll(a, b);
        }

        /// <exception cref="EnumWithoutFlagsAttributeException{TEnum}">
        /// Is thrown if TEnum does not have the FlagsAttribute
        /// </exception>
        public static TEnum SetFlags<TEnum>(this TEnum a, TEnum b, bool value)
            where TEnum: unmanaged, Enum
        {
            return IntFlagEnumUtils<TEnum>.SetFlags(a, b, value);
        }

        /// <exception cref="EnumWithoutFlagsAttributeException{TEnum}">
        /// Is thrown if TEnum does not have the FlagsAttribute
        /// </exception>
        public static void SetFlagsInPlace<TEnum>(ref this TEnum a, TEnum b, bool value)
            where TEnum: unmanaged, Enum
        {
            a = IntFlagEnumUtils<TEnum>.SetFlags(a, b, value);
        }
    }

    /// <remarks>
    /// This static class is used to enforce the constraint (via its static constructor)
    /// that the generic enum parameter passed has an underlying type of TUnderlying.
    /// </remarks>
    static class CheckedEnumUtils<TEnum, TUnderlying>
        where TEnum : unmanaged, Enum
        where TUnderlying : unmanaged
    {
        static CheckedEnumUtils()
        {
            var type = typeof(TEnum);
            if (Enum.GetUnderlyingType(type) != typeof(TUnderlying))
            {
                throw new UnhandledEnumUnderlyingTypeException<TEnum, int>();
            }
        }

        /// <remarks>
        /// Returns the unsafe value, but the static constructor ensures an exception
        /// is thrown if this is called anywhere on an enum that does not have an
        /// underlying type of TUnderlying.
        /// </remarks>
        public static TUnderlying CheckedCastToUnderlying(TEnum value)
        {
            return value.UnsafeCastToUnderlying<TEnum, TUnderlying>();
        }
    }

    /// <remarks>
    /// This static class is used to enforce the constraint (via its static constructor)
    /// that the generic parameter passed to it has the flags attribute.
    /// </remarks>
    static class IntFlagEnumUtils<TEnum> where TEnum : unmanaged, Enum
    {
        /// <remarks>
        /// Ensure that TEnum has the flags attribute
        /// </remarks>
        static IntFlagEnumUtils()
        {
            var type = typeof(TEnum);
            if (type.GetCustomAttributes(typeof(FlagsAttribute), true).Length <= 0)
            {
                throw new EnumWithoutFlagsAttributeException<TEnum>();
            }
            if (Enum.GetUnderlyingType(type) != typeof(int))
            {
                throw new UnhandledEnumUnderlyingTypeException<TEnum, int>();
            }
        }

        public static bool ContainsAny(TEnum a, TEnum b)
        {
            return (a.UnsafeCastToInt() & b.UnsafeCastToInt()) != 0;
        }

        public static bool ContainsAll(TEnum a, TEnum b)
        {
            return (a.UnsafeCastToInt() & b.UnsafeCastToInt()) == b.UnsafeCastToInt();
        }

        public static TEnum SetFlags(TEnum a, TEnum b, bool value)
        {
            return (value
                ? a.UnsafeCastToInt() | b.UnsafeCastToInt()
                : a.UnsafeCastToInt() & ~b.UnsafeCastToInt()).UnsafeCastToEnum<TEnum>();
        }
    }

    class EnumWithoutFlagsAttributeException<TEnum> : Exception
        where TEnum : unmanaged, Enum
    {
        public EnumWithoutFlagsAttributeException()
            : base($"Cannot use {nameof(EnumUtil.ContainsAny)}, {nameof(EnumUtil.SetFlags)}, or {nameof(EnumUtil.SetFlagsInPlace)} " +
                   $"on enum {nameof(TEnum)}, as it does not have the {nameof(FlagsAttribute)} attribute")
        {}
    }

    class UnhandledEnumUnderlyingTypeException<TEnum, TRequiredUnderlyingType> : Exception
        where TEnum : unmanaged, Enum
    {
        public UnhandledEnumUnderlyingTypeException()
            : base($"Cannot use {nameof(EnumUtil.ContainsAny)}, {nameof(EnumUtil.SetFlags)}, or {nameof(EnumUtil.SetFlagsInPlace)} " +
                   $"on enum {nameof(TEnum)}, because its underlying type {typeof(TEnum).UnderlyingSystemType} is not the required" +
                   $" underlying type {typeof(TRequiredUnderlyingType)}")
        {}
    }
}
