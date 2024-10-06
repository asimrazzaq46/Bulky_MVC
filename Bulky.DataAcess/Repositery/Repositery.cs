using Bulky.DataAcess.Repositery.IRepositery;
using BulkyWeb.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Bulky.DataAcess.Repositery;

public class Repositery<T> : IRepositery<T> where T : class
{

    private readonly ApplicationDbContext _db;
    internal DbSet<T> dbSet;
    public Repositery(ApplicationDbContext db)
    {
        _db = db;
        this.dbSet = _db.Set<T>();
        _db.Products.Include(u => u.Category);
    }

    public void Add(T entity)
    {
        dbSet.Add(entity);
    }

    public void Remove(T entity)
    {
        dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entity)
    {
        dbSet.RemoveRange(entity);
    }

    public T GetOne(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
    {
        IQueryable<T> query;
        if (tracked)
        {
            query = dbSet;
        }
        else
        {
             query = dbSet.AsNoTracking();
        }

        query = query.Where(filter);
        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(property);
            }
        }
        return query.FirstOrDefault();
    }

    // get the include properties like this e.g Category,CategoryId,CoverType...with "," if there are many properties 
    public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeProperties = null)
    {
        IQueryable<T> query = dbSet;
        if(filter != null)
        {
        query = query.Where(filter);
        }
        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(property);
            }
        }

        return query.ToList();
    }


}
