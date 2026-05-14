using NUnit.Framework;
using UnityEngine;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using Unity.Multiplayer.Tools.Common.Visualization;
using Unity.Multiplayer.Tools.Adapters;
using System;
using System.Collections.Generic;

namespace Unity.Multiplayer.Tools.NetVis.Tests.Editor
{
    internal class CustomColorSettingsTests
    {
        OwnershipSettings m_OwnershipSettings;
        bool m_ColorsChangedEventRaised;

        // Dictionary to store original colors for restoration after tests
        Dictionary<int, Color> m_OriginalColors;

        // Define ClientId constants for clarity
        static readonly ClientId k_HostId = (ClientId)0;
        static readonly ClientId k_ClientId1 = (ClientId)1;
        static readonly ClientId k_ClientId2 = (ClientId)2;
        static readonly ClientId k_ClientId3 = (ClientId)3;
        static readonly ClientId k_ClientId4 = (ClientId)4;
        static readonly ClientId k_ClientId5 = (ClientId)5;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Save the original colors at the beginning of the test run
            m_OriginalColors = BackupCurrentColors();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // Restore original colors at the end of all tests
            RestoreColors(m_OriginalColors);
        }

        /// <summary>
        /// Backs up all current custom color settings.
        /// </summary>
        /// <returns>Dictionary mapping color IDs to their custom colors</returns>
        static Dictionary<int, Color> BackupCurrentColors()
        {
            // Use the GetColors method to get a copy of all current custom colors
            return CustomColorSettings.GetColors();
        }

        /// <summary>
        /// Restores a set of previously backed up colors.
        /// </summary>
        /// <param name="colorBackup">Dictionary mapping color IDs to their custom colors</param>
        static void RestoreColors(Dictionary<int, Color> colorBackup)
        {
            // Clear any custom colors changes made by the tests
            CustomColorSettings.ClearColors();
            // Restore all original custom colors
            CustomColorSettings.SetColors(colorBackup);
        }

        [SetUp]
        public void SetUp()
        {
            // Ensure a clean state before each test by clearing any custom colors
            CustomColorSettings.ClearColors();
            m_ColorsChangedEventRaised = false;

            // Instantiate OwnershipSettings as a regular class
            m_OwnershipSettings = new OwnershipSettings();

            // Subscribe to the event for testing
            m_OwnershipSettings.ColorsChanged += OnColorsChanged;
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            if (m_OwnershipSettings != null)
            {
                m_OwnershipSettings.ColorsChanged -= OnColorsChanged;
                // No need to destroy, just release the reference
                m_OwnershipSettings = null;
            }
            CustomColorSettings.ClearColors();
            // Don't restore colors here - we'll do it once at the very end
        }

        void OnColorsChanged()
        {
            m_ColorsChangedEventRaised = true;
        }

        // --- Tests for CustomColorSettings static methods --- 

        [Test]
        public void SetAndGetColor_WhenNewId_SetsAndGetsColorCorrectly()
        {
            // Arrange
            var id = k_ClientId1;
            var expectedColor = Color.red;

            // Act
            CustomColorSettings.SetColor((int)id, expectedColor);
            var hasColor = CustomColorSettings.HasColor((int)id);
            var actualColor = CustomColorSettings.GetColor((int)id);

            // Assert
            Assert.IsTrue(hasColor, "HasColor should return true after setting a color.");
            Assert.AreEqual(expectedColor, actualColor, "GetColor should return the color that was set.");
        }

        [Test]
        public void SetColor_WhenOverwritingExistingId_UpdatesColorCorrectly()
        {
            // Arrange
            var id = k_ClientId1;
            var initialColor = Color.red;
            var newColor = Color.blue;

            // Act
            CustomColorSettings.SetColor((int)id, initialColor); // Set initial
            CustomColorSettings.SetColor((int)id, newColor);     // Overwrite
            var actualColor = CustomColorSettings.GetColor((int)id);

            // Assert
            Assert.AreEqual(newColor, actualColor, "GetColor should return the updated color after overwriting.");
        }

