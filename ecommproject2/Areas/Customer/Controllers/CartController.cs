using ecommproject2.DataAccess.Data;
using ecommproject2.DataAccess.Repository.IRepository;
using ecommproject2.Models;
using ecommproject2.Models.viewModels;
using ecommproject2.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis;
using Stripe;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Twilio.TwiML;

namespace ecommproject2.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private static bool isEmailConfirm = false;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TwilioService _twilioService;
        public CartController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, IEmailSender emailSender, TwilioService twilioService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailSender = emailSender;
            _twilioService = twilioService;
        }
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null)
            {
                ShoppingCartVM = new ShoppingCartVM()
                {
                    ListCart = new List<ShoppingCart>()
                };
                return View(ShoppingCartVM);
            }
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.Applicationuser.FirstOrDefault(au => au.Id == claims.Value);
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                if (list.Product.Description.Length >= 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + "...";
                }
            }
            /////email confirm
            if (!isEmailConfirm)
            {
                ViewBag.EmailMessage = "Email has been sent kindly verify your email !";
                ViewBag.EmailCSS = "text-succcess";
                isEmailConfirm = false;
            }
            else
            {
                ViewBag.EmailMessage = "Email must be confirm for authorize customer !!!";
                ViewBag.EmailCSS = "text-danger";
            }
            return View(ShoppingCartVM);
        }
        public IActionResult plus(int id)
        {
            var cart = _unitOfWork.ShoppingCart.Get(id);
            cart.Count += 1;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult minus(int id)
        {
            var cart = _unitOfWork.ShoppingCart.Get(id);
            if (cart.Count == 1)
                cart.Count = 1;
            else
                cart.Count -= 1;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult delete(int id)
        {
            var cart = _unitOfWork.ShoppingCart.Get(id);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Summary(List<int> selectedIds, ShoppingCartVM ShoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var emailClaim = claimsIdentity.FindFirst(ClaimTypes.Email);
            string email = emailClaim.Value;
            var userEmail = _unitOfWork.Applicationuser.FirstOrDefault(u => u.Email == email);
            bool emailConfirmed = userEmail.EmailConfirmed;
            // Get current user
            var userId = claims.Value;
            // Get previously used unique addresses
            var previousOrders = _unitOfWork.OrderHeader
                .GetAll(o => o.ApplicationUserId == userId)
                .GroupBy(o => new
                {
                    o.Name,
                    o.StreetAddress,
                    o.City,
                    o.State,
                    o.PostalCode
                })
                .Select(g => g.First()) // Remove duplicates
                .Select(o => new SelectListItem
                {
                    Text = $"{o.Name}, {o.StreetAddress}, {o.City}, {o.State}, {o.PostalCode}",
                    Value = $"{o.Name},{o.PhoneNumber},{o.StreetAddress},{o.City},{o.State},{o.PostalCode}"

                }).ToList();
            var user = _unitOfWork.Applicationuser.FirstOrDefault(u => u.Id == userId);
            //Fetch selected cart items
            if (selectedIds == null || selectedIds.Count == 0)
            {
                ModelState.AddModelError("", "Please select at least one item to proceed.");
                return RedirectToAction(nameof(Index));
            }
            var selectedCarts = _unitOfWork.ShoppingCart.GetAll(
                sc => sc.ApplicationUserId == userId && selectedIds.Contains(sc.Id),
                includeProperties: "Product"
            ).ToList();
            ShoppingCartVM = new ShoppingCartVM
            {
                OrderHeader = new OrderHeader
                {
                    Name = user.Name,
                    StreetAddress = user.StreetAddress,
                    City = user.City,
                    State = user.State,
                    PostalCode = user.PostalCode,
                    PhoneNumber = user.PhoneNumber,
                    ApplicationUserId = user.Id,
                    ApplicationUser = user
                },
                ListCart = selectedCarts,
                AnotherAddress = previousOrders
            };
            if (emailConfirmed == false)
            {
                var userId1 = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId1, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                return RedirectToAction(nameof(Index));
            } 
            if (claims == null) return NotFound();
            // Calculate total and product descriptions
            ShoppingCartVM.OrderHeader.OrderTotal = 0; 
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                if (list.Product.Description.Length > 100)
                    list.Product.Description = list.Product.Description.Substring(0, 99) + "...";
            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("summary")]
        public async Task<IActionResult> summarypost(string stripeToken, ShoppingCartVM ShoppingCartVM, bool isVerifiedOtp = false)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var previousOrders = _unitOfWork.OrderHeader
            .GetAll(o => o.ApplicationUserId == userId)
            .Select(o => new SelectListItem
            {
                Text = $"{o.Name}, {o.StreetAddress}, {o.City}, {o.State}, {o.PostalCode}",
                //Value = o.Id.ToString()
                Value = $"{o.Name},{o.PhoneNumber},{o.StreetAddress},{o.City},{o.State},{o.PostalCode}"

            }).ToList();
            if (ShoppingCartVM.SelectedAddressId.HasValue)
            {
                var selectedOrder = _unitOfWork.OrderHeader.FirstOrDefault(o => o.Id == ShoppingCartVM.SelectedAddressId.Value);
                if (selectedOrder != null)
                {
                    // Copy address fields
                    ShoppingCartVM.OrderHeader.StreetAddress = selectedOrder.StreetAddress;
                    ShoppingCartVM.OrderHeader.City = selectedOrder.City;
                    ShoppingCartVM.OrderHeader.State = selectedOrder.State;
                    ShoppingCartVM.OrderHeader.PostalCode = selectedOrder.PostalCode;
                    ShoppingCartVM.OrderHeader.PhoneNumber = selectedOrder.PhoneNumber;
                    ShoppingCartVM.OrderHeader.Name = selectedOrder.Name;
                }
            }
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.Applicationuser.FirstOrDefault(au => au.Id == claims.Value);
            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product");
            ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = list.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = list.Price,
                    Count = list.Count,
                };
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();
            //session Count
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, 0);
            //*****
            //Stripe Payment
            if (stripeToken == null)
            {
                ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentDelayPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;   
            }
            else
            {
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal),
                    Currency = "usd",
                    Description = "OrderId:" + ShoppingCartVM.OrderHeader.Id.ToString(),
                    Source = stripeToken
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);
                if (charge.BalanceTransactionId == null)
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                else
                    ShoppingCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
                if (charge.Status.ToLower() == "succeeded")
                {
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                }   
                _unitOfWork.Save();
            }   
            //email for order confirmation
            var userEmail = ShoppingCartVM.OrderHeader.ApplicationUser.Email;
            var userName = ShoppingCartVM.OrderHeader.Name;
            // Build product list HTML
            string productDetails = "<ul>";
            foreach (var item in ShoppingCartVM.ListCart)
            {
                productDetails += $"<li>{item.Product.Title} - Quantity: {item.Count}</li>";
            }
            productDetails += "</ul>";
            // Build email subject and body
            string subject = $"Order Confirmation - Order #{ShoppingCartVM.OrderHeader.Id}";
            string body = $@"
            <h2>Thank you for your order, {userName}!</h2>
            <p>Your order number is <strong>{ShoppingCartVM.OrderHeader.Id}</strong>.</p>
            <p>Total Amount: <strong>${ShoppingCartVM.OrderHeader.OrderTotal}</strong></p>
            <p><strong>Shipping Address:</strong><br/>
                {ShoppingCartVM.OrderHeader.StreetAddress},<br/>
                {ShoppingCartVM.OrderHeader.City}, {ShoppingCartVM.OrderHeader.State}, {ShoppingCartVM.OrderHeader.PostalCode}</p>
            <p><strong>Order Details:</strong><br/>
                {productDetails}
            </p>
            <p>We will send you updates once your order is processed and shipped.</p>
            <p>Regards,<br/>E-Commerce App Team</p>";
             // Send email
            await _emailSender.SendEmailAsync(userEmail, subject, body);
            // Send SMS notification to the user
            if (!string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.PhoneNumber))
            {
                string smsMessage = $"Order Confirmation - Your order #{ShoppingCartVM.OrderHeader.Id}with " +
                    $"{productDetails}has been successfully placed. Total: ${ShoppingCartVM.OrderHeader.OrderTotal}. Thank you!";
                _twilioService.SendSms(ShoppingCartVM.OrderHeader.PhoneNumber, smsMessage);
            }
            _twilioService.MakeCall(ShoppingCartVM.OrderHeader.PhoneNumber, "Order placed successfully!");

            // Send call using Twilio
            string twimlUrl = "https://handler.twilio.com/twiml/EH41d9b202266607ec42d627b76e5c2da7";
            _twilioService.MakeCall(ShoppingCartVM.OrderHeader.PhoneNumber, twimlUrl);
            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        }
        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
        [HttpGet("twilio/voice")]
        public IActionResult Voice(string userName, string orderId, string total)
        {
            var response = new VoiceResponse();
            var message = $"Hello {userName}, your order number {orderId} has been confirmed. The total amount is {total} dollars." +
                $" Thank you for shopping with us!";
            response.Say(message, voice: "alice");

            return Content(response.ToString(), "text/xml");
        }
        public IActionResult SendSms()
        {
            _twilioService.SendSms("+919816991300", "Hello from Twilio!");

            return Content("SMS sent!");
        }
        [HttpPost]
        public IActionResult Reorder(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get old order details
            var oldOrderDetails = _unitOfWork.OrderDetail.GetAll(od => od.OrderHeaderId == orderId).ToList();

            if (!oldOrderDetails.Any())
            {
                return NotFound();
            }
            var existingCartItems = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == userId);
            _unitOfWork.ShoppingCart.RemoveRange(existingCartItems);
            _unitOfWork.Save();

            // Add products from old order to cart
            foreach (var item in oldOrderDetails)
            {
                var cartItem = new ShoppingCart
                {
                    ProductId = item.ProductId,
                    Count = item.Count,
                    ApplicationUserId = userId
                };
                _unitOfWork.ShoppingCart.Add(cartItem);
            }
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, oldOrderDetails.Count);
            return RedirectToAction("Summary", "Cart");
        }


    }
}

