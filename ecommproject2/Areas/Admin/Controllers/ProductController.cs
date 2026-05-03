using ecommproject2.DataAccess.Repository.IRepository;
using ecommproject2.Models;
using ecommproject2.Models.viewModels;
using ecommproject2.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ecommproject2.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ProductController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TwilioService _twilioService;
        private readonly IEmailSender _emailSender;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, TwilioService twilioService, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _twilioService = twilioService;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitOfWork.Product.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var productInDb = _unitOfWork.Product.Get(id);
            if (productInDb == null)
                return Json(new { success = false, message = "Something went wrong while deleting data !!!" });
            _unitOfWork.Product.Remove(productInDb);
            _unitOfWork.Save();
            ////////////Image Deleted
            var imagePath = Path.Combine(webRootPath, productInDb.ImageUrl.Trim('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            return Json(new { success = true, message = "Data deleted successfully" });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString()
                })
            };
            if (id == null) return View(productVM);
            productVM.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
            if (productVM.Product == null) return NotFound();
            return View(productVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var webRootPath = _webHostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count() > 0)
                {
                    var fileName = Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(files[0].FileName);
                    var uploads = Path.Combine(webRootPath, @"images\products");
                    if (productVM.Product.Id != 0)
                    {
                        var imageExists = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageExists;
                    }
                    if (productVM.Product.ImageUrl != null)
                    {
                        var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.Trim('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }
                else
                {
                    if (productVM.Product.Id != 0)
                    {
                        var imageExists = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageExists;
                    }
                }
                if (productVM.Product.Id == 0)
                    _unitOfWork.Product.Add(productVM.Product);
                else
                    _unitOfWork.Product.Update(productVM.Product);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                productVM = new ProductVM()
                {
                    Product = new Product(),
                    CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()
                    }),
                    CoverTypeList = _unitOfWork.CoverType.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()
                    })
                };
                if (productVM.Product.Id != 0)
                {
                    productVM.Product = _unitOfWork.Product.Get(productVM.Product.Id);
                }
                return View(productVM);
            }
        }
        [Authorize(Roles = SD.Role_Admin)]
        //public IActionResult Discontinue()
        //{
        //    var products = _unitOfWork.Product.GetAll().Where(p => !p.IsDiscontinued).ToList();
        //    return View(products); // Make a view to list active products and give a "Discontinue" button
        //}

        [HttpGet]
        public IActionResult DiscontinueProduct()
        {
            var products = _unitOfWork.Product.GetAll().ToList();
            return View(products); 
        }
        [HttpPost]
        public async Task<IActionResult> DiscontinueProduct(int productId)
        {
            var product = _unitOfWork.Product.FirstOrDefault(p => p.Id == productId);

            if (product != null)
            {
                // Check if the product is in any shipped order
                var isInShippedOrder = _unitOfWork.OrderDetail
                    .GetAll(includeProperties: "OrderHeader")
                    .Any(od => od.ProductId == productId &&
                               od.OrderHeader.OrderStatus == SD.OrderStatusShipped);

                if (isInShippedOrder)
                {
                    product.IsDiscontinued = true;
                    _unitOfWork.Save();

                    // ✅ Send SMS and Call
                    string message = $"Product Discontinued: {product.Title} (ID: {product.Id}) has been discontinued.";
                    string toPhoneNumber = "+919816991300"; // ideally from your admin/contact settings or user record

                    _twilioService.SendSms(toPhoneNumber, message);

                    // Use TwiML bin URL or dynamic voice response (if implemented)
                    string twimlUrl = "https://handler.twilio.com/twiml/EH41d9b202266607ec42d627b76e5c2da7";
                    _twilioService.MakeCall(toPhoneNumber, twimlUrl);

                    //Gmail Notification
                    string userEmail = "admin@example.com"; // update to actual recipient
                    string subject = $"Product Discontinued: {product.Title}";

                    string body = $@"
                <h2>Product Discontinued Notification</h2>
                <p><strong>Product Title:</strong> {product.Title}</p>
                <p><strong>Product ID:</strong> {product.Id}</p>
                <p><strong>Author:</strong> {product.Author}</p>
                <p><strong>ISBN:</strong> {product.ISBN}</p>
                <p><strong>Description:</strong> {product.Description}</p>
                <p><strong>Price:</strong> ${product.Price}</p>
                <p>This product has been discontinued successfully from the catalog.</p>
                <p>Regards,<br/>E-Commerce App Team</p>";

                    await _emailSender.SendEmailAsync(userEmail, subject, body);
                    TempData["Success"] = "Product discontinued and notification sent.";
                }
                else
                {
                    TempData["Error"] = "Product is already in a shipped order and cannot be discontinued.";
                }
            }

            return RedirectToAction("DiscontinueProduct");
        }

        
    }

}

