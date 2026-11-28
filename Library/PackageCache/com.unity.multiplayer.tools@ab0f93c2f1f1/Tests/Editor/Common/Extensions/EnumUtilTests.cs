using System;
using NUnit.Framework;

// Shorter file local aliases to help make the test cases easier to read
using RV = Unity.Multiplayer.Tools.Common.Tests.Extensions.RandomValues;

namespace Unity.Multiplayer.Tools.Common.Tests.Extensions
{
    enum S32 : int
    {
        A = 0, B = 1, C = 1, D = 2, E = 3, F = 5, G = 8, H = 13, I = 21, J = 34, K = 55, L = 89, M = 144, N = 233,
        Min = int.MinValue, Max = int.MaxValue,
    }

    enum U8 : byte
    {
        A = 0, B = 1, C = 1, D = 2, E = 3, F = 5, G = 8, H = 13, I = 21, J = 34, K = 55, L = 89, M = 144, N = 233,
        Min = byte.MinValue, Max = byte.MaxValue
    }

    enum S8 : sbyte
    {
        A = 0, B = 1, C = 1, D = 2, E = 3, F = 5, G = 8, H = 13, I = 21, J = 34, K = 55, L = 89, M = -16, N = -105,
        Min = sbyte.MinValue, Max = sbyte.MaxValue
    }

    enum U16 : ushort
    {
        A = 0, B = 1, C = 1, D = 2, E = 3, F = 5, G = 8, H = 13, I = 21, J = 34, K = 55, L = 89, M = 144, N = 233,
        Min = ushort.MinValue, Max = ushort.MaxValue
    }

    enum S16 : short
    {
        A = 0, B = 1, C = 1, D = 2, E = 3, F = 5, G = 8, H = 13, I = 21, J = 34, K = 55, L = 89, M = 144, N = 233,
        Min = short.MinValue, Max = short.MaxValue
    }

    enum U32 : uint
    {
        A = 0, B = 1, C = 1, D = 2, E = 3, F = 5, G = 8, H = 13, I = 21, J = 34, K = 55, L = 89, M = 144, N = 233,
        Min = uint.MinValue, Max = uint.MaxValue
    }

    enum U64 : ulong
    {
        A = 0, B = 1, C = 1, D = 2, E = 3, F = 5, G = 8, H = 13, I = 21, J = 34, K = 55, L = 89, M = 144, N = 233,
        Min = ulong.MinValue, Max = ulong.MaxValue
    }

    enum S64 : long
    {
        A = 0, B = 1, C = 1, D = 2, E = 3, F = 5, G = 8, H = 13, I = 21, J = 34, K = 55, L = 89, M = 144, N = 233,
        Min = long.MinValue, Max = long.MaxValue
    }

    enum RandomValues
    {
        A = 55, B = 54, C = 46, D = -17, E = -69, F = -52, G = -61, H = 14, I = -16, J = 37, K = 22, L = -14, M = -99, N = 95
    }

    [Flags]
    enum Flags
    {
        None = 0,

        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,
        D = 1 << 3,
        E = 1 << 4,
        F = 1 << 5,
        G = 1 << 6,
        H = 1 << 7,
        I = 1 << 8,
        J = 1 << 9,
        K = 1 << 10,
        L = 1 << 11,
        M = 1 << 12,
        N = 1 << 13,

        AEGIJM = A | E | G | I | J | M,

        All = A | B | C | D | E | F | G | H | I | J | K | L | M | N
    }

    [TestFixture]
    class EnumUtilTests
    {
        [TestCase(U8.A, (byte)0)]
        [TestCase(U8.K, (byte)55)]
        [TestCase(U8.N, (byte)233)]
        [TestCase(U8.Min, byte.MinValue)]
        [TestCase(U8.Max, byte.MaxValue)]

        [TestCase(S8.A, (sbyte)0)]
        [TestCase(S8.K, (sbyte)55)]
        [TestCase(S8.N, (sbyte)-105)]
        [TestCase(S8.Min, sbyte.MinValue)]
        [TestCase(S8.Max, sbyte.MaxValue)]

