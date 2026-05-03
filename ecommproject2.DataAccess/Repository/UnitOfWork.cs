using ecommproject2.DataAccess.Data;
using ecommproject2.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ecommproject2.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Category = new CategoryRepository(context);
            CoverType = new CoverTypeRepository(context);
            SP_CAll = new SP_CAll(context);
            Product = new ProductRepository(context);
            Company = new CompanyRepository(context);
            Applicationuser = new ApplicationUserRepository(context);
            ShoppingCart = new ShoppingCartRepository(context);
            OrderHeader = new OrderHeaderRepository(context);
            OrderDetail = new OrderDetailRepository(context);
        }
        public ICategoryRepository Category { private set; get; }

        public ICoverTypeRepository CoverType { private set; get; }
        public ISP_CAll SP_CAll { private set; get; }
        public IProductRepository ProductRepository { private set; get; }

        public IProductRepository Product { private set; get; }
        public ICompanyRepository Company { private set; get; }

        public IApplicationuserRepository Applicationuser { private set; get; }
        public IShoppingCartRepository ShoppingCart { private set; get; }
        public IOrderHeaderRepository OrderHeader { private set; get; }
        public IOrderDetailRepository OrderDetail { private set; get; }
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