        [Test]
        public void HasColor_WhenColorNotSet_ReturnsFalse()
        {
            // Arrange
            var idWithColor = k_ClientId1;
            var idWithoutColor = k_ClientId2;
            CustomColorSettings.SetColor((int)idWithColor, Color.red);

            // Act
            var hasColor = CustomColorSettings.HasColor((int)idWithoutColor);

            // Assert
            Assert.IsFalse(hasColor, "HasColor should return false for an ID without a custom color.");
        }

        [Test]
        public void RemoveColor_WhenColorExists_RemovesColor()
        {
            // Arrange
            var id = k_ClientId1;
            CustomColorSettings.SetColor((int)id, Color.red);

            // Act
            CustomColorSettings.RemoveColor((int)id);
            var hasColor = CustomColorSettings.HasColor((int)id);

            // Assert
            Assert.IsFalse(hasColor, "HasColor should return false after removing the color.");
        }

        [Test]
        public void RemoveColor_WhenColorDoesNotExist_DoesNotThrow()
        {
            // Arrange
            var idWithColor = k_ClientId1;
            var idToRemove = k_ClientId2;
            CustomColorSettings.SetColor((int)idWithColor, Color.red);

            // Act & Assert
            Assert.DoesNotThrow(() => CustomColorSettings.RemoveColor((int)idToRemove),
                "Removing a non-existent color should not throw an exception.");
            Assert.IsTrue(CustomColorSettings.HasColor((int)idWithColor),
                "Removing a non-existent color should not affect existing colors.");
        }

        [Test]
        public void ClearColors_WhenColorsExist_RemovesAllColors()
        {
            // Arrange
            var id1 = k_HostId;
            var id2 = k_ClientId1;
            var id3 = k_ClientId5;
            CustomColorSettings.SetColor((int)id1, Color.yellow);
            CustomColorSettings.SetColor((int)id2, Color.red);
            CustomColorSettings.SetColor((int)id3, Color.blue);

            // Act
            CustomColorSettings.ClearColors();

            // Assert
            Assert.IsFalse(CustomColorSettings.HasColor((int)id1), "Color for ID 0 should be cleared.");
            Assert.IsFalse(CustomColorSettings.HasColor((int)id2), "Color for ID 1 should be cleared.");
            Assert.IsFalse(CustomColorSettings.HasColor((int)id3), "Color for ID 5 should be cleared.");
        }

        // --- Tests for OwnershipSettings interaction --- 

        [Test]
        public void OwnershipSettings_ServerHostColor_ReflectsCustomColorAndDefault()
        {
            // Arrange
            var hostId = k_HostId;
            var customHostColor = Color.magenta;
            // Use the static CategoricalColorPalette directly for the expected default color (palette uses int)
            var defaultHostColor = CategoricalColorPalette.GetColor((int)hostId);

            // Assert Initial State (Default) - Assuming ServerHostColor accesses CategoricalColorPalette when no custom color exists
            Assert.AreEqual(defaultHostColor, m_OwnershipSettings.ServerHostColor, "Initial ServerHostColor should be the default palette color.");

            // Act 1: Set custom color via CustomColorSettings (simulate external change)
            CustomColorSettings.SetColor((int)hostId, customHostColor);

            // Assert 1: OwnershipSettings reflects the custom color
            Assert.AreEqual(customHostColor, m_OwnershipSettings.ServerHostColor, "ServerHostColor should return the custom color when set via CustomColorSettings.");

            // Act 2: Remove custom color via CustomColorSettings
            CustomColorSettings.RemoveColor((int)hostId);

            // Assert 2: OwnershipSettings reverts to default
            Assert.AreEqual(defaultHostColor, m_OwnershipSettings.ServerHostColor, "ServerHostColor should revert to default after removing custom color via CustomColorSettings.");
        }

