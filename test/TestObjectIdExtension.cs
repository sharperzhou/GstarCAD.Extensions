using System;
using GrxCAD.DatabaseServices;
using GrxCAD.Runtime;
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;
using Exception = GrxCAD.Runtime.Exception;

namespace GstarCAD.Extensions.Test
{
    public class TestObjectIdExtension
    {
        [Test]
        public void TestTryGetObject()
        {
            var nullObjectIdException = Assert.Catch<Exception>(() => ObjectId.Null.TryGetObject<DBObject>(out _));
            Assert.NotNull(nullObjectIdException);
            Assert.AreEqual(nullObjectIdException.ErrorStatus, ErrorStatus.NullObjectId);

            var noTransactionException =
                Assert.Catch<Exception>(() => Active.Database.BlockTableId.TryGetObject<BlockTable>(out _));
            Assert.NotNull(noTransactionException);
            Assert.AreEqual(noTransactionException.ErrorStatus, ErrorStatus.NoActiveTransactions);

            using (Active.StartTransaction())
            {
                Assert.IsTrue(Active.Database.BlockTableId.TryGetObject<BlockTable>(out var blockTable));
                Assert.NotNull(blockTable);

                Assert.IsFalse(Active.Database.BlockTableId.TryGetObject<DBText>(out _));
            }
        }

        [Test]
        public void TestGetObject()
        {
            var nullObjectException = Assert.Catch<Exception>(() => ObjectId.Null.GetObject<DBObject>());
            Assert.NotNull(nullObjectException);
            Assert.AreEqual(nullObjectException.ErrorStatus, ErrorStatus.NullObjectId);

            var noTransactionException =
                Assert.Catch<Exception>(() => Active.Database.BlockTableId.GetObject<BlockTable>());
            Assert.NotNull(noTransactionException);
            Assert.AreEqual(noTransactionException.ErrorStatus, ErrorStatus.NoActiveTransactions);

            using (Active.StartTransaction())
            {
                Assert.NotNull(Active.Database.BlockTableId.GetObject<BlockTable>());

                Assert.NotNull(
                    Assert.Catch<InvalidCastException>(
                        () => Active.Database.BlockTableId.GetObject<DBText>()));
            }
        }
    }
}
