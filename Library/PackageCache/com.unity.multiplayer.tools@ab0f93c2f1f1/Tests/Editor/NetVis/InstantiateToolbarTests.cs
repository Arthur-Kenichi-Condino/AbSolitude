using NUnit.Framework;
using Unity.Multiplayer.Tools.NetVis.Editor.UI;

namespace Unity.Multiplayer.Tools.NetVis.Tests.Editor
{
    /// <remarks>
    /// Unity catches and rethrows errors that occur during panel creation in a way that does not preserve the full
    /// context and callstack. For this reason, it's nice to be able to isolate any such failures in a standalone
    /// test to get the full context. It's also beneficial to catch any such issues in CI.
    /// </remarks>
    [TestFixture]
    class InstantiateToolbarTests
    {
        [Test]
        public void InstantiateNetVisToolbarOverlay()
        {
            var toolbarOverlay = new NetVisToolbarOverlay();
            toolbarOverlay.CreatePanelContent();
        }

        [Test]
        public void InstantiateBandwidthToolbarDropdownToggleTest()
        {
            var bandwidthDropdownToggle = new BandwidthToolbarDropdownToggle();
        }

        [Test]
        public void InstantiateOwnershipToolbarDropdownToggleTest()
        {
            var ownershipDropdownToggle = new OwnershipToolbarDropdownToggle();
        }

        [Test]
        public void InstantiateSettingsToolbarButton()
        {
            var settingsToolbarButton = new SettingsToolbarButton();
        }
    }
}
