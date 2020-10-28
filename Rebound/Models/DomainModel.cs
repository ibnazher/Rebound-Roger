using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Rebound.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        [Display(Name ="First Name"),Required]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Phone { get; set; }
        [ MinLength(6), MaxLength(18)]
        public string Mobile { get; set; }
        public int Code { get; set; }
        public int? CodeNumber { get; set; }
        public bool Blacklist { get; set; }
        [DataType(DataType.Date),Display(Name ="Date of Birth")]
        public DateTime? DatrOfBirth { get; set; }
        public string Nationality { get; set; }
        public string City { get; set; }
        public string Street { get; set; }

        [DataType(DataType.MultilineText)]
        public string Address { get; set; }
        [Display(Name = "Mode of Payment")]
        public PaymentMode? PaymentType { get; set; }
        public decimal? Balance { get; set; }
        [DataType(DataType.MultilineText)]
        public string Note { get; set; }
        public bool Status { get; set; }
        public string FullName => FirstName + " " + LastName;

        public virtual ICollection<Reservation> Reservation { get; set; }
        public virtual ICollection<Transaction> Transaction { get; set; }


    }
    public class Reservation
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Started From")]
        public DateTime StartedAt { get; set; }
        [Display(Name = "To End")]
        public DateTime EndAt { get; set; }
        public string Note { get; set; }
        [DataType(DataType.Date)]
        public DateTime CreateDate { get; set; }
        [Display(Name = "Approve Date"), DataType(DataType.Date)]
        public DateTime? ApproveDate { get; set; }
        [Display(Name = "Cancel Date"), DataType(DataType.Date)]
        public DateTime? CancelDate { get; set; }
        [Display(Name = "Cancel Date"), DataType(DataType.Date)]
        public string CreateUser { get; set; }
        [Display(Name = "Update User")]
        public string UpdateUser { get; set; }
        [Display(Name = "Approve User")]
        public string ApproveUser { get; set; }
        [Display(Name = "Cancel User")]
        public string CancelUser { get; set; }
        [Display(Name = "Cancel Note")]
        public string CancelNote { get; set; }
        [Display(Name = "Billing Status")]
        public BillingStatus BillingStatus { get; set; }

        public bool Status { get; set; }

        public string Color { get; set; }
        public bool IsFullDay { get; set; }
        public bool IsBallRent { get; set; }
        public string ScheduleTitle { get; set; }
        public string ScheduleNote { get; set; }
        public decimal Price { get; set; }
        [Display(Name = "Paid Price")]
        public decimal? PaidPrice { get; set; }
        [Display(Name = "Extra Item Price")]
        public decimal? ExtraitemPrice { get; set; }
        public decimal TotalPrice => Convert.ToDecimal(Price) + Convert.ToDecimal(ExtraitemPrice);
        public decimal DuePrice => TotalPrice - Convert.ToDecimal(PaidPrice);

        [ForeignKey("Customer")]
        public int Client { get; set; }
        public virtual Customer Customer { get; set; }

        [ForeignKey("Item")]
        [Display(Name = "Item")]
        public int ItemId { get; set; }
        public virtual Item Item { get; set; }

        [ForeignKey("Operators")]
        [Display(Name = "Operators")]
        public int? OperatorsId { get; set; }
        public virtual Operators Operators { get; set; }
        public virtual ICollection<Transaction> Transaction { get; set; }

        [NotMapped]
        public string ToDate { get; set; }
        [NotMapped]
        public string Fromdate { get; set; }
        [NotMapped]
        public string FirstName { get; set; }
        [NotMapped]
        public string LastName { get; set; }
        [NotMapped]
        public string Phone { get; set; }
        [NotMapped]
        public string Mobile { get; set; }
        [NotMapped]
        public bool AddNew { get; set; }
        [NotMapped]
        public bool Advancereservation { get; set; }
    }
    public class Operators
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public string FullName => FirstName + " " + LastName;

        public Gender Gender { get; set; }
        [EmailAddress]
        public string Email { get; set; }

        [MinLength(6), MaxLength(18)]
        public string Phone { get; set; }
        [MinLength(6), MaxLength(18)]
        public string Mobile { get; set; }
        public bool Status { get; set; }
        public string AppUserId { get; set; }
        public virtual ICollection<Reservation> Reservation { get; set; }
    }
    public class OperatorsVm
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        [EmailAddress]
        public string Email { get; set; }

        [MinLength(6), MaxLength(18)]
        public string Phone { get; set; }
        [MinLength(6), MaxLength(18)]
        public string Mobile { get; set; }
        public bool Status { get; set; }

        [NotMapped]
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
    public class ItemCategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Item> Item { get; set; }
    }
    public class Item
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        [Display(Name = "Short Description")]
        public string ShortDescription { get; set; }
        public bool Published { get; set; }

        [ForeignKey("ItemCategory")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public virtual ItemCategory ItemCategory { get; set; }
        public virtual ICollection<Reservation> Reservation { get; set; }
    }
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public long VoucherNo { get; set; }
        public string VoucherType { get; set; }
        public decimal TrasactionalAmount { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public string Narration { get; set; }
        public DateTime CreatedOn {get;set;}
        public DateTime TransactionDate { get; set; }

        [ForeignKey("Customer")]
        public int Client { get; set; }
        public virtual Customer Customer { get; set; }

        [ForeignKey("Reservation")]
        public int? ReservationId { get; set; }
        public virtual Reservation Reservation { get; set; }
    }
    public class Cashbook
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public long VoucherNo { get; set; }
        public string VoucherType { get; set; }
        public decimal TrasactionalAmount { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public string Narration { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime TransactionDate { get; set; }

        [ForeignKey("Customer")]
        public int Client { get; set; }
        public virtual Customer Customer { get; set; }

        [ForeignKey("Reservation")]
        public int? ReservationId { get; set; }
        public virtual Reservation Reservation { get; set; }
    }
    //public class Audit
    //{
    //    public long Id { get; set; }
    //    public string EntityName { get; set; }
    //    public string FieldName { get; set; }
    //    public long ItemId { get; set; }
    //    public string Action { get; set; }
    //    public DateTime ActionDate { get; set; }
    //    public string ActionBy { get; set; }
    //    public string OldData { get; set; }
    //    public string NewData { get; set; }
    //}
}