        [Test]
        public void OwnershipSettings_GetClientColor_ReflectsCustomColorAndDefault()
        {
            // Arrange
            var clientId = k_ClientId2; // Use a non-zero ID
            var customClientColor = Color.cyan;
            // Use the static CategoricalColorPalette directly for the expected default color (palette uses int)
            var defaultClientColor = CategoricalColorPalette.GetColor((int)clientId);

            // Assert Initial State (Default) - Assuming GetClientColor accesses CategoricalColorPalette when no custom color exists
            Assert.AreEqual(defaultClientColor, m_OwnershipSettings.GetClientColor(clientId), "Initial GetClientColor should return the default palette color.");

            // Act 1: Set custom color via CustomColorSettings
            CustomColorSettings.SetColor((int)clientId, customClientColor);

            // Assert 1: OwnershipSettings reflects the custom color
            Assert.AreEqual(customClientColor, m_OwnershipSettings.GetClientColor(clientId), "GetClientColor should return the custom color when set via CustomColorSettings.");

            // Act 2: Remove custom color via CustomColorSettings
            CustomColorSettings.RemoveColor((int)clientId);

            // Assert 2: OwnershipSettings reverts to default
            Assert.AreEqual(defaultClientColor, m_OwnershipSettings.GetClientColor(clientId), "GetClientColor should revert to default after removing custom color via CustomColorSettings.");
        }

        [Test]
        public void OwnershipSettings_SetCustomColor_UpdatesCustomColorSettingsAndRaisesEvent()
        {
            // Arrange
            var clientId = k_ClientId3;
            var customColor = Color.green;

            // Act
            m_OwnershipSettings.SetCustomColor(clientId, customColor);

            // Assert
            Assert.IsTrue(CustomColorSettings.HasColor((int)clientId), "CustomColorSettings should have the color after SetCustomColor.");
            Assert.AreEqual(customColor, CustomColorSettings.GetColor((int)clientId), "CustomColorSettings should have the correct color value.");
            Assert.AreEqual(customColor, m_OwnershipSettings.GetClientColor(clientId), "OwnershipSettings should return the new custom color.");
            Assert.IsTrue(m_ColorsChangedEventRaised, "ColorsChanged event should be raised after SetCustomColor.");
        }

        [Test]
        public void OwnershipSettings_ResetCustomColors_ClearsCustomColorSettingsAndRaisesEvent()
        {
            // Arrange
            var id1 = k_HostId;
            var id2 = k_ClientId4;
            CustomColorSettings.SetColor((int)id1, Color.yellow); // Set some initial colors
            CustomColorSettings.SetColor((int)id2, Color.blue);
            m_ColorsChangedEventRaised = false; // Reset flag
            // Get expected default colors directly from the static palette
            var defaultColor1 = CategoricalColorPalette.GetColor((int)id1);
            var defaultColor2 = CategoricalColorPalette.GetColor((int)id2);

            // Act
            m_OwnershipSettings.ResetCustomColors();

            // Assert
            Assert.IsFalse(CustomColorSettings.HasColor((int)id1), "CustomColorSettings should not have color for id1 after ResetCustomColors.");
            Assert.IsFalse(CustomColorSettings.HasColor((int)id2), "CustomColorSettings should not have color for id2 after ResetCustomColors.");
            // Check that OwnershipSettings now returns defaults
            Assert.AreEqual(defaultColor1, m_OwnershipSettings.ServerHostColor, "ServerHostColor should revert to default after ResetCustomColors.");
            Assert.AreEqual(defaultColor2, m_OwnershipSettings.GetClientColor(id2), "GetClientColor should revert to default after ResetCustomColors.");
            Assert.IsTrue(m_ColorsChangedEventRaised, "ColorsChanged event should be raised after ResetCustomColors.");
        }
        // --- Tests for GetColors and SetColors methods ---

