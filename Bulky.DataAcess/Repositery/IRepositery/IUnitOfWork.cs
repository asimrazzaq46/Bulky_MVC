using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repositery.IRepositery;

public interface IUnitOfWork
{
    ICategoryRepositery Category { get; }
    IProductRepositery Product { get; }

    ICompanyRepositery Company { get; }
    IShoppingCartRepositery ShoppingCart { get; }
    IApplicationUserRepositery User { get; }
    IOrderDetailRepositery OrderDetail { get; }
    IOrderHeaderRepositery OrderHeader { get; }
    void Save();
}
