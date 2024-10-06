using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repositery.IRepositery;

public interface IRepositery<T> where T : class
{
    // T - Category, Product , Users

    IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
    T GetOne(Expression<Func<T,bool>> filter, string? includeProperties = null, bool tracked = false);
    void Add(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entity);



}
