using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ecommproject2.Models.viewModels;
using ecommproject2.DataAccess.Repository.IRepository;
using ecommproject2.DataAccess.Repository;
using Microsoft.AspNetCore.Hosting;
using ecommproject2.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ecommproject2.Utility;

namespace ecommproject2.Areas.Customer.Controllers;
[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        if (claims != null)
        {
            var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
        }
        var productList = _unitOfWork.Product.GetAll(includeProperties: "category,coverType");

        // Calculate Sale Count for each product
        var saleCounts = _unitOfWork.OrderDetail.GetAll()
            .GroupBy(od => od.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(od => od.Count));
        ViewBag.SaleCounts = saleCounts;

        return View(productList);
    }
    #region APIs
    [HttpGet]
    public IActionResult GetAll()
    {
        return Json(new { data = _unitOfWork.Product.GetAll(includeProperties: "category,coverType") });
    }
   
    #endregion

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public IActionResult Details (int id)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        if (claims != null)
        {
            var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
        }

        var productInDb = _unitOfWork.Product.FirstOrDefault(p => p.Id == id, includeProperties: "category,coverType");
        if (productInDb == null) return NotFound();
        var shoppingCart = new ShoppingCart()
        {
            Product = productInDb,
            ProductId = productInDb.Id,
        };
        return View(shoppingCart);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Details (ShoppingCart shoppingCart)
    {
        shoppingCart.Id = 0;
        if (ModelState.IsValid)
        {
            var ClaimIdentity = (ClaimsIdentity)(User.Identity);
            var claims = ClaimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return NotFound();
            shoppingCart.ApplicationUserId = claims.Value;
            var shoppingCartInDb = _unitOfWork.ShoppingCart.FirstOrDefault(sc => sc.ApplicationUserId == claims.Value && sc.ProductId == shoppingCart.ProductId);
            if (shoppingCartInDb == null)
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            else
                shoppingCartInDb.Count += shoppingCart.Count;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        else
        {
            var productInDb = _unitOfWork.Product.FirstOrDefault(p => p.Id == shoppingCart.Id, includeProperties: "category,coverType");
            if (productInDb == null) return NotFound();
            var shoppingCartEdit = new ShoppingCart()
            {
                Product = productInDb,
                ProductId = productInDb.Id,
            };
            return View(shoppingCartEdit);
        }
    }
}
