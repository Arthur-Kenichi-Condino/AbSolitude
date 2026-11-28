using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.Tests;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using UnityEngine;
using System.Text;
using System;

[TestFixture]
[BurstCompile]
internal class xxHash3Tests : CollectionsTestCommonBase
{
    private unsafe void* SanityBuffer;
    private unsafe void* DestinationBuffer;

    private const int SANITY_BUFFER_SIZE = 2367;

    [SetUp]
    public unsafe override void Setup()
    {
        base.Setup();

        unchecked
        {
            uint prime = 2654435761U;
            ulong prime64 = 11400714785074694797UL;
            ulong byteGen = prime;

            SanityBuffer = Memory.Unmanaged.Allocate(SANITY_BUFFER_SIZE, 64, Allocator.Persistent);
            byte* buffer = (byte*)SanityBuffer;

            DestinationBuffer = Memory.Unmanaged.Allocate(SANITY_BUFFER_SIZE, 64, Allocator.Persistent);

            int i;
            for (i=0; i<SANITY_BUFFER_SIZE; i++) {
                buffer[i] = (byte)(byteGen>>56);
                byteGen *= prime64;
            }
        }
    }

    [TearDown]
    public unsafe override void TearDown()
    {
        Memory.Unmanaged.Free(SanityBuffer, Allocator.Persistent);
        Memory.Unmanaged.Free(DestinationBuffer, Allocator.Persistent);
        base.TearDown();
    }

    [BurstCompile(CompileSynchronously = true)]
    struct xxHash3Hash64SanityCheckJob : IJob
    {
        [NativeDisableUnsafePtrRestriction]
        public unsafe void* SanityBuffer;
        [NativeDisableUnsafePtrRestriction]
        public unsafe void* DestinationBuffer;

        public long Length;
        public ulong Seed;

        public NativeArray<uint2> Result;

        public unsafe void Execute()
        {
            var resultIndex = 1;
            // Compute Hash from buffer
            Result[resultIndex++] = xxHash3.Hash64(SanityBuffer, Length, Seed);

            // Hash & copy and Streaming API is currently not supported with Hash64

            // // Compute/Copy (TODO API still not developed)
            // if (DestinationBuffer != null)
            // {
            //     CopySingleCallHashResult = xxHash3.Hash64(SanityBuffer, DestinationBuffer, Length, Seed);
            // }

            // Streaming API Test
            {
                var state = new xxHash3.StreamingState(true, Seed);
                state.Update(SanityBuffer, (int)Length);
                Result[resultIndex++] = state.DigestHash64();
            }

            // 2 updates
            if (Length > 3)
            {
                var state = new xxHash3.StreamingState(true, Seed);
                {
                    state.Update(SanityBuffer, 3);
                    state.Update((byte*)SanityBuffer+3, (int)Length-3);
                    Result[resultIndex++] = state.DigestHash64();
                }
            }

            // byte per byte update
            if (Length > 0) {
                var state = new xxHash3.StreamingState(true, Seed);
                {
                    var bBuffer = (byte*) SanityBuffer;
                    for (int i = 0; i < Length; i++)
                    {
                        state.Update(bBuffer + i, 1);
                    }
                    Result[resultIndex++] = state.DigestHash64();
                }
            }

            Result[0] = new uint2(resultIndex - 1);
        }
    }

    const ulong Prime = 2654435761U;
    const ulong Prime64 = 11400714785074694797UL;

    private unsafe void TestHash64(long length, ulong seed, ulong result, ulong resultWithSeed)
    {
        var job = new xxHash3Hash64SanityCheckJob
        {
            SanityBuffer = SanityBuffer,
            DestinationBuffer = DestinationBuffer,
            Result = CollectionHelper.CreateNativeArray<uint2>(10, CommonRwdAllocator.Handle),
            Seed = 0,
            Length = length
        };

        var b = xxHash3.ToUint2(result);
        job.Schedule().Complete();
        var resultCount = job.Result[0].x;
        for (int i = 0; i < resultCount; i++)
        {
            var a = job.Result[i+1];
            Assert.That(a, Is.EqualTo(b), $"Failed on entry {i}");
        }

        job.Seed = seed;
        job.Schedule().Complete();

        b = xxHash3.ToUint2(resultWithSeed);
        resultCount = job.Result[0].x;
        for (int i = 0; i < resultCount; i++)
        {
            Assert.That(job.Result[i+1], Is.EqualTo(b), $"Failed on entry {i}");
        }

        job.Result.Dispose();
    }

    [Test]
    public void xxHash3_Hash_64_Length0000()
    {
        TestHash64(0, Prime64, 0x2D06800538D394C2UL, 0xA8A6B918B2F0364AUL);
    }

    [Test]
    public void xxHash3_Hash_64_Length0001()
    {
        TestHash64(1, Prime64, 0xC44BDFF4074EECDBUL, 0x032BE332DD766EF8UL);
    }

    [Test]
    public void xxHash3_Hash_64_Length0006()
    {
        TestHash64(6, Prime64, 0x27B56A84CD2D7325UL, 0x84589C116AB59AB9UL);
    }

    [Test]
    public void xxHash3_Hash_64_Length0012()
    {
        TestHash64(12, Prime64, 0xA713DAF0DFBB77E7UL, 0xE7303E1B2336DE0EUL);
    }

    [Test]
    public void xxHash3_Hash_64_Length0024()
    {
        TestHash64(24, Prime64, 0xA3FE70BF9D3510EBUL, 0x850E80FC35BDD690UL);
    }

    [Test]
    public void xxHash3_Hash_64_Length0048()
    {
        TestHash64(48, Prime64, 0x397DA259ECBA1F11UL, 0xADC2CBAA44ACC616UL);
    }


    [Test]
    public void xxHash3_Hash_64_Length0080()
    {
        TestHash64(80, Prime64, 0xBCDEFBBB2C47C90AUL, 0xC6DD0CB699532E73UL);
    }

    [Test]
    public void xxHash3_Hash_64_Length0195()
    {
        TestHash64(195, Prime64, 0xCD94217EE362EC3AUL, 0xBA68003D370CB3D9UL);
    }

    [Test]
    public void xxHash3_Hash_64_Length0403()
    {
        TestHash64(403, Prime64, 0xCDEB804D65C6DEA4UL, 0x6259F6ECFD6443FDUL);
    }

    [Test]
    public void xxHash3_Hash_64_Length0512()
    {
        TestHash64(512, Prime64, 0x617E49599013CB6BUL, 0x3CE457DE14C27708UL);
    }

    [Test]
    public void xxHash3_Hash_64_Length2048()
    {
        TestHash64(2048, Prime64, 0xDD59E2C3A5F038E0UL, 0x66F81670669ABABCUL);
    }

    [Test]
    public void xxHash3_Hash_64_Length2240()
    {
        TestHash64(2240, Prime64, 0x6E73A90539CF2948UL, 0x757BA8487D1B5247UL);
    }

    [Test]
    public void xxHash3_Hash_64_Length2243()
    {
        TestHash64(2367, Prime64, 0xCB37AEB9E5D361EDUL, 0xD2DB3415B942B42AUL);
    }

    [BurstCompile(CompileSynchronously = true)]
    struct xxHash3Hash128SanityCheckJob : IJob
    {
        [NativeDisableUnsafePtrRestriction]
        public unsafe void* SanityBuffer;
        [NativeDisableUnsafePtrRestriction]
        public unsafe void* DestinationBuffer;

        public long Length;
        public ulong Seed;

        public NativeArray<uint4> Result;

