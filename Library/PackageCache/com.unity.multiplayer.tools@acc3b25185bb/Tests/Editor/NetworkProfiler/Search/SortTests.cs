using System.Collections.Generic;
using NUnit.Framework;
using Unity.Multiplayer.Tools.NetworkProfiler.Editor;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Tests.Editor
{
    class SortTest
    {
        static readonly List<TestRowData> TestSearchDataList = new List<TestRowData>
        {
            // Game objects
            new TestRowData("Bullet", "gameobject", received: 12),
            new TestRowData("Bullet(1)", "gameobject", sent: 8),
            new TestRowData("Bullet(2)", "gameobject", sent: 3),
            new TestRowData("Bullet(3)", "gameobject", received: 4),
            new TestRowData("Bullet(4)", "gameobject", sent: 1),
        };

        static readonly List<TestRowData> TestSameNameData = new List<TestRowData>
        {
            // Game objects with the same path but different IDs
            new TestRowData("Bullet", "gameobject", received: 12, id: 0),
            new TestRowData("Bullet", "gameobject", sent: 8, id: 1),
            new TestRowData("Bullet", "gameobject", sent: 3, id: 2),
            new TestRowData("Bullet", "gameobject", received: 4, id: 3),
            new TestRowData("Bullet", "gameobject", sent: 1, id: 4),
        };

        [TestCase(SortDirection.NameAscending, "Bullet")]
        [TestCase(SortDirection.NameDescending, "Bullet(4)")]
        public void Sort_PrintableRowSorting_Name(SortDirection direction, string expected)
        {
            var list = new List<IRowData>(TestSearchDataList);

            RowDataSorting.Sort(list, direction);

            Assert.AreEqual(expected, list[0].Name);
        }

        [TestCase(SortDirection.BytesReceivedAscending, 0)]
        [TestCase(SortDirection.BytesReceivedDescending, 12)]
        public void Sort_PrintableRowSorting_BytesReceived(SortDirection direction, int expected)
        {
            var list = new List<IRowData>(TestSearchDataList);

            RowDataSorting.Sort(list, direction);

            Assert.AreEqual(expected, list[0].Bytes.Received);
        }

        [TestCase(SortDirection.BytesSentAscending, 0)]
        [TestCase(SortDirection.BytesSentDescending, 8)]
        public void Sort_PrintableRowSorting_BytesSent(SortDirection direction, int expected)
        {
            var list = new List<IRowData>(TestSearchDataList);

            RowDataSorting.Sort(list, direction);

            Assert.AreEqual(expected, list[0].Bytes.Sent);
        }

        [TestCase(SortDirection.BytesReceivedAscending, 0, TestName = "SortAndExpand_BytesReceivedAscending_EnsuresCorrectSortingAndFoldout")]
        [TestCase(SortDirection.BytesReceivedDescending, 12, TestName = "SortAndExpand_BytesReceivedDescending_EnsuresCorrectSortingAndFoldout")]
        public void Sort_And_Expand_Test(SortDirection direction, int expected)
        {
            var list = new List<IRowData>(TestSearchDataList);
            var detailsViewFoldoutState = new DetailsViewFoldoutState();
            detailsViewFoldoutState.SetFoldoutContractAll();
            var treeViewElementNoId = "Bullet";
            detailsViewFoldoutState.SetFoldout(treeViewElementNoId, true);

            RowDataSorting.Sort(list, direction);
            Assert.AreEqual(expected, list[0].Bytes.Received);

            var isFoldedOut = detailsViewFoldoutState.IsFoldedOut(treeViewElementNoId);

            // Check if foldout state remains consistent
            Assert.IsTrue(isFoldedOut);
        }

        [Test]
        public void SortByBytesSentDescending_SamePath_EnsuresOrderAndFoldoutConsistency()
        {
            var list = new List<IRowData>(TestSameNameData);

            var detailsViewFoldoutState = new DetailsViewFoldoutState();

            // We use a TreeView id which is path + index to identify the items to 
            // make sure the id is unique i.e. Bullet as path and 0 as id combined
            var treeViewElementId0 = "Bullet0";
            detailsViewFoldoutState.SetFoldout(treeViewElementId0, true);

            RowDataSorting.Sort(list, SortDirection.BytesSentDescending);

            // Check if the order is correct after sorting
            Assert.AreEqual(8, list[0].Bytes.Sent);
            Assert.AreEqual(3, list[1].Bytes.Sent);
            Assert.AreEqual(1, list[2].Bytes.Sent);
            Assert.AreEqual(0, list[3].Bytes.Sent);
            Assert.AreEqual(0, list[4].Bytes.Sent);

            // Check if foldout state remains consistent
            Assert.IsTrue(detailsViewFoldoutState.IsFoldedOut(treeViewElementId0));
        }

        [TestCase(SortDirection.BytesReceivedAscending, 0, TestName = "SortAndFold_BytesReceivedAscending_EnsuresCorrectOrderAndFoldout")]
        [TestCase(SortDirection.BytesReceivedDescending, 12, TestName = "SortAndFold_BytesReceivedDescending_EnsuresCorrectOrderAndFoldout")]
        [TestCase(SortDirection.BytesSentAscending, 0, TestName = "SortAndContract_BytesSentAscending_EnsuresCorrectOrderAndFoldout")]
        [TestCase(SortDirection.BytesSentDescending, 8, TestName = "SortAndContract_BytesSentDescending_EnsuresCorrectOrderAndFoldout")]
        public void SortAndContract_SamePath_GivenSortDirection_EnsuresCorrectOrderAndFoldout(SortDirection direction, int expected)
        {
            var list = new List<IRowData>(TestSearchDataList);
            var detailsViewFoldoutState = new DetailsViewFoldoutState();
            detailsViewFoldoutState.SetFoldoutExpandAll();
            var treeViewElementNoId = "Bullet";
            detailsViewFoldoutState.SetFoldout(treeViewElementNoId, false);

            RowDataSorting.Sort(list, direction);

            // Check if the order is correct after sorting
            Assert.AreEqual(expected, direction.ToString().Contains("Received") ? list[0].Bytes.Received : list[0].Bytes.Sent);

            var isFoldedOut = detailsViewFoldoutState.IsFoldedOut(treeViewElementNoId);

            // Check if foldout state remains consistent
            Assert.IsFalse(isFoldedOut);
        }

        [TestCase(SortDirection.BytesReceivedAscending, 0, TestName = "SortAndContract_BytesReceivedAscending_EnsuresCorrectOrderAndFoldout")]
        [TestCase(SortDirection.BytesReceivedDescending, 12, TestName = "SortAndContract_BytesReceivedDescending_EnsuresCorrectOrderAndFoldout")]
        [TestCase(SortDirection.BytesSentAscending, 0, TestName = "SortAndContract_BytesSentAscending_EnsuresCorrectOrderAndFoldout")]
        [TestCase(SortDirection.BytesSentDescending, 8, TestName = "SortAndContract_BytesSentDescending_EnsuresCorrectOrderAndFoldout")]
        public void SortAndContract_GivenSortDirection_EnsuresCorrectOrderAndFoldout(SortDirection direction, int expected)
        {
            var list = new List<IRowData>(TestSameNameData);
            var detailsViewFoldoutState = new DetailsViewFoldoutState();
            detailsViewFoldoutState.SetFoldoutExpandAll();

            // We use a TreeView id which is path + index to identify the items to 
            // make sure the id is unique i.e. Bullet as path and 0 as id combined
            var treeViewElementId0 = "Bullet0";
            var treeViewElementId1 = "Bullet1";

            detailsViewFoldoutState.SetFoldout(treeViewElementId0, false);

            RowDataSorting.Sort(list, direction);
            Assert.AreEqual(expected, direction.ToString().Contains("Received") ? list[0].Bytes.Received : list[0].Bytes.Sent);

            // Check if foldout state remains consistent
            var isFoldedOut = detailsViewFoldoutState.IsFoldedOut(treeViewElementId0);
            Assert.IsFalse(isFoldedOut);

            isFoldedOut = detailsViewFoldoutState.IsFoldedOut(treeViewElementId1);
            Assert.IsTrue(isFoldedOut);
        }

        [Test]
        public void Sort_DataListIsEmpty_EnsuresListRemainsEmpty()
        {
            var list = new List<IRowData>();
            RowDataSorting.Sort(list, SortDirection.BytesReceivedAscending);

            // The list should still be empty after sorting.
            Assert.AreEqual(0, list.Count);
        }
    }
}