        [TestCase(U16.A, (ushort)0)]
        [TestCase(U16.K, (ushort)55)]
        [TestCase(U16.N, (ushort)233)]
        [TestCase(U16.Min, ushort.MinValue)]
        [TestCase(U16.Max, ushort.MaxValue)]

        [TestCase(S16.A, (short)0)]
        [TestCase(S16.K, (short)55)]
        [TestCase(S16.N, (short)233)]
        [TestCase(S16.Min, short.MinValue)]
        [TestCase(S16.Max, short.MaxValue)]

        [TestCase(U32.A, (uint)0)]
        [TestCase(U32.K, (uint)55)]
        [TestCase(U32.N, (uint)233)]
        [TestCase(U32.Min, uint.MinValue)]
        [TestCase(U32.Max, uint.MaxValue)]

        [TestCase(S32.A, 0)]
        [TestCase(S32.B, 1)]
        [TestCase(S32.C, 1)]
        [TestCase(S32.D, 2)]
        [TestCase(S32.E, 3)]
        [TestCase(S32.F, 5)]
        [TestCase(S32.G, 8)]
        [TestCase(S32.H, 13)]
        [TestCase(S32.I, 21)]
        [TestCase(S32.J, 34)]
        [TestCase(S32.K, 55)]
        [TestCase(S32.L, 89)]
        [TestCase(S32.M, 144)]
        [TestCase(S32.N, 233)]
        [TestCase(S32.Min, int.MinValue)]
        [TestCase(S32.Max, int.MaxValue)]

        [TestCase(U64.A, (ulong)0)]
        [TestCase(U64.K, (ulong)55)]
        [TestCase(U64.N, (ulong)233)]
        [TestCase(U64.Min, ulong.MinValue)]
        [TestCase(U64.Max, ulong.MaxValue)]

        [TestCase(S64.A, (long)0)]
        [TestCase(S64.K, (long)55)]
        [TestCase(S64.N, (long)233)]
        [TestCase(S64.Min, long.MinValue)]
        [TestCase(S64.Max, long.MaxValue)]

        [TestCase(RV.A, 55)]
        [TestCase(RV.B, 54)]
        [TestCase(RV.C, 46)]
        [TestCase(RV.D, -17)]
        [TestCase(RV.E, -69)]
        [TestCase(RV.F, -52)]
        [TestCase(RV.G, -61)]
        [TestCase(RV.H, 14)]
        [TestCase(RV.I, -16)]
        [TestCase(RV.J, 37)]
        [TestCase(RV.K, 22)]
        [TestCase(RV.L, -14)]
        [TestCase(RV.M, -99)]
        [TestCase(RV.N, 95)]

        [TestCase(Flags.None, 0)]
        [TestCase(Flags.A, 1 << 0)]
        [TestCase(Flags.B, 1 << 1)]
        [TestCase(Flags.C, 1 << 2)]
        [TestCase(Flags.D, 1 << 3)]
        [TestCase(Flags.E, 1 << 4)]
        [TestCase(Flags.F, 1 << 5)]
        [TestCase(Flags.G, 1 << 6)]
        [TestCase(Flags.H, 1 << 7)]
        [TestCase(Flags.I, 1 << 8)]
        [TestCase(Flags.J, 1 << 9)]
        [TestCase(Flags.K, 1 << 10)]
        [TestCase(Flags.L, 1 << 11)]
        [TestCase(Flags.M, 1 << 12)]
        [TestCase(Flags.N, 1 << 13)]
        [TestCase(Flags.All, 0b11111111111111)]

        public static void CastToAndFromIntTest<TEnum, TUnderlying>(TEnum enumValue, TUnderlying underlyingValue)
            where TEnum : unmanaged, Enum
            where TUnderlying : unmanaged
        {
            var enumAsUnderlying = enumValue.UnsafeCastToUnderlying<TEnum, TUnderlying>();
            Assert.AreEqual(
                expected: underlyingValue,
                actual: enumAsUnderlying);

            var underlyingAsEnum = underlyingValue.UnsafeCastToEnum<TUnderlying, TEnum>();
            Assert.AreEqual(
                expected: enumValue,
                actual: underlyingAsEnum);
        }

