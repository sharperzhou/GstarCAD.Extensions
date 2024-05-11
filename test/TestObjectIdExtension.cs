using System;
#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Runtime;
using Exception = Gssoft.Gscad.Runtime.Exception;
#else
using GrxCAD.DatabaseServices;
using GrxCAD.Runtime;
using Exception = GrxCAD.Runtime.Exception;
#endif
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

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
