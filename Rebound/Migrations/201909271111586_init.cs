namespace Rebound.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Gender = c.Int(nullable: false),
                        Email = c.String(),
                        Phone = c.String(maxLength: 18),
                        Mobile = c.String(maxLength: 18),
                        Code = c.String(),
                        Blacklist = c.Boolean(nullable: false),
                        DatrOfBirth = c.DateTime(nullable: false),
                        Nationality = c.String(),
                        City = c.String(),
                        Street = c.String(),
                        Address = c.String(),
                        PaymentType = c.Int(nullable: false),
                        Balance = c.Decimal(precision: 18, scale: 2),
                        Note = c.String(),
                        Status = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Reservations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartedAt = c.DateTime(nullable: false),
                        EndAt = c.DateTime(),
                        Note = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        ApproveDate = c.DateTime(),
                        CancelDate = c.DateTime(),
                        CreateUser = c.String(),
                        UpdateUser = c.String(),
                        ApproveUser = c.String(),
                        CancelUser = c.String(),
                        CancelNote = c.String(),
                        BillingStatus = c.Int(nullable: false),
                        Status = c.Boolean(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaidPrice = c.Decimal(precision: 18, scale: 2),
                        ExtraitemPrice = c.Decimal(precision: 18, scale: 2),
                        Client = c.Int(nullable: false),
                        ItemId = c.Int(nullable: false),
                        OperatorsId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customers", t => t.Client, cascadeDelete: true)
                .ForeignKey("dbo.Items", t => t.ItemId, cascadeDelete: true)
                .ForeignKey("dbo.Operators", t => t.OperatorsId, cascadeDelete: true)
                .Index(t => t.Client)
                .Index(t => t.ItemId)
                .Index(t => t.OperatorsId);
            
            CreateTable(
                "dbo.Items",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Image = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ShortDescription = c.String(),
                        CategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItemCategories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.ItemCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Operators",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Gender = c.Int(nullable: false),
                        Email = c.String(),
                        Phone = c.String(maxLength: 18),
                        Mobile = c.String(maxLength: 18),
                        Status = c.Boolean(nullable: false),
                        AppUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AppUserId)
                .Index(t => t.AppUserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        OperatorsId = c.Int(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Reservations", t => t.ReservationId, cascadeDelete: true)
                .ForeignKey("dbo.Transactions", t => t.TransactionID, cascadeDelete: true)
                .Index(t => t.TransactionID)
                .Index(t => t.ReservationId);
            
            CreateTable(
                "dbo.Transactions",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        UserId = c.String(),
                        VoucherNo = c.Int(nullable: false),
                        VoucherType = c.String(),
                        TrasactionalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Narration = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        TransactionDate = c.DateTime(nullable: false),
                        Client = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customers", t => t.Client, cascadeDelete: false)
                .Index(t => t.Client);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.TransactionDetails", "TransactionID", "dbo.Transactions");
            DropForeignKey("dbo.Transactions", "Client", "dbo.Customers");
            DropForeignKey("dbo.TransactionDetails", "ReservationId", "dbo.Reservations");
            DropForeignKey("dbo.Reservations", "OperatorsId", "dbo.Operators");
            DropForeignKey("dbo.Operators", "AppUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Reservations", "ItemId", "dbo.Items");
            DropForeignKey("dbo.Items", "CategoryId", "dbo.ItemCategories");
            DropForeignKey("dbo.Reservations", "Client", "dbo.Customers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Transactions", new[] { "Client" });
            DropIndex("dbo.TransactionDetails", new[] { "ReservationId" });
            DropIndex("dbo.TransactionDetails", new[] { "TransactionID" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Operators", new[] { "AppUserId" });
            DropIndex("dbo.Items", new[] { "CategoryId" });
            DropIndex("dbo.Reservations", new[] { "OperatorsId" });
            DropIndex("dbo.Reservations", new[] { "ItemId" });
            DropIndex("dbo.Reservations", new[] { "Client" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Transactions");
            DropTable("dbo.TransactionDetails");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Operators");
            DropTable("dbo.ItemCategories");
            DropTable("dbo.Items");
            DropTable("dbo.Reservations");
            DropTable("dbo.Customers");
        }
    }
}
