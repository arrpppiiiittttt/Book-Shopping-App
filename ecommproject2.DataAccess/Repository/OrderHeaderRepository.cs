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
    public class OrderHeaderRepository:Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderHeaderRepository(ApplicationDbContext context):base(context) 
        {
            _context = context;
        }
    }
}