        [TestCase(U8.A, U8.B)]
        [TestCase(S8.A, S8.B)]
        [TestCase(U16.A, U16.B)]
        [TestCase(S16.A, S16.B)]
        [TestCase(U32.A, U32.B)]
        [TestCase(S32.A, S32.B)]
        [TestCase(U64.A, U64.B)]
        [TestCase(S64.A, S64.B)]
        [TestCase(RV.A, RV.B)]
        public static void AttemptToUseFlagsOperationOnNonFlagsEnumThrowsExceptionTest<TEnum>(TEnum a, TEnum b)
            where TEnum : unmanaged, Enum
        {
            Assert.Throws<EnumWithoutFlagsAttributeException<TEnum>>(() =>
            {
                try
                {
                    a.ContainsAny(b);
                }
                catch (TypeInitializationException e)
                {
                    if (e.InnerException != null)
                    {
                        throw e.InnerException;
                    }
                }
            });
        }

        [TestCase(Flags.None, Flags.None, false)]
        [TestCase(Flags.None, Flags.A, false)]
        [TestCase(Flags.None, Flags.B, false)]
        [TestCase(Flags.None, Flags.C, false)]
        [TestCase(Flags.None, Flags.D, false)]
        [TestCase(Flags.None, Flags.E, false)]
        [TestCase(Flags.None, Flags.F, false)]
        [TestCase(Flags.None, Flags.G, false)]
        [TestCase(Flags.None, Flags.H, false)]
        [TestCase(Flags.None, Flags.I, false)]
        [TestCase(Flags.None, Flags.J, false)]
        [TestCase(Flags.None, Flags.K, false)]
        [TestCase(Flags.None, Flags.L, false)]
        [TestCase(Flags.None, Flags.M, false)]
        [TestCase(Flags.None, Flags.N, false)]
        [TestCase(Flags.None, Flags.All, false)]

        [TestCase(Flags.All, Flags.None, false)]
        [TestCase(Flags.All, Flags.A, true)]
        [TestCase(Flags.All, Flags.B, true)]
        [TestCase(Flags.All, Flags.C, true)]
        [TestCase(Flags.All, Flags.D, true)]
        [TestCase(Flags.All, Flags.E, true)]
        [TestCase(Flags.All, Flags.F, true)]
        [TestCase(Flags.All, Flags.G, true)]
        [TestCase(Flags.All, Flags.H, true)]
        [TestCase(Flags.All, Flags.I, true)]
        [TestCase(Flags.All, Flags.J, true)]
        [TestCase(Flags.All, Flags.K, true)]
        [TestCase(Flags.All, Flags.L, true)]
        [TestCase(Flags.All, Flags.M, true)]
        [TestCase(Flags.All, Flags.N, true)]
        [TestCase(Flags.All, Flags.All, true)]

        [TestCase(Flags.A, Flags.None, false)]
        [TestCase(Flags.A, Flags.A, true)]
        [TestCase(Flags.A, Flags.B, false)]
        [TestCase(Flags.A, Flags.C, false)]
        [TestCase(Flags.A, Flags.D, false)]
        [TestCase(Flags.A, Flags.E, false)]
        [TestCase(Flags.A, Flags.F, false)]
        [TestCase(Flags.A, Flags.G, false)]
        [TestCase(Flags.A, Flags.H, false)]
        [TestCase(Flags.A, Flags.I, false)]
        [TestCase(Flags.A, Flags.J, false)]
        [TestCase(Flags.A, Flags.K, false)]
        [TestCase(Flags.A, Flags.L, false)]
        [TestCase(Flags.A, Flags.M, false)]
        [TestCase(Flags.A, Flags.N, false)]
        [TestCase(Flags.A, Flags.All, true)]

