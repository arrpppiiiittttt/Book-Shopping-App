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
    public class OrderDetailRepository:Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderDetailRepository(ApplicationDbContext context):base(context)
        {
            _context = context;   
        }
    }
}
