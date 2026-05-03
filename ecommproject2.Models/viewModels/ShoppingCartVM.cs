using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ecommproject2.Models.viewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable <ShoppingCart> ListCart { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<SelectListItem> AnotherAddress { get; set; }

        //property to hold the selected address ID
        public int? SelectedAddressId { get; set; }

    }
}