        public unsafe void Execute()
        {
            var resultIndex = 1;
            // Compute Hash from buffer
            Result[resultIndex++] = xxHash3.Hash128(SanityBuffer, Length, Seed);

            // Compute/Copy
            if (DestinationBuffer != null)
            {
                Result[resultIndex++] = xxHash3.Hash128(SanityBuffer, DestinationBuffer, Length, Seed);
            }

            // Streaming API Test
            {
                var state = new xxHash3.StreamingState(false, Seed);
                state.Update(SanityBuffer, (int)Length);
                Result[resultIndex++] = state.DigestHash128();
            }

            // 2 updates
            if (Length > 3)
            {
                var state = new xxHash3.StreamingState(false, Seed);
                {
                    state.Update(SanityBuffer, 3);
                    state.Update((byte*)SanityBuffer+3, (int)Length-3);
                    Result[resultIndex++] = state.DigestHash128();
                }
            }

            // byte per byte update
            if (Length > 0) {
                var state = new xxHash3.StreamingState(false, Seed);
                {
                    var bBuffer = (byte*) SanityBuffer;
                    for (int i = 0; i < Length; i++)
                    {
                        state.Update(bBuffer + i, 1);
                    }
                    Result[resultIndex++] = state.DigestHash128();
                }
            }

            Result[0] = new uint4(resultIndex - 1);
        }
    }

    private unsafe void TestHash128(long length, ulong seed, uint4 result, uint4 resultWithSeed)
    {
        var job = new xxHash3Hash128SanityCheckJob
        {
            SanityBuffer = SanityBuffer,
            DestinationBuffer = DestinationBuffer,
            Result = CollectionHelper.CreateNativeArray<uint4>(10, CommonRwdAllocator.Handle),
            Seed = 0,
            Length = length
        };

        job.Schedule().Complete();

        var resultCount = (int)job.Result[0].x;
        for (int i = 0; i < resultCount; i++)
        {
            Assert.That(job.Result[i+1], Is.EqualTo(result), $"Failed on entry {i}");
        }

        job.Seed = seed;
        job.Schedule().Complete();

        resultCount = (int)job.Result[0].x;
        for (int i = 0; i < resultCount; i++)
        {
            Assert.That(job.Result[i+1], Is.EqualTo(resultWithSeed), $"Failed on entry {i}");
        }

        job.Result.Dispose();
    }

    [Test]
    public unsafe void xxHash3_Hash_128_Length0000()
    {
        TestHash128(0, Prime,
            xxHash3.ToUint4(0x6001C324468D497FUL, 0x99AA06D3014798D8UL),
            xxHash3.ToUint4(0x5444F7869C671AB0UL, 0x92220AE55E14AB50UL));
    }

    [Test]
    public void xxHash3_Hash_128_Length0001()
    {
        TestHash128(1, Prime,
            xxHash3.ToUint4(0xC44BDFF4074EECDBUL, 0xA6CD5E9392000F6AUL),
            xxHash3.ToUint4(0xB53D5557E7F76F8DUL, 0x89B99554BA22467CUL));
    }

    [Test]
    public void xxHash3_Hash_128_Length0006()
    {
            // Length 6
            TestHash128(6, Prime,
                xxHash3.ToUint4(0x3E7039BDDA43CFC6UL, 0x082AFE0B8162D12AUL),
                xxHash3.ToUint4(0x269D8F70BE98856EUL, 0x5A865B5389ABD2B1UL));
    }

    [Test]
    public void xxHash3_Hash_128_Length0012()
    {
        // Length 12
        TestHash128(12, Prime,
            xxHash3.ToUint4(0x061A192713F69AD9UL, 0x6E3EFD8FC7802B18UL),
            xxHash3.ToUint4(0x9BE9F9A67F3C7DFBUL, 0xD7E09D518A3405D3UL));
    }

    [Test]
    public void xxHash3_Hash_128_Length0024()
    {
        // Length 24
        TestHash128(24, Prime,
            xxHash3.ToUint4(0x1E7044D28B1B901DUL, 0x0CE966E4678D3761UL),
            xxHash3.ToUint4(0xD7304C54EBAD40A9UL, 0x3162026714A6A243UL));
    }

    [Test]
    public void xxHash3_Hash_128_Length0048()
    {
        // Length 48
        TestHash128(48, Prime,
            xxHash3.ToUint4(0xF942219AED80F67BUL, 0xA002AC4E5478227EUL),
            xxHash3.ToUint4(0x7BA3C3E453A1934EUL, 0x163ADDE36C072295UL));
    }

    [Test]
    public void xxHash3_Hash_128_Length0081()
    {
        // Length 81
        TestHash128(81, Prime,
            xxHash3.ToUint4(0x5E8BAFB9F95FB803UL, 0x4952F58181AB0042UL),
            xxHash3.ToUint4(0x703FBB3D7A5F755CUL, 0x2724EC7ADC750FB6UL));
    }

    [Test]
    public void xxHash3_Hash_128_Length0222()
    {
        // Length 222
        TestHash128(222, Prime,
            xxHash3.ToUint4(0xF1AEBD597CEC6B3AUL, 0x337E09641B948717UL),
            xxHash3.ToUint4(0xAE995BB8AF917A8DUL, 0x91820016621E97F1UL));
    }

    [Test]
    public void xxHash3_Hash_128_Length0403()
    {
        // Length 403
        TestHash128(403, Prime64,
            xxHash3.ToUint4(0xCDEB804D65C6DEA4UL, 0x1B6DE21E332DD73DUL),
            xxHash3.ToUint4(0x6259F6ECFD6443FDUL, 0xBED311971E0BE8F2UL));
    }

    [Test]
    public void xxHash3_Hash_128_Length0512()
    {
        // Length 512
        TestHash128(512, Prime64,
            xxHash3.ToUint4(0x617E49599013CB6BUL, 0x18D2D110DCC9BCA1UL),
            xxHash3.ToUint4(0x3CE457DE14C27708UL, 0x925D06B8EC5B8040UL));
    }

    [Test]
    public void xxHash3_Hash_128_Length2048()
    {
        // Length 2048
        TestHash128(2048, Prime,
            xxHash3.ToUint4(0xDD59E2C3A5F038E0UL, 0xF736557FD47073A5UL),
            xxHash3.ToUint4(0x230D43F30206260BUL, 0x7FB03F7E7186C3EAUL));
    }

    [Test]
    public void xxHash3_Hash_128_Length2240()
    {
        // Length 2240
        TestHash128(2240, Prime,
            xxHash3.ToUint4(0x6E73A90539CF2948UL, 0xCCB134FBFA7CE49DUL),
            xxHash3.ToUint4(0xED385111126FBA6FUL, 0x50A1FE17B338995FUL));
    }

    [Test]
    public void xxHash3_Hash_128_Length2367()
    {
        // Length 2367
        TestHash128(2367, Prime,
            xxHash3.ToUint4(0xCB37AEB9E5D361EDUL, 0xE89C0F6FF369B427UL),
            xxHash3.ToUint4(0x6F5360AE69C2F406UL, 0xD23AAE4B76C31ECBUL));
    }