        [TestCase(Flags.AEGIJM, Flags.None, false)]
        [TestCase(Flags.AEGIJM, Flags.A, true)]
        [TestCase(Flags.AEGIJM, Flags.B, false)]
        [TestCase(Flags.AEGIJM, Flags.C, false)]
        [TestCase(Flags.AEGIJM, Flags.D, false)]
        [TestCase(Flags.AEGIJM, Flags.E, true)]
        [TestCase(Flags.AEGIJM, Flags.F, false)]
        [TestCase(Flags.AEGIJM, Flags.G, true)]
        [TestCase(Flags.AEGIJM, Flags.H, false)]
        [TestCase(Flags.AEGIJM, Flags.I, true)]
        [TestCase(Flags.AEGIJM, Flags.J, true)]
        [TestCase(Flags.AEGIJM, Flags.K, false)]
        [TestCase(Flags.AEGIJM, Flags.L, false)]
        [TestCase(Flags.AEGIJM, Flags.M, true)]
        [TestCase(Flags.AEGIJM, Flags.N, false)]
        [TestCase(Flags.AEGIJM, Flags.All, true)]

        [TestCase(Flags.AEGIJM, Flags.A | Flags.B, true)]
        [TestCase(Flags.AEGIJM, Flags.A | Flags.E, true)]
        [TestCase(Flags.AEGIJM, Flags.B | Flags.C, false)]
        [TestCase(Flags.AEGIJM, Flags.B | Flags.E, true)]
        [TestCase(Flags.AEGIJM, Flags.C | Flags.D, false)]
        [TestCase(Flags.AEGIJM, Flags.C | Flags.D | Flags.F, false)]
        [TestCase(Flags.AEGIJM, Flags.C | Flags.D | Flags.G, true)]
        [TestCase(Flags.AEGIJM, Flags.C | Flags.D | Flags.H, false)]
        [TestCase(Flags.AEGIJM, Flags.C | Flags.D | Flags.H | Flags.I, true)]
        [TestCase(Flags.AEGIJM, Flags.C | Flags.D | Flags.H | Flags.K, false)]
        [TestCase(Flags.AEGIJM, Flags.C | Flags.D | Flags.H | Flags.K | Flags.M, true)]
        [TestCase(Flags.AEGIJM, Flags.C | Flags.D | Flags.H | Flags.K | Flags.N, false)]
        [TestCase(Flags.AEGIJM, Flags.AEGIJM, true)]
        [TestCase(Flags.AEGIJM, Flags.All, true)]

        public static void ContainsAnyTest<TFlags>(TFlags a, TFlags b, bool expected)
            where TFlags : unmanaged, Enum
        {
            Assert.AreEqual(
                expected: expected,
                actual: a.ContainsAny(b));

            // This operation is commutative, so we might as well test the reverse while we're at it
            Assert.AreEqual(
                expected: expected,
                actual: b.ContainsAny(a));
        }

        [TestCase(Flags.None, Flags.None, true)]
        [TestCase(Flags.None, Flags.A, false)]
        [TestCase(Flags.None, Flags.B, false)]
        [TestCase(Flags.None, Flags.C, false)]
        [TestCase(Flags.None, Flags.D, false)]
        [TestCase(Flags.None, Flags.E, false)]
        [TestCase(Flags.None, Flags.F, false)]
        [TestCase(Flags.None, Flags.G, false)]
        [TestCase(Flags.None, Flags.H, false)]
        [TestCase(Flags.None, Flags.I, false)]
        [TestCase(Flags.None, Flags.J, false)]
        [TestCase(Flags.None, Flags.K, false)]
        [TestCase(Flags.None, Flags.L, false)]
        [TestCase(Flags.None, Flags.M, false)]
        [TestCase(Flags.None, Flags.N, false)]
        [TestCase(Flags.None, Flags.All, false)]

        [TestCase(Flags.All, Flags.None, true)]
        [TestCase(Flags.All, Flags.A, true)]
        [TestCase(Flags.All, Flags.B, true)]
        [TestCase(Flags.All, Flags.C, true)]
        [TestCase(Flags.All, Flags.D, true)]
        [TestCase(Flags.All, Flags.E, true)]
        [TestCase(Flags.All, Flags.F, true)]
        [TestCase(Flags.All, Flags.G, true)]
        [TestCase(Flags.All, Flags.H, true)]
        [TestCase(Flags.All, Flags.I, true)]
        [TestCase(Flags.All, Flags.J, true)]
        [TestCase(Flags.All, Flags.K, true)]
        [TestCase(Flags.All, Flags.L, true)]
        [TestCase(Flags.All, Flags.M, true)]
        [TestCase(Flags.All, Flags.N, true)]
        [TestCase(Flags.All, Flags.All, true)]

