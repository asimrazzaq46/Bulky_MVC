using Bulky.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repositery.IRepositery;

public interface IProductRepositery : IRepositery<Product>
{

    void update(Product obj);

}
