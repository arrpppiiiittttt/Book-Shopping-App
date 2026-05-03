using Dapper;
using ecommproject2.DataAccess.Data;
using ecommproject2.DataAccess.Repository.IRepository;
using ecommproject2.Models;
using ecommproject2.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecommproject2.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController (IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            //return Json(new { data = _unitOfWork.CoverType.GetAll() });
            return Json(new { data = _unitOfWork.SP_CAll.List<CoverType>(SD.SP_GetCoverTypes )});
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            DynamicParameters dynamic = new DynamicParameters();
            dynamic.Add("id", id);
            //var coverTypeInDb = _unitOfWork.CoverType.Get(id);
            var coverTypeInDb = _unitOfWork.SP_CAll.
                OneRecord<CoverType>(SD.SP_GetCoverType, dynamic);
            if (coverTypeInDb == null)
                return Json(new { success = false, message = "Something went wrong while deleting data !!!" });
            _unitOfWork.SP_CAll.Execute(SD.SP_DeleteCoverType, dynamic);
            //_unitOfWork.CoverType.Remove(coverTypeInDb);
            //_unitOfWork.Save();
            return Json(new { success = true, message = "Data deleted successfully!!!" });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();
            if (id == null) return View(coverType);
            DynamicParameters p = new DynamicParameters();
            p.Add("id", id.GetValueOrDefault());
            coverType = _unitOfWork.SP_CAll.OneRecord<CoverType>(SD.SP_GetCoverType, p);
            //coverType = _unitOfWork.CoverType.Get(id.GetValueOrDefault());

            if (coverType == null) return NotFound();
            return View(coverType);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert (CoverType coverType)
        {
            if (coverType == null) return NotFound();
            if (!ModelState.IsValid) return View(coverType);
            DynamicParameters dynamic = new DynamicParameters();
            dynamic.Add("name", coverType.Name);
            if (coverType.Id == 0)
                //_unitOfWork.CoverType.Add(coverType);
                _unitOfWork.SP_CAll.Execute(SD.SP_CreateCoverType, dynamic);
            else
            {
                dynamic.Add("id", coverType.Id);
                //_unitOfWork.CoverType.Update(coverType);
                _unitOfWork.SP_CAll.Execute(SD.SP_UpdateCoverType, dynamic);
            }
               
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
