using Microsoft.EntityFrameworkCore;
using GymTrackerDataModel;
using System.Collections.Generic;
using System.Diagnostics;
using GymTrackerDbUow.Generic;
using GymTrackerDataModel.Models;
using System.Reflection;

namespace GymTrackerDbUOW
{
    //The following GenericRepos class Implement the IGenericRepos Interface
    //And Here T is going to be a class
    //While Creating an Instance of the GenericRepos type, we need to specify the Class Name
    //That is we need to specify the actual class name of the type T
    public class GenericRepo<T> : IGenericRepo<T> where T : class
    {
        //The following variable is going to hold the EmployeeDBContext instance
        protected EntityDBContext _context = null;

        //The following Variable is going to hold the DbSet Entity
        protected DbSet<T> table = null;

        ////Using the Parameterless Constructor, 
        ////we are initializing the context object and table variable
        //public GenericRepo()
        //{
        //    this._context = new EntityDBContext();

        //    //Whatever class name we specify while creating the instance of GenericRepos
        //    //That class name will be stored in the table variable
        //    table = _context.Set<T>();
        //}

        //Using the Parameterized Constructor, 
        //we are initializing the context object and table variable
        public GenericRepo(EntityDBContext _context)
        {
            this._context = _context;
            table = _context.Set<T>();
        }

        //This method will return all the Records from the table
        //public async Task<IEnumerable<T>> GetAllAsync()
        //{
        //    return await table.ToListAsync();
        //}
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            IQueryable<T> query = _context.Set<T>();

            var navigationProps = typeof(T)
                .GetProperties()
                .Where(p => p.GetCustomAttribute<NavigationPropertyAttribute>() != null);

            foreach (var prop in navigationProps)
            {
                query = query.Include(prop.Name);
            }

            return await query.ToListAsync();
        }
        //This method will return the specified record from the table
        //based on the ID which it received as an argument
        public async Task<T> GetByIdAsync(object id)
        {
            return await table.FindAsync(id);
        }

        //This method will Insert one object into the table
        //It will receive the object as an argument which needs to be inserted into the database
        public async Task<int> InsertAsync(T obj)
        {
            try
            {
                // Marks the entity as Added
                table.Add(obj);
                await _context.SaveChangesAsync();


                // Assuming your entity has a property named "Id"
                var propertyInfo = obj.GetType().GetProperty("Id");
                if (propertyInfo != null)
                {
                    return (int)propertyInfo.GetValue(obj);
                }

                throw new InvalidOperationException("Entity does not have an Id property.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error inserting record: " + ex.Message);
                new InvalidOperationException("Unable to insert entity in db:" + ex.Message);
                return 0;
            }

        }

        //This method is going to update the record in the table
        //It will receive the object as an argument
        public async Task UpdateAsync(T obj)
        {
            try
            {
                var local = _context.Set<T>().Local
                .FirstOrDefault(e => GetPrimaryKeyValue(e).Equals(GetPrimaryKeyValue(obj)));

                if (local != null)
                {
                    // Detach the already tracked local entity
                    _context.Entry(local).State = EntityState.Detached;
                }

                // Attach the new object and mark it as modified
                _context.Set<T>().Attach(obj);
                _context.Entry(obj).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error updating record: " + ex.Message);
                new InvalidOperationException("Unable to insert entity in db:" + ex.Message);
            }
        }


        //This method is going to remove the record from the table
        //It will receive the primary key value as an argument whose information needs to be removed from the table
        public async Task DeleteAsync(object id)
        {
            try
            {
                //First, fetch the record from the table
                T existing = table.Find(id);
                //This will mark the Entity State as Deleted
                table.Remove(existing);

                await _context.SaveChangesAsync();
            } 
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }

        private object GetPrimaryKeyValue(T entity)
        {
            var entityType = _context.Model.FindEntityType(typeof(T));

            // Handles composite or single keys
            var key = entityType.FindPrimaryKey();

            if (key.Properties.Count == 1)
            {
                // Single primary key
                return entity.GetType()
                             .GetProperty(key.Properties[0].Name)
                             .GetValue(entity);
            }
            else
            {
                // Composite key → return anonymous object or array
                return key.Properties
                          .Select(p => entity.GetType().GetProperty(p.Name).GetValue(entity))
                          .ToArray();
            }
        }

    }
}