    // Expected uint4 values for xxHash3_128_vs_ReferenceXxHash128, captured once from
    // System.IO.Hashing.XxHash128 (.NET 8). Index 0 is the empty input; index i in 1..299
    // is the hash of the first i bytes of the 300-byte repeated digit string used by the
    // test.
    //
    // To regenerate the table below: drop the Program.cs and .csproj contents shown below
    // into a scratch folder, run `dotnet run`, paste the output into the array initializer.
    //
    //   // ----- Program.cs -----
    //   using System;
    //   using System.IO.Hashing;
    //   using System.Text;
    //
    //   static void PrintHash(byte[] data, int idx)
    //   {
    //       byte[] hash = XxHash128.Hash(data);
    //       Array.Reverse(hash);
    //       uint u0 = BitConverter.ToUInt32(hash, 0);
    //       uint u1 = BitConverter.ToUInt32(hash, 4);
    //       uint u2 = BitConverter.ToUInt32(hash, 8);
    //       uint u3 = BitConverter.ToUInt32(hash, 12);
    //       Console.WriteLine($"        new uint4(0x{u0:X8}u, 0x{u1:X8}u, 0x{u2:X8}u, 0x{u3:X8}u), // {idx}");
    //   }
    //
    //   byte[] longInput = Encoding.UTF8.GetBytes(""
    //       + "0123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789"
    //       + "0123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789"
    //       + "0123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789"
    //       );
    //
    //   PrintHash(Array.Empty<byte>(), 0);
    //   for (int i = 1; i < longInput.Length; i++)
    //   {
    //       byte[] slice = new byte[i];
    //       Array.Copy(longInput, 0, slice, 0, i);
    //       PrintHash(slice, i);
    //   }
    //
    //   // ----- xxhash128gen.csproj -----
    //   // <Project Sdk="Microsoft.NET.Sdk">
    //   //   <PropertyGroup>
    //   //     <OutputType>Exe</OutputType>
    //   //     <TargetFramework>net8.0</TargetFramework>
    //   //   </PropertyGroup>
    //   //   <ItemGroup>
    //   //     <PackageReference Include="System.IO.Hashing" Version="8.0.0" />
    //   //   </ItemGroup>
    //   // </Project>
    static readonly uint4[] s_ExpectedXxHash128Values = new uint4[]
    {
        new uint4(0x468D497Fu, 0x6001C324u, 0x014798D8u, 0x99AA06D3u), // 0
        new uint4(0xBB241055u, 0x1982E3A7u, 0x1282A400u, 0xCE8F1588u), // 1
        new uint4(0x2494D689u, 0xD39AF2A0u, 0x93477E82u, 0x1B73EDE3u), // 2
        new uint4(0x60965D90u, 0x8ED2B2F3u, 0x685CEBC1u, 0x93AEC821u), // 3
        new uint4(0x917B737Bu, 0x824B77D5u, 0x576B45EEu, 0xE7F00C8Du), // 4
        new uint4(0xEFF4DD2Eu, 0xBD3F18DBu, 0x5D404389u, 0x44F0AD48u), // 5
        new uint4(0x2AFF50D6u, 0x37392E79u, 0x70888CB0u, 0x97A37DF3u), // 6
        new uint4(0x0BC183C4u, 0x78CF37FDu, 0xF79608E2u, 0x6343E550u), // 7
        new uint4(0x46810217u, 0xF455A8E4u, 0x30E43A75u, 0x647579EAu), // 8
        new uint4(0xC9118966u, 0xE23A7F36u, 0x9E915EA8u, 0x7ED63115u), // 9
        new uint4(0x692165FBu, 0x49655FC9u, 0x19EC664Bu, 0xE3536676u), // 10
        new uint4(0x9DCBC8DDu, 0xA77CD8F0u, 0xA6D98239u, 0x39A7E489u), // 11
        new uint4(0x92F2B08Du, 0xE1A1E42Eu, 0xBB8A63D0u, 0x16F60AF8u), // 12
        new uint4(0x3CD0486Du, 0x9E010D7Bu, 0x60ACB82Du, 0x82B11C35u), // 13
        new uint4(0x22622A54u, 0x69C9E717u, 0x14D48F48u, 0xC1FA1422u), // 14
        new uint4(0x7937A63Au, 0xB7170640u, 0xD7D3E5D7u, 0xDEEABA05u), // 15
        new uint4(0xE115A4E3u, 0x7A9739F3u, 0x2ECE9E1Au, 0x7F1AC84Fu), // 16
        new uint4(0x95A67592u, 0xEBF11F11u, 0x846FC82Bu, 0x56FB0EDFu), // 17
        new uint4(0xBA53B286u, 0x0BA44552u, 0x4E4E2623u, 0x391E5105u), // 18
        new uint4(0x27E7D2FFu, 0x87939DE7u, 0x86BF68F8u, 0x1183F8FFu), // 19
        new uint4(0xF8B6F4C8u, 0xC50B1560u, 0x64D5A87Cu, 0x0651CCB9u), // 20
        new uint4(0x0FC832C8u, 0xB6FD1AD7u, 0x0D7916B4u, 0x0314EA74u), // 21
        new uint4(0x47C20ECFu, 0x7D97042Eu, 0x741B6BDDu, 0x36F4DEF1u), // 22
        new uint4(0x311367EAu, 0x5D92CBBBu, 0x6D014C73u, 0xC6346E38u), // 23
        new uint4(0xBBA53F89u, 0x3C94CFB6u, 0xB066B7E8u, 0x445BA253u), // 24
        new uint4(0x565E6CBDu, 0x54FD8E1Eu, 0x9DAC3274u, 0xC29ED0D9u), // 25
        new uint4(0xC1F9264Au, 0x640BFC42u, 0x48176C71u, 0xA3E6125Bu), // 26
        new uint4(0x51ED94ACu, 0x4B2C0A83u, 0x2709AF0Eu, 0xBEA01026u), // 27
        new uint4(0xFEDA3A3Au, 0x475A259Cu, 0x378AE536u, 0xBBED2856u), // 28
        new uint4(0x3A89959Du, 0xAA669371u, 0x11FA29C4u, 0xB83C9CBCu), // 29
        new uint4(0x0FC41635u, 0xE6E225C0u, 0x542F09F4u, 0xED4243D6u), // 30
        new uint4(0x308AF16Bu, 0xD5F18E7Du, 0x9703B40Cu, 0x178EDFC4u), // 31
        new uint4(0xCA8BD665u, 0x5D426A07u, 0xFCD763FEu, 0x7C3D550Du), // 32
        new uint4(0x4AD30F76u, 0x9957DBFEu, 0xAF73AB7Au, 0x9D661750u), // 33
        new uint4(0xB64AC512u, 0x1C0B0565u, 0x7E76ABCDu, 0x147778E0u), // 34
        new uint4(0xC4C53D92u, 0xFAE4264Fu, 0xBDC8C67Fu, 0xC3E71C08u), // 35
        new uint4(0x684C02ACu, 0x0E2F3494u, 0x687FB3E9u, 0x99BD8DCBu), // 36
        new uint4(0x1BC717A1u, 0x603DDA4Au, 0x8FCE22B4u, 0xD3DF54B8u), // 37
        new uint4(0x0DEE9247u, 0x69D880B6u, 0xB459B340u, 0x7D3FCFECu), // 38
        new uint4(0x6ADAEDB6u, 0xDA7485AEu, 0xB8864D13u, 0x93AF7F8Fu), // 39
        new uint4(0x4E0FD88Fu, 0x809DC74Cu, 0xB147A56Au, 0xDE643CA7u), // 40
        new uint4(0x8926F760u, 0xC52848C4u, 0xB2650E3Au, 0x6B4F4290u), // 41
        new uint4(0x63B79D85u, 0xD8A7CBAEu, 0x3427D26Eu, 0xB9C5E371u), // 42
        new uint4(0xBE325684u, 0x3F6547D9u, 0x3789783Au, 0xC7A919F0u), // 43
        new uint4(0xBEEBD554u, 0xC402F8B5u, 0xF45DFDC6u, 0x4677C4CFu), // 44
        new uint4(0x0A90BDFAu, 0x68A00C75u, 0x547135CDu, 0x29D9B011u), // 45
        new uint4(0x367C2FC6u, 0x10BC65B7u, 0xF019C4FAu, 0xCAC940C5u), // 46
        new uint4(0xDB95D95Bu, 0x2C72FBC0u, 0x07958656u, 0xAA1E0A9Cu), // 47
        new uint4(0x8754D64Au, 0xCAB7094Cu, 0x7AF3A87Fu, 0x5637E7F8u), // 48
        new uint4(0x9FB65FD5u, 0x53E7BF9Eu, 0x4D7BF23Cu, 0x8E5C6217u), // 49
        new uint4(0xDFC2CCA3u, 0xB0F9796Du, 0xB66B3666u, 0x9ADBF6D5u), // 50
        new uint4(0x327DBD5Fu, 0x6B9579DEu, 0xC7815FFEu, 0x335D1D32u), // 51
        new uint4(0x82C24133u, 0x44796C35u, 0xCBB37718u, 0xA900E1B3u), // 52
        new uint4(0x6821DA2Fu, 0xB126DBF1u, 0x963CA39Fu, 0x8EF6D01Au), // 53
        new uint4(0x5C3D019Au, 0x85BA3DC9u, 0xD522BD6Du, 0x3C6104B2u), // 54
        new uint4(0x03F09462u, 0x6ABF5740u, 0xF5353BB7u, 0x37E2D762u), // 55
        new uint4(0x2383FD84u, 0xA8760C73u, 0x9E9BB756u, 0xF4068100u), // 56
        new uint4(0x15097C01u, 0x86B32A45u, 0xE0DBCB73u, 0xA5181DFFu), // 57
        new uint4(0x2A84AF66u, 0x0D46DD3Au, 0xE209BC9Cu, 0xFDF7A7F9u), // 58
        new uint4(0xB6A08C63u, 0x9B0485C5u, 0x5ADE2364u, 0x73BCAE6Fu), // 59
        new uint4(0xE7921758u, 0xB6537990u, 0x92F5A4F9u, 0x07638AF8u), // 60
        new uint4(0x186FB9C1u, 0xC72FF760u, 0xB9BEC564u, 0x1EA5B03Eu), // 61
        new uint4(0x214C95FEu, 0x76480646u, 0xA74B0A39u, 0xF0C29716u), // 62
        new uint4(0x29D5CC29u, 0x02C51CD1u, 0x739B4A3Bu, 0x3D4F20ABu), // 63
        new uint4(0x126658F0u, 0xC951B624u, 0xC2C4F973u, 0xD46EF10Bu), // 64
        new uint4(0x3F25B88Fu, 0x3270E3ADu, 0x37795109u, 0x3CCF0702u), // 65
        new uint4(0x9C107CA8u, 0x100F8861u, 0x796D2DADu, 0x7FBB8881u), // 66
        new uint4(0x0E89C938u, 0xCA052FC4u, 0x8D334FBCu, 0x27187F53u), // 67
        new uint4(0x2B9E2BDEu, 0x0F04DC45u, 0x196EA148u, 0x3CD6A1D6u), // 68
        new uint4(0x6AE6912Fu, 0xAC237786u, 0xBCAF2741u, 0x48DD350Eu), // 69
        new uint4(0x3FF2411Cu, 0xE931005Bu, 0x0012EE0Du, 0x37CD2541u), // 70
        new uint4(0xF56FC0D2u, 0xEE37D9AEu, 0x4A346FA7u, 0xC877F058u), // 71
        new uint4(0x2ADEE20Bu, 0x2811BA33u, 0xCEB5BC35u, 0xA5686FE2u), // 72
        new uint4(0x278359A9u, 0xA874F939u, 0x73560CD3u, 0xE0B4CF3Bu), // 73
        new uint4(0x43F67BA3u, 0x748813A6u, 0xC66104A9u, 0x402024F9u), // 74
        new uint4(0xA0ADE15Cu, 0x3725F966u, 0x19DA8A7Fu, 0x85929976u), // 75
        new uint4(0x078EFC61u, 0x096779DFu, 0xB01B8ADAu, 0x7A3EA76Au), // 76
        new uint4(0xFB794895u, 0x975C0DA0u, 0x4623A6F1u, 0x22D2A945u), // 77
        new uint4(0x21DD999Au, 0x9CBFB78Cu, 0xE4A7F632u, 0x413570CBu), // 78
        new uint4(0x57B619A5u, 0x478C9F1Du, 0xA3D63276u, 0x6C37B2CCu), // 79
        new uint4(0x60F4FB4Eu, 0x4CF3509Cu, 0xC001DE4Du, 0xD2AB2AAFu), // 80
        new uint4(0xC0DD3CE9u, 0x83BDDB0Fu, 0x84400485u, 0x8D76D6AEu), // 81
        new uint4(0x5CE8CB58u, 0x6FD35F39u, 0xDF4B249Au, 0xDBA07E47u), // 82
        new uint4(0x6E453EC6u, 0x7019789Bu, 0xEC10C73Eu, 0x70AB54D7u), // 83
        new uint4(0x2312DFCDu, 0x36D40574u, 0x6437983Eu, 0x3C548CD7u), // 84
        new uint4(0xD1718CA3u, 0xC9281A3Eu, 0x851325BAu, 0x6DA488F5u), // 85
        new uint4(0xB3D72972u, 0xABC2DC00u, 0xD1041A64u, 0x13030B20u), // 86
        new uint4(0x34992ADEu, 0xA81D5E54u, 0x71565903u, 0xC56464C0u), // 87
        new uint4(0xFA4616B8u, 0x783E9226u, 0x35D97DCEu, 0xDDEE8E75u), // 88
        new uint4(0x9D8FE853u, 0x7324F1D9u, 0xBAE557E3u, 0x087D231Fu), // 89
        new uint4(0xA5691DA7u, 0x03F206A4u, 0xBAC6AD65u, 0x25C1A790u), // 90
        new uint4(0x38138BCFu, 0xDEC8B440u, 0xD121CE1Au, 0x8C35EBD3u), // 91
        new uint4(0x7163A6E0u, 0x26A583A2u, 0xEABD226Au, 0x73B27E75u), // 92
        new uint4(0x4BDE4688u, 0xC20F219Cu, 0xFF16C80Cu, 0x06151808u), // 93
        new uint4(0xA5F1B03Au, 0x74178D08u, 0xE2A90BA4u, 0x4B2B5BC0u), // 94
        new uint4(0xDC658B96u, 0x6426AA90u, 0x564B8D36u, 0x847D79D8u), // 95
        new uint4(0x69B1E399u, 0x06B068AAu, 0xF526F131u, 0x33ED13A6u), // 96
        new uint4(0x61012D7Fu, 0x76A2184Eu, 0x452BAB29u, 0x169BD58Au), // 97
        new uint4(0x094668A4u, 0x386E1584u, 0x5EE6FCF3u, 0x73D64869u), // 98
        new uint4(0x5E9C5E55u, 0xE64A344Bu, 0x0C8C01F5u, 0x2B1BCBA2u), // 99
        new uint4(0xE3DE8851u, 0x30108F5Du, 0x41F016F6u, 0xCF6EF08Fu), // 100
        new uint4(0x708C8787u, 0x8B9C1B78u, 0x001B04E1u, 0xB4C04AADu), // 101
        new uint4(0xB7A97612u, 0xB87F8EE4u, 0x81D6C64Au, 0x76D97ACFu), // 102
        new uint4(0xEE88CB75u, 0x440F3C17u, 0xE0F097D8u, 0xC48EFE88u), // 103
        new uint4(0x7F6F4E96u, 0x5946A044u, 0x77F5ABA6u, 0xF98D6034u), // 104
        new uint4(0xC9DB94E6u, 0x3EAD6FB4u, 0x14499485u, 0xC3EAAEB7u), // 105
        new uint4(0x29CE4778u, 0xC810A38Fu, 0x956DCBBBu, 0x1EC4237Au), // 106
        new uint4(0x2DDEF2BBu, 0x4D9D0DA6u, 0xA7915A94u, 0x8AD34BB4u), // 107
        new uint4(0xB455715Cu, 0xAB168C91u, 0x6F4322BBu, 0xA6A8532Eu), // 108
        new uint4(0xE0C8FA07u, 0xA96A6242u, 0xC02FE9C8u, 0xCB850E6Cu), // 109
        new uint4(0x8AD2C9D8u, 0xAFF3B633u, 0x404C7ED5u, 0x1A5782DEu), // 110
        new uint4(0x13826B30u, 0x3B5B22A2u, 0x61A7BF84u, 0x9C06BB5Du), // 111
        new uint4(0x2C0CAA9Bu, 0x11374EFDu, 0x6ED7A67Bu, 0x77C5BFB5u), // 112
        new uint4(0x1508A85Cu, 0x36578F93u, 0xFC757B33u, 0x8F81F8CDu), // 113
        new uint4(0x601E3459u, 0x2370D4EAu, 0xAFC56629u, 0x27602737u), // 114
        new uint4(0x2348CC4Eu, 0xA2B14676u, 0x13A8A1B7u, 0x0E28B638u), // 115
        new uint4(0x0C1AB3B6u, 0xD371A311u, 0xBB2179E3u, 0x2B6AF840u), // 116
        new uint4(0x8B7E0949u, 0x96C52FB8u, 0x96D567A3u, 0x2B22157Eu), // 117
        new uint4(0x9396A563u, 0xED858403u, 0x3B75ECDFu, 0x2B8AC1AAu), // 118
        new uint4(0x60D9EE44u, 0xD525A8B3u, 0xCD7E714Du, 0x8EEF15E4u), // 119
        new uint4(0x4D172A85u, 0x54CC069Cu, 0xA6ADDD0Bu, 0x803B11ACu), // 120
        new uint4(0x3249D0D5u, 0x8F32D6D1u, 0x40D1F1F7u, 0x15990CC5u), // 121
        new uint4(0x982E4087u, 0x307EA912u, 0xB0E659B2u, 0x6558515Eu), // 122
        new uint4(0x9319466Cu, 0xCE1E7ED7u, 0x20894DF2u, 0x07A465BEu), // 123
        new uint4(0xDF1BD519u, 0x7FB0E62Bu, 0x16F39F0Au, 0xE0B9C862u), // 124
        new uint4(0x2DD0658Eu, 0x3E954200u, 0xE3CF8A61u, 0xDCA2E75Du), // 125
        new uint4(0xB5601E49u, 0x0E195D8Bu, 0x8D7B6095u, 0x84911E59u), // 126
        new uint4(0x90FB9000u, 0x19081859u, 0x5322A284u, 0x2C803A9Eu), // 127
        new uint4(0x0F9F5AE4u, 0x28A6278Bu, 0x3967EEBAu, 0x675EA4C1u), // 128
        new uint4(0xAC9F53BEu, 0x6F071552u, 0xE35B1601u, 0xFB6029C8u), // 129
        new uint4(0xF995633Au, 0x2B3CDBAFu, 0x24DFAD80u, 0xFB07B7A5u), // 130
        new uint4(0x3E3C4E72u, 0xF3F750C9u, 0x8D038F48u, 0xADA608F0u), // 131
        new uint4(0xD88CF214u, 0x4A9ADE12u, 0x8C564307u, 0x2C167E36u), // 132
        new uint4(0xA6207468u, 0xEABACE01u, 0x5BD53193u, 0x25DB461Eu), // 133
        new uint4(0x65FD936Au, 0xBB49948Bu, 0xAFD2AEC2u, 0x2B9F1D40u), // 134
        new uint4(0xDC1A754Fu, 0x98E93D12u, 0x89582AA1u, 0x149EE308u), // 135
        new uint4(0x11B30C7Bu, 0x983E15ABu, 0x3D3B89C9u, 0xD0693411u), // 136
        new uint4(0x44815B74u, 0x053DAA7Fu, 0x2CD11EFDu, 0xE1D59C18u), // 137
        new uint4(0x105C893Eu, 0x41A77E3Eu, 0xB81DC1C7u, 0x53525121u), // 138
        new uint4(0x5DEFD8C9u, 0x13D1E567u, 0x15EBFD04u, 0x43F932D9u), // 139
        new uint4(0x97B7E46Fu, 0x03BC6DBEu, 0xEF7CCB04u, 0x4FE47150u), // 140
        new uint4(0xF6EBF3B5u, 0x2F8FB71Au, 0x296E17BDu, 0xD959633Au), // 141
        new uint4(0x41CF14AAu, 0x3309B0A5u, 0x3A5B3E85u, 0x8E462A91u), // 142
        new uint4(0x206673D9u, 0x69A9E8E1u, 0x5A0591C7u, 0x54259DC7u), // 143
        new uint4(0x377D189Au, 0x3C88032Eu, 0xD5ED9178u, 0x1E29AB00u), // 144
        new uint4(0x3488DE84u, 0x1FC8CA35u, 0xF6A383E8u, 0x60C2F59Fu), // 145
        new uint4(0xAA6CBBB2u, 0x0D59AB60u, 0x3AF22A17u, 0xA1F87DCEu), // 146
        new uint4(0xB49A35DBu, 0x2393F80Du, 0xAFC1EB64u, 0xD8A960F0u), // 147
        new uint4(0xCD029125u, 0x638DA92Au, 0xD83ACAF9u, 0xCB468073u), // 148
        new uint4(0x39E53691u, 0xF7BCC79Du, 0xEEA6F90Eu, 0xEB938C9Fu), // 149
        new uint4(0x76B16D77u, 0xF6BF762Du, 0xCD07A9BCu, 0xC029574Au), // 150
        new uint4(0xB1E3C6E8u, 0xC2C95B5Du, 0xF01ED6FBu, 0xF4332264u), // 151
        new uint4(0x8701807Au, 0xB59C3993u, 0x508B4138u, 0x6A07F468u), // 152
        new uint4(0x7E583993u, 0x7789F181u, 0xD597CFF4u, 0xD80D28AAu), // 153
        new uint4(0xA0C82B46u, 0x7F32E5E0u, 0x8EA8B82Fu, 0x39DC3C0Fu), // 154
        new uint4(0xB4914DF5u, 0x8EF09BF3u, 0xC7E9BD2Fu, 0xBE480347u), // 155
        new uint4(0x8E7D24B3u, 0x35574230u, 0xC1C644C6u, 0xA265E466u), // 156
        new uint4(0x5807D585u, 0x169EDC39u, 0x441B5EDAu, 0x6FD0EEC9u), // 157
        new uint4(0x87388A72u, 0xB61F8788u, 0xEBF5A23Au, 0x4E09E98Cu), // 158
        new uint4(0x5E0EAE55u, 0x75AAB1FDu, 0x3B5C2D3Cu, 0xE4D8FB50u), // 159
        new uint4(0x3D52FE07u, 0x4BA1949Du, 0x41E7A237u, 0xB2A90F3Cu), // 160
        new uint4(0x169C4048u, 0xEE4FFC73u, 0xD7362F71u, 0xAE458739u), // 161
        new uint4(0x9CEDE352u, 0x1D6E951Fu, 0x21B535E2u, 0x94357B7Fu), // 162
        new uint4(0x768CAA4Du, 0x4CD12563u, 0x9377CECBu, 0xC3FD1339u), // 163
        new uint4(0x513C0534u, 0xC3D95657u, 0x74E28AC0u, 0x14FACCECu), // 164
        new uint4(0x2DC20865u, 0x38BDE8B5u, 0x043F80F3u, 0x323EA957u), // 165
        new uint4(0xB4E96765u, 0x7747E052u, 0x56146442u, 0xE6A02E02u), // 166
        new uint4(0x0E1047E6u, 0x35B1AA36u, 0xCA41CA6Au, 0xA42E5C06u), // 167
        new uint4(0x3CE821F8u, 0xA329775Au, 0x3BF073A7u, 0xFB09CBEEu), // 168
        new uint4(0x0DE11557u, 0x95934529u, 0x7F9107AFu, 0x040E6545u), // 169
        new uint4(0x9B9984BEu, 0xB7891954u, 0xF56F526Au, 0x2686D787u), // 170
        new uint4(0x061E32D2u, 0x942CB092u, 0xCE8E302Au, 0xD0133677u), // 171
        new uint4(0x8EFF9849u, 0xD7E5A3D4u, 0x5EE2264Du, 0x4E155C0Eu), // 172
        new uint4(0x05C16117u, 0x64B84FD9u, 0xF35281DBu, 0xD6FD92AFu), // 173
        new uint4(0x2D137BABu, 0x73A59B61u, 0x18BB628Cu, 0x12E51B61u), // 174
        new uint4(0xB9FC474Bu, 0xD88838F7u, 0x76E34446u, 0xA343E0CBu), // 175
        new uint4(0x26918A32u, 0x1D1C2AECu, 0x540E7DD3u, 0x1A414437u), // 176
        new uint4(0xB56CB591u, 0xF35DBFBEu, 0x2A9BECBDu, 0x3CD7C1C5u), // 177
        new uint4(0xA8E48BE9u, 0xF57DA8BAu, 0x3732E12Cu, 0xF792ED4Cu), // 178
        new uint4(0x6779F9E2u, 0x0855F300u, 0xBE72AC86u, 0x0A1621E1u), // 179
        new uint4(0x28D139B0u, 0x8834C183u, 0xE1620450u, 0x228D91E2u), // 180
        new uint4(0x43059422u, 0x9A74292Bu, 0x235C6B73u, 0x61AFD4B6u), // 181
        new uint4(0xEA772DA6u, 0xA2014087u, 0xD167FB8Du, 0x4DBA9493u), // 182
        new uint4(0x78CC0B68u, 0x981FD391u, 0xA4BF7CB3u, 0xABEEAD32u), // 183
        new uint4(0x6EAA0556u, 0xE31D9DD7u, 0xAD5C9F76u, 0x842DA644u), // 184
        new uint4(0x6FCA6D26u, 0xF0A484BDu, 0x4D433CFDu, 0x5B3C5FA0u), // 185
        new uint4(0x8F0BA049u, 0x9D7F1BD2u, 0x64AC8AB9u, 0x812ADEFEu), // 186
        new uint4(0x1CEB21E5u, 0xC2064BD7u, 0xB4BB9687u, 0x3D1420B2u), // 187
        new uint4(0x78955C73u, 0x4B48FB97u, 0xC2C355BCu, 0xB6CE9B12u), // 188
        new uint4(0xADFE00D8u, 0x9D8264A6u, 0xA9FBE467u, 0x8B186833u), // 189
        new uint4(0x1977460Fu, 0x53A3A69Bu, 0xCE07773Bu, 0x01C9D0BAu), // 190
        new uint4(0xED514D7Fu, 0x03FC4666u, 0x3F3DB171u, 0x14EAFD67u), // 191
        new uint4(0x12AF662Cu, 0xACE0E612u, 0x342CA984u, 0x6A153842u), // 192
        new uint4(0x5CE58734u, 0x86EE18ADu, 0xDA06A456u, 0x47B838F6u), // 193
        new uint4(0x619525A2u, 0xB57F57C8u, 0x7B341EE7u, 0x4177696Au), // 194
        new uint4(0xF856E2BCu, 0xCEFF16AFu, 0x232D2EB0u, 0x497A64F3u), // 195
        new uint4(0xDE61DC05u, 0x27423555u, 0x372B9117u, 0x423315BDu), // 196
        new uint4(0xF6613B81u, 0x8333440Au, 0xBCBF9CB5u, 0xFED834CCu), // 197
        new uint4(0x4C705476u, 0x16F94AB1u, 0xBC3DB56Du, 0xD659CADAu), // 198
        new uint4(0x9A1FEB12u, 0xCCDE6EC0u, 0x4EA11242u, 0xBABDDF8Bu), // 199
        new uint4(0xF430EC87u, 0x7A608AEAu, 0x88A9FB3Au, 0xE3BEF1F9u), // 200
        new uint4(0x634303D6u, 0x868E38E4u, 0x0C6260E4u, 0x698401EFu), // 201
        new uint4(0x33377A93u, 0x57099DCEu, 0x5223EA8Bu, 0x52A7C90Du), // 202
        new uint4(0x351D2980u, 0xA3FEDDB4u, 0x22CC0979u, 0x4B282929u), // 203
        new uint4(0xDCC7BEE3u, 0xF1CFFE12u, 0x13BA0879u, 0xF341F3B0u), // 204
        new uint4(0x62955F1Eu, 0xCF6D5A84u, 0x1C23CC04u, 0x8343C659u), // 205
        new uint4(0x36D5FB0Du, 0x0944BB29u, 0x48107CCAu, 0x74AE6E7Du), // 206
        new uint4(0x2D6C5ECBu, 0x2774F9C4u, 0x3E67906Fu, 0x356DD8B4u), // 207
        new uint4(0x0ADF5203u, 0xB2462AD5u, 0x078361B5u, 0x982BD9A5u), // 208
        new uint4(0x6F957A68u, 0x6CC1B53Cu, 0x43292B5Bu, 0xED20E971u), // 209
        new uint4(0x8D078DABu, 0x257D001Au, 0xB6B94AD3u, 0xD3B8CC96u), // 210
        new uint4(0x9515873Fu, 0xC2E45229u, 0x1D1C828Au, 0xE8D0D979u), // 211
        new uint4(0x25097514u, 0x4E4D66F2u, 0x797397D6u, 0x147BB340u), // 212
        new uint4(0xE01602CAu, 0x05A0E47Au, 0x69056359u, 0x46D58EC7u), // 213
        new uint4(0x71F0A8CAu, 0x742F8C8Eu, 0x67345D0Bu, 0x32CE5E3Fu), // 214
        new uint4(0x1E00B3FDu, 0xF7EF2541u, 0x79CD02B4u, 0xD26A19D8u), // 215
        new uint4(0x642214BCu, 0x4F564EFEu, 0xB7C4A5C2u, 0x481306B9u), // 216
        new uint4(0xD4585992u, 0x6C19DBCEu, 0xB347D658u, 0xE5BCF35Bu), // 217
        new uint4(0x0A5A55E5u, 0xF0CB81DDu, 0x7D83F586u, 0xC8C7BFFCu), // 218
        new uint4(0x1A46434Cu, 0x2A5836A5u, 0x56E4F32Bu, 0x932B2695u), // 219
        new uint4(0x97AFA0A0u, 0x4C63A738u, 0x8259F190u, 0x259F8441u), // 220
        new uint4(0xD486E159u, 0xE0C4F294u, 0x5093A4D5u, 0x3B94ED6Cu), // 221
        new uint4(0x4AC8F907u, 0x3386BEC6u, 0x3D7A3BA9u, 0x86EBED05u), // 222
        new uint4(0xC3BFC722u, 0xEA538D86u, 0xFA050E10u, 0xFAF1C533u), // 223
        new uint4(0x94BB5A27u, 0x191B8403u, 0xCB9E73DDu, 0x60E0B707u), // 224
        new uint4(0x433083AEu, 0xA490A3CFu, 0xE2337376u, 0x4C6D0E6Bu), // 225
        new uint4(0x4DBF74C9u, 0xC4AB92EAu, 0xD7D25CEDu, 0x55868761u), // 226
        new uint4(0x2CAF45E4u, 0x646C453Bu, 0x578F7F6Eu, 0x0938977Bu), // 227
        new uint4(0x5290FEE7u, 0x1F7459B2u, 0xB4FBC43Fu, 0x48F9ACEDu), // 228
        new uint4(0xFD6F24DDu, 0xB81AD3D0u, 0xAA49DC8Fu, 0xD565922Bu), // 229
        new uint4(0xFCED04BFu, 0x7743BF3Du, 0x442F4A98u, 0x76CCB270u), // 230
        new uint4(0xCFC1D8F0u, 0x0A70308Bu, 0x4714E3E8u, 0x76DD8C26u), // 231
        new uint4(0x2B720F0Cu, 0x573EDFB3u, 0x508FC637u, 0xAC308F01u), // 232
        new uint4(0x5E0D90E4u, 0x6F8FB05Au, 0x8CFAB48Du, 0x3C0E5A0Eu), // 233
        new uint4(0x83EEE90Du, 0x080DB944u, 0x6F8A152Au, 0x8A235B42u), // 234
        new uint4(0x89ADCDD5u, 0x186FC7D0u, 0x26F8B503u, 0x600DC7A7u), // 235
        new uint4(0x65210F19u, 0x85DF223Eu, 0x30BF837Fu, 0x583C124Eu), // 236
        new uint4(0xCA145F6Eu, 0x9F030A63u, 0x6E7C84D9u, 0x3FC5F701u), // 237
        new uint4(0x8D6AC3E6u, 0xE01747DCu, 0x14C6159Au, 0x681128E7u), // 238
        new uint4(0x8984C936u, 0x73B037D1u, 0x4A1B379Bu, 0x9CB3A409u), // 239
        new uint4(0xDABE4173u, 0x50795544u, 0xE12BA663u, 0x5AF2F177u), // 240
        new uint4(0x7B4B2EF6u, 0x79EB84F1u, 0x8CA62E48u, 0x46DE0FC6u), // 241
        new uint4(0x7968C5EAu, 0xC1BE4D18u, 0x30E562A9u, 0x227A1C60u), // 242
        new uint4(0xB3B8998Eu, 0x0EB2F662u, 0x50D776E1u, 0x8DC5B336u), // 243
        new uint4(0x41F19BCEu, 0x70788E5Cu, 0x5E82BD83u, 0x9A2B7458u), // 244
        new uint4(0xF49B20F0u, 0x40E6FB89u, 0x3E13A60Du, 0x41324EF0u), // 245
        new uint4(0x273DB80Bu, 0x31EA8511u, 0xC961A0F9u, 0xF5F1FB7Au), // 246
        new uint4(0xBFBD19EBu, 0xF6A1A06Du, 0x4FEE7BDBu, 0x8FB5F781u), // 247
        new uint4(0xA8F9061Du, 0x0F2E5B80u, 0x90999AD3u, 0x547893E8u), // 248
        new uint4(0xB0464472u, 0x43E64F94u, 0x5C12F9C8u, 0x9914DF8Du), // 249
        new uint4(0xD63AA2E0u, 0x0BEFF664u, 0xF8328B4Bu, 0xE6DC20D4u), // 250
        new uint4(0xA1C6ABBBu, 0x276E5F4Au, 0xC56D6651u, 0x27ED61C1u), // 251
        new uint4(0xB2AE3083u, 0x081D9B6Fu, 0xCDC661C9u, 0x25BA8B15u), // 252
        new uint4(0x150B742Du, 0x510874FDu, 0x40ED0949u, 0x1F2FB9DCu), // 253
        new uint4(0x0FB0B1AFu, 0x19E636F1u, 0x9F3FC142u, 0xB7565AD4u), // 254
        new uint4(0x63567797u, 0xA12C134Bu, 0x26FE314Eu, 0x1EC1BA9Du), // 255
        new uint4(0x3E4940C1u, 0x12671013u, 0x0070B37Du, 0x393958CCu), // 256
        new uint4(0xC1937BB8u, 0x2AE33BEDu, 0x23E65E63u, 0x6DF32F49u), // 257
        new uint4(0xE0E492FBu, 0x0D472198u, 0x883903F0u, 0xA01EAC86u), // 258
        new uint4(0x5811DD03u, 0x77D7D7C9u, 0x75B72A9Eu, 0x055A31E6u), // 259
        new uint4(0x7FF52D16u, 0x2A88452Du, 0xFD0EEE16u, 0xEB6B5758u), // 260
        new uint4(0x2C88170Au, 0x9F5A37ABu, 0xBA698BA9u, 0x2ED9FBF0u), // 261
        new uint4(0x6D3E3C42u, 0x16A6B6F8u, 0x428A6498u, 0xE2DCF3AFu), // 262
        new uint4(0x911FF198u, 0x7AD8AF84u, 0x77929225u, 0xF6F38A7Du), // 263
        new uint4(0xC3F0A085u, 0x0FF4E9EBu, 0x0CEDB0C2u, 0x0E3C15DFu), // 264
        new uint4(0x17001FC8u, 0x8BD621C9u, 0x0C0B34FAu, 0x08CC94F2u), // 265
        new uint4(0x9F3052EEu, 0x7360E428u, 0x76536499u, 0x679A6130u), // 266
        new uint4(0x8ABC8F81u, 0xD9725B3Du, 0x0ED86C1Eu, 0x4974A33Cu), // 267
        new uint4(0x69FF4B2Eu, 0xA52CC8A8u, 0x25018BFBu, 0x88961C0Du), // 268
        new uint4(0xD2BA15ECu, 0x38BD25C9u, 0xE62F8DB7u, 0xB9A52F49u), // 269
        new uint4(0x5CE1DDF1u, 0xF4732C05u, 0xCF988313u, 0x2C90AFAEu), // 270
        new uint4(0xADC1A2ACu, 0xE7634612u, 0x7D64ADF4u, 0xB3359C5Du), // 271
        new uint4(0xEACEB627u, 0x32750587u, 0x4F97628Du, 0x66B7E611u), // 272
        new uint4(0x1DD33694u, 0xE2B0E6A4u, 0xD6993AF8u, 0x7AE2DE81u), // 273
        new uint4(0x856D02AAu, 0x12A2878Eu, 0xFBFB3AC8u, 0xFD81B58Cu), // 274
        new uint4(0x8C442998u, 0x363CF140u, 0x9562134Du, 0xAB39B0C8u), // 275
        new uint4(0xAF60C47Bu, 0xA2867298u, 0x3F8FB6F9u, 0x3B0F63D5u), // 276
        new uint4(0x16DF584Bu, 0xC8236CAEu, 0x00DC3C61u, 0xA550A182u), // 277
        new uint4(0xB6A638B0u, 0xE3026798u, 0x786BC79Eu, 0x58197539u), // 278
        new uint4(0x30D53485u, 0x68665CF9u, 0x4CA6CDAFu, 0xA4B88D20u), // 279
        new uint4(0x7DEAC81Au, 0x0597B7D8u, 0x9FF9B8C8u, 0xA2799D70u), // 280
        new uint4(0x404DBCD7u, 0x4A71B88Cu, 0xD111D167u, 0x92337D65u), // 281
        new uint4(0x27EBD6CBu, 0x049E9ED9u, 0x3680D133u, 0x7B8BAAF8u), // 282
        new uint4(0x65DCDB1Bu, 0x67ED8552u, 0x080202EDu, 0xA2635821u), // 283
        new uint4(0x4702C663u, 0x52C87191u, 0x427CD70Bu, 0xF0780FB2u), // 284
        new uint4(0x6BE168A0u, 0x2650D6DAu, 0x1583DF75u, 0x5C80E11Eu), // 285
        new uint4(0xEA325C50u, 0x62B08D35u, 0x8DC45B38u, 0x52F5BC67u), // 286
        new uint4(0xDFDE5C32u, 0x07FC1051u, 0xC21C01C1u, 0x0448D5B9u), // 287
        new uint4(0x8E8EEEE5u, 0x3F0249F4u, 0x9A572EEEu, 0xE3DEA1DAu), // 288
        new uint4(0xB67DFBB6u, 0xD65E6034u, 0x6C5727B5u, 0x816BB5EDu), // 289
        new uint4(0x8316ADF2u, 0x54580C5Bu, 0xA049E3BFu, 0xD251582Cu), // 290
        new uint4(0x1F8AA6CEu, 0xFB96CBC3u, 0x67522865u, 0xBFBC89E6u), // 291
        new uint4(0xFF79E11Eu, 0xAF38701Bu, 0x9BB9F5FFu, 0xDE3EE5CDu), // 292
        new uint4(0x236D3C93u, 0x31E934B4u, 0x078FB568u, 0x05EA880Bu), // 293
        new uint4(0x271D1D26u, 0x5EF5219Fu, 0xEC39CE02u, 0x69EE0210u), // 294
        new uint4(0x9F66F2F3u, 0xC964C03Eu, 0xE4E0173Au, 0x9AB8361Au), // 295
        new uint4(0x33C073CAu, 0x265B40E4u, 0x77F72407u, 0xC40245E9u), // 296
        new uint4(0x751289C1u, 0xF179BC49u, 0x5C2A54CEu, 0xEF58A98Bu), // 297
        new uint4(0xEDAA1448u, 0xA64233FFu, 0x8747913Bu, 0x87428AD2u), // 298
        new uint4(0x23AFE979u, 0x5B4EA2D0u, 0x605754FDu, 0xDE906410u), // 299
    };

