using GymTrackerDataModel;
using GymTrackerDbUow.Generic;
using GymTrackerDbUOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerBusinessService.Generic
{
    public class GenericRepoService<T> : IGenericRepoService<T> where T : class
    {
        private readonly IGenericRepo<T> _dbRepo;
        public GenericRepoService(EntityDBContext dbContext)
        {
            _dbRepo = new GenericRepo<T>(dbContext);
        }
        public Task DeleteAsync(object id)
        {
            return _dbRepo.DeleteAsync(id);
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return _dbRepo.GetAllAsync();
        }

        public Task<T> GetByIdAsync(object id)
        {
            return _dbRepo.GetByIdAsync(id);
        }

        public Task<int> InsertAsync(T obj)
        {
            return _dbRepo.InsertAsync(obj);
        }

        public Task UpdateAsync(T obj)
        {
            return _dbRepo.UpdateAsync(obj);
        }
    }
}
