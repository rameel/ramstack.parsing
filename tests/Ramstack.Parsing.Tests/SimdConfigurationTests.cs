using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace Ramstack.Parsing;

[TestFixture]
public class SimdConfigurationTests
{
    [Test]
    public void VerifySimdConfiguration()
    {
        if (Environment.GetEnvironmentVariable("DOTNET_EnableHWIntrinsic") == "0")
        {
            Assert.That(Sse2.IsSupported, Is.False);
            Assert.That(Sse41.IsSupported, Is.False);
            Assert.That(Avx2.IsSupported, Is.False);
            Assert.That(AdvSimd.Arm64.IsSupported, Is.False);
            Assert.That(AdvSimd.IsSupported, Is.False);
        }

        if (RuntimeInformation.ProcessArchitecture == Architecture.X64 && Environment.GetEnvironmentVariable("DOTNET_EnableAVX2") == "0")
        {
            Assert.That(Sse2.IsSupported, Is.True);
            Assert.That(Sse41.IsSupported, Is.True);
            Assert.That(Avx2.IsSupported, Is.False);
        }
    }
}
