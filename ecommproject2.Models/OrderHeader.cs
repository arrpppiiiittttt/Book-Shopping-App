using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ecommproject2.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [Required]
        public ApplicationUser ApplicationUser { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public DateTime ShippingDate { get; set; }
        public double OrderTotal { get; set; }
        public string TrackingNumber { get; set; }
        public string Career {  get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PaymentDueDate { get; set; }
        public string TransactionId { get; set; }
        public string Name { get; set; }
        [Required]
        [Display(Name ="Street Address")]
        public string StreetAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State {  get; set; }
        [Required]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
    }
}