        [Test]
        public void GetColors_WhenEmpty_ReturnsEmptyDictionary()
        {
            // Arrange - SetUp already clears colors

            // Act
            var colors = CustomColorSettings.GetColors();

            // Assert
            Assert.AreEqual(0, colors.Count, "GetColors should return an empty dictionary when no colors are set.");
        }

        [Test]
        public void GetColors_WithMultipleColors_ReturnsAllColors()
        {
            // Arrange
            CustomColorSettings.SetColor(0, Color.red);
            CustomColorSettings.SetColor(1, Color.green);
            CustomColorSettings.SetColor(5, Color.blue);

            // Act
            var colors = CustomColorSettings.GetColors();

            // Assert
            Assert.AreEqual(3, colors.Count, "GetColors should return all 3 colors that were set.");
            Assert.AreEqual(Color.red, colors[0], "Dictionary should contain the correct color for ID 0.");
            Assert.AreEqual(Color.green, colors[1], "Dictionary should contain the correct color for ID 1.");
            Assert.AreEqual(Color.blue, colors[5], "Dictionary should contain the correct color for ID 5.");
        }

        [Test]
        public void SetColors_WhenEmpty_DoesNothing()
        {
            // Arrange
            var emptyDict = new Dictionary<int, Color>();

            // Act
            CustomColorSettings.SetColors(emptyDict);

            // Assert
            Assert.AreEqual(0, CustomColorSettings.GetColors().Count, "SetColors with empty dictionary should result in no colors.");
        }

        [Test]
        public void SetColors_WithMultipleColors_SetsAllColors()
        {
            // Arrange
            var colors = new Dictionary<int, Color>
            {
                { 0, Color.cyan },
                { 3, Color.magenta },
                { 7, Color.yellow }
            };

            // Act
            CustomColorSettings.SetColors(colors);

            // Assert
            var retrievedColors = CustomColorSettings.GetColors();
            Assert.AreEqual(3, retrievedColors.Count, "SetColors should set all 3 colors.");
            Assert.AreEqual(Color.cyan, retrievedColors[0], "Color for ID 0 should be set correctly.");
            Assert.AreEqual(Color.magenta, retrievedColors[3], "Color for ID 3 should be set correctly.");
            Assert.AreEqual(Color.yellow, retrievedColors[7], "Color for ID 7 should be set correctly.");
        }

        [Test]
        public void SetColors_OverwritesExistingColors()
        {
            // Arrange - Set some initial colors
            CustomColorSettings.SetColor(1, Color.red);
            CustomColorSettings.SetColor(2, Color.green);

            // Create a different set of colors
            var newColors = new Dictionary<int, Color>
            {
                { 1, Color.blue }, // Overwrite ID 1
                { 3, Color.yellow } // New ID 3
                // ID 2 should remain unchanged
            };

            // Act
            CustomColorSettings.SetColors(newColors);

            // Assert
            var retrievedColors = CustomColorSettings.GetColors();
            Assert.AreEqual(3, retrievedColors.Count, "There should be exactly 3 colors after SetColors.");
            Assert.AreEqual(Color.blue, retrievedColors[1], "Color for ID 1 should be overwritten.");
            Assert.AreEqual(Color.green, retrievedColors[2], "Color for ID 2 should be preserved.");
            Assert.AreEqual(Color.yellow, retrievedColors[3], "Color for ID 3 should be added.");
        }

        [Test]
        public void GetColors_ReturnsCopy_NotReference()
        {
            // Arrange
            CustomColorSettings.SetColor(0, Color.red);
            CustomColorSettings.SetColor(1, Color.green);

            // Act
            var colors = CustomColorSettings.GetColors();
            colors[0] = Color.blue; // Modify the retrieved dictionary

            // Assert
            var newRetrieval = CustomColorSettings.GetColors();
            Assert.AreEqual(Color.red, newRetrieval[0], "Modifying the dictionary returned by GetColors should not affect the actual stored colors.");
        }
    }
}
