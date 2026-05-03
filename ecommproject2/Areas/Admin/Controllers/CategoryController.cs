using Dapper;
using ecommproject2.DataAccess.Repository.IRepository;
using ecommproject2.Models;
using ecommproject2.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecommproject2.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        #region APIs
        public IActionResult GetAll()
        {
            //var categoryList = _unitOfWork.Category.GetAll();
            //return Json(new { data = categoryList });
            return Json(new { data = _unitOfWork.SP_CAll.List<Category>(SD.SP_GetCategories) });
        }
        [HttpDelete]
        public IActionResult Delete (int id)
        {
            DynamicParameters dynamic = new DynamicParameters();
            dynamic.Add("id", id);
            //var categoryInDb = _unitOfWork.Category.Get(id);
            var categoryInDb = _unitOfWork.SP_CAll.
               OneRecord<Category>(SD.SP_GetCategory, dynamic);
            if (categoryInDb == null)
                return Json(new { success = false, message = "Something went wrong while deleting data !!!" });
            _unitOfWork.SP_CAll.Execute(SD.SP_DeleteCategory, dynamic);
            //_unitOfWork.Category.Remove(categoryInDb);
            //_unitOfWork.Save();
            return Json(new { success = true, message = "Data deletd successfully!!!" });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            Category category = new Category();
            if (id == null) return View(category);
            DynamicParameters p = new DynamicParameters();
            p.Add("id", id.GetValueOrDefault());
            category = _unitOfWork.SP_CAll.OneRecord<Category>(SD.SP_GetCategory, p);
            
            //category = _unitOfWork.Category.Get(id.GetValueOrDefault());
            if (category == null) return NotFound();
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (category == null) return NotFound();
            if (!ModelState.IsValid) return View(category);
            DynamicParameters dynamic = new DynamicParameters();
            dynamic.Add("name", category.Name);

            if (category.Id == 0)
                //_unitOfWork.Category.Add(category);
                _unitOfWork.SP_CAll.Execute(SD.SP_CreateCategory, dynamic);
            else
            {
                dynamic.Add("id", category.Id);
                //_unitOfWork.Category.Update(category);
                _unitOfWork.SP_CAll.Execute(SD.SP_UpdateCategory, dynamic);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