    [Test]
    public unsafe void xxHash3_128_vs_ReferenceXxHash128()
    {
        Assert.AreEqual(s_ExpectedXxHash128Values[0], xxHash3.Hash128(null, 0));

        byte[] input = Encoding.UTF8.GetBytes(""
            + "0123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789"
            + "0123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789"
            + "0123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789"
            );
        Assert.True(input.Length > 240 /* MIDSIZE_MAX */);
        Assert.AreEqual(s_ExpectedXxHash128Values.Length, input.Length);

        fixed (byte* data = input)
        {
            for (var i = 1; i < input.Length; i++)
            {
                Assert.AreEqual(s_ExpectedXxHash128Values[i], xxHash3.Hash128(data, i), $"length {i}");
            }
        }
    }

    [Test]
    public unsafe void xxHash3_Hash128_Alignment()
    {
        // arm 32 bit has strict alignment requirements so we must be sure that we can
        // hash at any alignment on that platform.
        const int kBufferBytes = 1024 * 1024; // 1 MB
        var buffer = new NativeArray<byte>(kBufferBytes, Allocator.Temp);

        for (int i = 0; i < buffer.Length; ++i)
        {
            buffer[i] = (byte)i;
        }

        for (int offset = 0; offset < 64; ++offset)
        {
            var hash = xxHash3.Hash128((byte*)buffer.GetUnsafePtr() + offset, buffer.Length - offset);
            Assert.AreNotEqual(uint4.zero, hash);
        }

        // Also check various smaller sizes at different offsets.
        for (int bytes_to_hash = 1; bytes_to_hash <= 512; ++bytes_to_hash)
        {
            for (int offset = 0; offset < 64; ++offset)
            {
                var hash = xxHash3.Hash128((byte*)buffer.GetUnsafePtr() + offset, bytes_to_hash);
                Assert.AreNotEqual(uint4.zero, hash);
            }
        }

        buffer.Dispose();
    }