        [TestCase(Flags.A, Flags.None, true)]
        [TestCase(Flags.A, Flags.A, true)]
        [TestCase(Flags.A, Flags.B, false)]
        [TestCase(Flags.A, Flags.C, false)]
        [TestCase(Flags.A, Flags.D, false)]
        [TestCase(Flags.A, Flags.E, false)]
        [TestCase(Flags.A, Flags.F, false)]
        [TestCase(Flags.A, Flags.G, false)]
        [TestCase(Flags.A, Flags.H, false)]
        [TestCase(Flags.A, Flags.I, false)]
        [TestCase(Flags.A, Flags.J, false)]
        [TestCase(Flags.A, Flags.K, false)]
        [TestCase(Flags.A, Flags.L, false)]
        [TestCase(Flags.A, Flags.M, false)]
        [TestCase(Flags.A, Flags.N, false)]
        [TestCase(Flags.A, Flags.All, false)]

        [TestCase(Flags.AEGIJM, Flags.None, true)]
        [TestCase(Flags.AEGIJM, Flags.A, true)]
        [TestCase(Flags.AEGIJM, Flags.B, false)]
        [TestCase(Flags.AEGIJM, Flags.C, false)]
        [TestCase(Flags.AEGIJM, Flags.D, false)]
        [TestCase(Flags.AEGIJM, Flags.E, true)]
        [TestCase(Flags.AEGIJM, Flags.F, false)]
        [TestCase(Flags.AEGIJM, Flags.G, true)]
        [TestCase(Flags.AEGIJM, Flags.H, false)]
        [TestCase(Flags.AEGIJM, Flags.I, true)]
        [TestCase(Flags.AEGIJM, Flags.J, true)]
        [TestCase(Flags.AEGIJM, Flags.K, false)]
        [TestCase(Flags.AEGIJM, Flags.L, false)]
        [TestCase(Flags.AEGIJM, Flags.M, true)]
        [TestCase(Flags.AEGIJM, Flags.N, false)]

        [TestCase(Flags.AEGIJM, Flags.A | Flags.B, false)]
        [TestCase(Flags.AEGIJM, Flags.A | Flags.C, false)]
        [TestCase(Flags.AEGIJM, Flags.A | Flags.D, false)]
        [TestCase(Flags.AEGIJM, Flags.A | Flags.E, true)]
        [TestCase(Flags.AEGIJM, Flags.A | Flags.E | Flags.F, false)]
        [TestCase(Flags.AEGIJM, Flags.A | Flags.E | Flags.G, true)]
        [TestCase(Flags.AEGIJM, Flags.A | Flags.E | Flags.G | Flags.H, false)]
        [TestCase(Flags.AEGIJM, Flags.A | Flags.E | Flags.G | Flags.I, true)]
        [TestCase(Flags.AEGIJM, Flags.A | Flags.E | Flags.G | Flags.I | Flags.J, true)]
        [TestCase(Flags.AEGIJM, Flags.A | Flags.E | Flags.G | Flags.I | Flags.J | Flags.K, false)]
        [TestCase(Flags.AEGIJM, Flags.A | Flags.E | Flags.G | Flags.I | Flags.J | Flags.L, false)]
        [TestCase(Flags.AEGIJM, Flags.A | Flags.E | Flags.G | Flags.I | Flags.J | Flags.M, true)]
        [TestCase(Flags.AEGIJM, Flags.A | Flags.E | Flags.G | Flags.I | Flags.J | Flags.M | Flags.N, false)]
        [TestCase(Flags.AEGIJM, Flags.AEGIJM, true)]
        [TestCase(Flags.AEGIJM, Flags.All, false)]
        public static void ContainsAllTest<TFlags>(TFlags a, TFlags b, bool expected)
            where TFlags : unmanaged, Enum
        {
            Assert.AreEqual(
                expected: expected,
                actual: a.ContainsAll(b));
        }

