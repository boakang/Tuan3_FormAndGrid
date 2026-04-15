using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace HQSoft.Models
{
    // EF6 DbContext mapped directly to SQL Server tables/procedures.
    public class Product_eSales_2026Entities : DbContext
    {
        public Product_eSales_2026Entities()
            : base("name=Product_eSales_2026Entities")
        {
            Database.SetInitializer<Product_eSales_2026Entities>(null);
        }

        public Product_eSales_2026Entities(string connectionString)
            : base(connectionString)
        {
            Database.SetInitializer<Product_eSales_2026Entities>(null);
        }

        public DbSet<FS_Batch_Huy> FS_Batch_Huy { get; set; }
        public DbSet<FS_BatchDetail_Huy> FS_BatchDetail_Huy { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FS_Batch_Huy>().ToTable("FS_Batch_Huy");
            modelBuilder.Entity<FS_Batch_Huy>().HasKey(x => new { x.CpnyID, x.BatchID });

            modelBuilder.Entity<FS_BatchDetail_Huy>().ToTable("FS_BatchDetail_Huy");
            modelBuilder.Entity<FS_BatchDetail_Huy>().HasKey(x => new { x.CpnyID, x.BatchID, x.InventoryID });

            base.OnModelCreating(modelBuilder);
        }

        public List<FS10901_pgBatch_Huy_Result> FS10901_pgBatch_Huy(string cpnyID, string userName, short langID, string batchID, string branchID)
        {
            return Database.SqlQuery<FS10901_pgBatch_Huy_Result>(
                "EXEC dbo.FS10901_pgBatch_Huy @CpnyID, @UserName, @LangID, @BatchID, @BranchID",
                new SqlParameter("@CpnyID", (object)cpnyID ?? DBNull.Value),
                new SqlParameter("@UserName", (object)userName ?? DBNull.Value),
                new SqlParameter("@LangID", langID),
                new SqlParameter("@BatchID", (object)batchID ?? DBNull.Value),
                new SqlParameter("@BranchID", (object)branchID ?? DBNull.Value)
            ).ToList();
        }

        public List<FS10901_pgBatchDetail_Huy_Result> FS10901_pgBatchDetail_Huy(string cpnyID, string userName, short langID, string batchID, string branchID)
        {
            return Database.SqlQuery<FS10901_pgBatchDetail_Huy_Result>(
                "EXEC dbo.FS10901_pgBatchDetail_Huy @CpnyID, @UserName, @LangID, @BatchID, @BranchID",
                new SqlParameter("@CpnyID", (object)cpnyID ?? DBNull.Value),
                new SqlParameter("@UserName", (object)userName ?? DBNull.Value),
                new SqlParameter("@LangID", langID),
                new SqlParameter("@BatchID", (object)batchID ?? DBNull.Value),
                new SqlParameter("@BranchID", (object)branchID ?? DBNull.Value)
            ).ToList();
        }
    }

    public static class DbSetCompatExtensions
    {
        public static void AddObject<T>(this DbSet<T> set, T item) where T : class
        {
            set.Add(item);
        }

        public static void DeleteObject<T>(this DbSet<T> set, T item) where T : class
        {
            set.Remove(item);
        }
    }

    [Table("FS_Batch_Huy")]
    public class FS_Batch_Huy
    {
        [Key, Column(Order = 0), StringLength(30)]
        public string CpnyID { get; set; }

        [Key, Column(Order = 1), StringLength(30)]
        public string BatchID { get; set; }

        public DateTime? OrderDay { get; set; }
        public double TotalNumer { get; set; }
        public double TotalVolume { get; set; }
        public double TotalAmount { get; set; }
        public DateTime Crtd_DateTime { get; set; }
        public string Crtd_Prog { get; set; }
        public string Crtd_User { get; set; }
        public DateTime LUpd_DateTime { get; set; }
        public string LUpd_Prog { get; set; }
        public string LUpd_User { get; set; }
    }

    [Table("FS_BatchDetail_Huy")]
    public class FS_BatchDetail_Huy
    {
        [Key, Column(Order = 0), StringLength(30)]
        public string CpnyID { get; set; }

        [Key, Column(Order = 1), StringLength(30)]
        public string BatchID { get; set; }

        [Key, Column(Order = 2), StringLength(30)]
        public string InventoryID { get; set; }

        public double Number { get; set; }
        public double Volume { get; set; }
        public double Price { get; set; }
        public double Tax { get; set; }
        public double Amount { get; set; }
        public DateTime Crtd_DateTime { get; set; }
        public string Crtd_Prog { get; set; }
        public string Crtd_User { get; set; }
        public DateTime LUpd_DateTime { get; set; }
        public string LUpd_Prog { get; set; }
        public string LUpd_User { get; set; }
    }

    public class FS10901_pgBatch_Huy_Result
    {
        public string BatchID { get; set; }
        public string CpnyID { get; set; }
        public DateTime? OrderDay { get; set; }
        public double TotalNumer { get; set; }
        public double TotalVolume { get; set; }
        public double TotalAmount { get; set; }
    }

    public class FS10901_pgBatchDetail_Huy_Result
    {
        public string BatchID { get; set; }
        public string CpnyID { get; set; }
        public string InventoryID { get; set; }
        public string InventoryName { get; set; }
        public double Number { get; set; }
        public double Volume { get; set; }
        public double Price { get; set; }
        public double Tax { get; set; }
        public double TaxPrice { get; set; }
        public double Amount { get; set; }
    }
}
