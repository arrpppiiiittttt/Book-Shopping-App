using ecommproject2.DataAccess.Data;
using ecommproject2.DataAccess.Repository.IRepository;
using ecommproject2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ecommproject2.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository (ApplicationDbContext context): base(context)
        {
            _context = context;
        }
    }
}
