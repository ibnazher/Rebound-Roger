using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rebound.Models
{
    public enum PaymentMode
    {
        Cash, Monthly, Prepaid
    }
    public enum BillingStatus
    {
        Pending, Confirmed
    }
    public enum Gender
    {
        Male, Female
    }
}