    [Test]
    public unsafe void xxHash3_Hash64_Alignment()
    {
        // arm 32 bit has strict alignment requirements so we must be sure that we can
        // hash at any alignment on that platform.
        const int kBufferBytes = 1024 * 1024; // 1 MB
        var buffer = new NativeArray<byte>(kBufferBytes, Allocator.Temp);

        for (int i = 0; i < buffer.Length; ++i)
        {
            buffer[i] = (byte)i;
        }

        for (int offset = 0; offset < 64; ++offset)
        {
            var hash = xxHash3.Hash64((byte*)buffer.GetUnsafePtr() + offset, buffer.Length - offset);
            Assert.AreNotEqual(uint2.zero, hash);
        }

        // Also check various smaller sizes at different offsets.
        for (int bytes_to_hash = 1; bytes_to_hash <= 512; ++bytes_to_hash)
        {
            for (int offset = 0; offset < 64; ++offset)
            {
                var hash = xxHash3.Hash64((byte*)buffer.GetUnsafePtr() + offset, bytes_to_hash);
                Assert.AreNotEqual(uint2.zero, hash);
            }
        }

        buffer.Dispose();
    }

    [Test]
    public unsafe void xxHash3_Streaming_Hash128_Alignment()
    {
        // arm 32 bit has strict alignment requirements so we must be sure that we can
        // hash at any alignment on that platform.
        const int kBufferBytes = 1024 * 1024; // 1 MB
        var buffer = new NativeArray<byte>(kBufferBytes, Allocator.Temp);

        for (int i = 0; i < buffer.Length; ++i)
        {
            buffer[i] = (byte)i;
        }

        for (int offset = 0; offset < 64; ++offset)
        {
            var stream = new xxHash3.StreamingState(isHash64: false);
            stream.Update((byte*)buffer.GetUnsafePtr() + offset, buffer.Length - offset);
            var hash = stream.DigestHash128();
            Assert.AreNotEqual(uint4.zero, hash);
        }

        // Also check various smaller sizes at different offsets.
        for (int bytes_to_hash = 1; bytes_to_hash <= 512; ++bytes_to_hash)
        {
            for (int offset = 0; offset < 64; ++offset)
            {
                var stream = new xxHash3.StreamingState(isHash64: false);
                stream.Update((byte*)buffer.GetUnsafePtr() + offset, bytes_to_hash);
                var hash = stream.DigestHash128();
                Assert.AreNotEqual(uint4.zero, hash);
            }
        }

        buffer.Dispose();
    }

