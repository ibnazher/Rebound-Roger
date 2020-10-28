namespace Rebound.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class vertion2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TransactionDetails", "ReservationId", "dbo.Reservations");
            DropForeignKey("dbo.TransactionDetails", "TransactionID", "dbo.Transactions");
            DropIndex("dbo.TransactionDetails", new[] { "TransactionID" });
            DropIndex("dbo.TransactionDetails", new[] { "ReservationId" });
            AddColumn("dbo.Transactions", "ReservationId", c => c.Int(nullable: false));
            CreateIndex("dbo.Transactions", "ReservationId");
            AddForeignKey("dbo.Transactions", "ReservationId", "dbo.Reservations", "Id", cascadeDelete: true);
            DropTable("dbo.TransactionDetails");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TransactionDetails",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        VoucherNo = c.Int(nullable: false),
                        VoucherType = c.String(),
                        Narration = c.String(),
                        TrasactionalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedOn = c.DateTime(nullable: false),
                        TransactionDate = c.DateTime(nullable: false),
                        TransactionID = c.Guid(nullable: false),
                        ReservationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.Transactions", "ReservationId", "dbo.Reservations");
            DropIndex("dbo.Transactions", new[] { "ReservationId" });
            DropColumn("dbo.Transactions", "ReservationId");
            CreateIndex("dbo.TransactionDetails", "ReservationId");
            CreateIndex("dbo.TransactionDetails", "TransactionID");
            AddForeignKey("dbo.TransactionDetails", "TransactionID", "dbo.Transactions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TransactionDetails", "ReservationId", "dbo.Reservations", "Id", cascadeDelete: true);
        }
    }
}
