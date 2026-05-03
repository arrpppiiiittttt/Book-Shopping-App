using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ecommproject2.Utility
{
    public static class SD
    {
        //SP For CoverTypes
        public const string SP_GetCoverTypes = "sp_GetCoverTypes";
        public const string SP_GetCoverType = "sp_GetCoverType";
        public const string SP_CreateCoverType = "sp_CreateCoverType";
        public const string SP_UpdateCoverType = "sp_UpdateCoverType";
        public const string SP_DeleteCoverType = "sp_DeleteCoverType";
        //SP For Categories
        public const string SP_GetCategories = "sp_GetCategories";
        public const string SP_GetCategory = "sp_GetCategory";
        public const string SP_CreateCategory = "sp_CreateCategory";
        public const string SP_UpdateCategory = "sp_UpdateCategory";
        public const string SP_DeleteCategory = "sp_DeleteCategory";
        //roles
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee User";
        public const string Role_Company = "Company User";
        public const string Role_Individual = "Individual User";
        //Order Status 
        public const string OrderStatusPending = "Pending";
        public const string OrderStatusApproved = "Approved";
        public const string OrderStatusInProgress = "Processing";
        public const string OrderStatusShipped = "Shipped";
        public const string OrderStatusCancelled = "Cancelled";
        public const string OrderStatusRefunded = "Refunded";

        //Payment Status
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentDelayPayment = "PaymentStatusDelay";
        public const string PaymentStatusRejected = "Rejected";
        public const string PaymentStatusRefunded = "Refunded";

        //Session
        public const string Ss_CartSessionCount = "Cart Count Session";

        public static double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity < 50)
                return price;
            else if(quantity<100)
                return price50;return price100;
        }
    }
}
