using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Saes.Models.Core
{
    public static class DbSetExtensions
    {
        public static void AddOrUpdateRange<T>(this DbSet<T> dbset, ICollection<T> entities) where T: class
        {
            var listForAddEntities = new List<T>();
            var listForUpdateEntities = new List<T>();

            foreach (T entity in entities)
            {
                var entry = dbset.Entry(entity);
                switch (entry.State)
                {
                    case EntityState.Detached:
                    case EntityState.Added:
                        listForAddEntities.Add(entity);
                        break;
                    case EntityState.Modified:
                        listForUpdateEntities.Add(entity);
                        break;
                    //For Unchanged nothing to do

                    default:
                        break;
                }
            }
            dbset.AddRange(listForAddEntities);
            dbset.UpdateRange(listForUpdateEntities);
        }

        public static void AddOrUpdate<T>(this DbSet<T> dbset, T entity) where T : class
        {
            switch (dbset.Entry(entity).State)
            {
                case EntityState.Detached:
                case EntityState.Added:
                    dbset.AddRange(entity);
                    break;
                case EntityState.Modified:
                    dbset.Update(entity);
                    break;
                //For Unchanged nothing to do

                default:
                    break;
            }

            
            
        }
    }
}
