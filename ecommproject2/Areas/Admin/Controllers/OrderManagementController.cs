using ecommproject2.DataAccess.Repository.IRepository;
using ecommproject2.Models;
using ecommproject2.Models.viewModels;
using ecommproject2.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Climate;
using System.Security.Claims;

namespace ecommproject2.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class OrderManagementController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TwilioService _twilioService;
        public OrderManagementController(IUnitOfWork unitOfWork, TwilioService twilioService)
        {
            _unitOfWork = unitOfWork;
            _twilioService = twilioService;
        }
        public IActionResult Index(string status, DateTime? fromDate, DateTime? toDate)
        {
            IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

            if (!string.IsNullOrEmpty(status))
                orderHeaders = orderHeaders.Where(o => o.OrderStatus.ToLower() == status.ToLower());

            if (fromDate != null && toDate != null)
            {
                if (fromDate > toDate)
                    ModelState.AddModelError(string.Empty, "From date cannot be greater than To date.");
                else 
                    orderHeaders = orderHeaders.Where(o => o.OrderDate.Date >= fromDate.Value.Date && o.OrderDate.Date <= toDate.Value.Date);
            }
            return View(orderHeaders);  
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string newStatus, OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.FirstOrDefault(o => o.Id == orderId);
            if (orderHeader == null)
            {
                return NotFound();
            }
            if (newStatus == SD.OrderStatusRefunded)
            {
                await _twilioService.MakeRefundedCallAsync(orderHeader.PhoneNumber, orderId);
            }
            orderHeader.OrderStatus = newStatus;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index)); 
        }
        public IActionResult Details(int orderId)
        {
            var orderHeader = _unitOfWork.OrderHeader
                .FirstOrDefault(o => o.Id == orderId, includeProperties: "ApplicationUser");

            var orderDetails = _unitOfWork.OrderDetail
                .GetAll(od => od.Id == orderId, includeProperties: "Product");

            var viewModel = new OrderVM
            {
                OrderHeader = orderHeader,
                OrderDetails = orderDetails
            };

            return View(viewModel);
        }
        public IActionResult RecentOrders()
        {
            var recentOrders = _unitOfWork.OrderHeader
                .GetAll(includeProperties: "ApplicationUser")
                .OrderByDescending(o => o.OrderDate)
                .Take(30) // fetch latest 30 orders
                .ToList();

            return View(recentOrders);
        }


    }
}