    [Test]
    public unsafe void xxHash3_Streaming_Hash64_Alignment()
    {
        // arm 32 bit has strict alignment requirements so we must be sure that we can
        // hash at any alignment on that platform.
        const int kBufferBytes = 1024 * 1024; // 1 MB
        var buffer = new NativeArray<byte>(kBufferBytes, Allocator.Temp);

        for (int i = 0; i < buffer.Length; ++i)
        {
            buffer[i] = (byte)i;
        }

        for (int offset = 0; offset < 64; ++offset)
        {
            var stream = new xxHash3.StreamingState(isHash64: true);
            stream.Update((byte*)buffer.GetUnsafePtr() + offset, buffer.Length - offset);
            var hash = stream.DigestHash64();
            Assert.AreNotEqual(uint2.zero, hash);
        }

        // Also check various smaller sizes at different offsets.
        for (int bytes_to_hash = 1; bytes_to_hash <= 512; ++bytes_to_hash)
        {
            for (int offset = 0; offset < 64; ++offset)
            {
                var stream = new xxHash3.StreamingState(isHash64: true);
                stream.Update((byte*)buffer.GetUnsafePtr() + offset, bytes_to_hash);
                var hash = stream.DigestHash64();
                Assert.AreNotEqual(uint2.zero, hash);
            }
        }

        buffer.Dispose();
    }
}