        static void TestSetDifferenceProperties<TFlags>(TFlags a, TFlags b)
            where TFlags : unmanaged, Enum
        {
            var aExcludingB = a.SetFlags(b, false);
            Assert.IsFalse(aExcludingB.ContainsAny(b));
            if (b.ContainsAll(a))
            {
                Assert.AreEqual(
                    expected: 0,
                    actual: aExcludingB.UnsafeCastToInt());
            }
            else
            {
                Assert.IsTrue(a.ContainsAny(aExcludingB));
            }
            if (b.ContainsAny(a))
            {
                Assert.AreNotEqual(a, aExcludingB);
            }
            else
            {
                Assert.AreEqual(a, aExcludingB);
            }
        }

        [TestCase(Flags.None, Flags.None)]
        [TestCase(Flags.None, Flags.A)]
        [TestCase(Flags.None, Flags.B)]
        [TestCase(Flags.None, Flags.C)]
        [TestCase(Flags.None, Flags.G)]
        [TestCase(Flags.None, Flags.B | Flags.C)]
        [TestCase(Flags.None, Flags.B | Flags.C | Flags.F)]
        [TestCase(Flags.None, Flags.B | Flags.C | Flags.F | Flags.G)]
        [TestCase(Flags.None, Flags.B | Flags.C | Flags.F | Flags.G | Flags.J)]
        [TestCase(Flags.None, Flags.B | Flags.C | Flags.F | Flags.G | Flags.J | Flags.M | Flags.N)]
        [TestCase(Flags.None, Flags.AEGIJM)]
        [TestCase(Flags.None, Flags.All)]

        [TestCase(Flags.AEGIJM, Flags.None)]
        [TestCase(Flags.AEGIJM, Flags.A)]
        [TestCase(Flags.AEGIJM, Flags.B)]
        [TestCase(Flags.AEGIJM, Flags.C)]
        [TestCase(Flags.AEGIJM, Flags.G)]
        [TestCase(Flags.AEGIJM, Flags.B | Flags.C)]
        [TestCase(Flags.AEGIJM, Flags.B | Flags.C | Flags.F)]
        [TestCase(Flags.AEGIJM, Flags.B | Flags.C | Flags.F | Flags.G)]
        [TestCase(Flags.AEGIJM, Flags.B | Flags.C | Flags.F | Flags.G | Flags.J)]
        [TestCase(Flags.AEGIJM, Flags.B | Flags.C | Flags.F | Flags.G | Flags.J | Flags.M | Flags.N)]
        [TestCase(Flags.AEGIJM, Flags.AEGIJM)]
        [TestCase(Flags.AEGIJM, Flags.All)]

        [TestCase(Flags.All, Flags.None)]
        [TestCase(Flags.All, Flags.A)]
        [TestCase(Flags.All, Flags.B)]
        [TestCase(Flags.All, Flags.C)]
        [TestCase(Flags.All, Flags.G)]
        [TestCase(Flags.All, Flags.B | Flags.C)]
        [TestCase(Flags.All, Flags.B | Flags.C | Flags.F)]
        [TestCase(Flags.All, Flags.B | Flags.C | Flags.F | Flags.G)]
        [TestCase(Flags.All, Flags.B | Flags.C | Flags.F | Flags.G | Flags.J)]
        [TestCase(Flags.All, Flags.B | Flags.C | Flags.F | Flags.G | Flags.J | Flags.M | Flags.N)]
        [TestCase(Flags.All, Flags.AEGIJM)]
        [TestCase(Flags.All, Flags.All)]
        public static void SetFlagsTest<TFlags>(TFlags a, TFlags b)
            where TFlags : unmanaged, Enum
        {
            {
                var aUnionB = a.SetFlags(b, true);
                Assert.IsTrue(aUnionB.ContainsAll(a));
                Assert.IsTrue(aUnionB.ContainsAll(b));
            }
            TestSetDifferenceProperties(a, b);
            TestSetDifferenceProperties(b, a);
        }
    }
}
