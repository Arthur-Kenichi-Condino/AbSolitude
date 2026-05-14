using NUnit.Framework;
using Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow;

namespace Tests.Editor.MultiplayerToolsWindowTests
{
    [TestFixture]
    internal class MultiplayerToolsWindowTests
    {
        [Test]
        public void OpenMultiplayerToolsWindowTests_OpensWithoutException()
        {
            Assert.DoesNotThrow(MultiplayerToolsWindow.Open);
        }
    }
}
