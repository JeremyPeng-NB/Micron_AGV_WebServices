namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Micron_AGV_DB : DbContext
    {
        public Micron_AGV_DB()
            : base("name=Micron_AGV_DB")
        {
        }

        public virtual DbSet<CarStatus> CarStatuss { get; set; }
        public virtual DbSet<EquipmentStatus> EquipmentStatuss { get; set; }
        public virtual DbSet<DispatchRecord> DispatchRecords { get; set; }
        public virtual DbSet<TaskList> TaskLists { get; set; }
        public virtual DbSet<TaskType> TaskTypes { get; set; }
        public virtual DbSet<DispatchHistory> DispatchHistorys { get; set; }
        public virtual DbSet<DispatchErrorLog> DispatchErrorLogs { get; set; }
        public virtual DbSet<ShelfErrorLog> ShelfErrorLogs { get; set; }
        public virtual DbSet<PurchaseAndShipmentLog> PurchaseAndShipmentLogs { get; set; }
        public virtual DbSet<ShelfManagement> ShelfManagements { get; set; }
        public virtual DbSet<ShelfManagementTEST> ShelfManagementTESTs { get; set; }
        public virtual DbSet<TaskHistory> TaskHistorys { get; set; }
        public virtual DbSet<TaskProcess> TaskProcesss { get; set; }
        public virtual DbSet<View_DispatchRecord> View_DispatchRecords { get; set; }
        public virtual DbSet<View_TaskList> View_TaskLists { get; set; }
        public virtual DbSet<View_TaskType> View_TaskTypes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
