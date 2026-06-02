using GymTrackerBusinessService.Generic;
using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerBusinessService.Repository
{
    public class EmptyGridModel
    {
    }
    public interface IExerciseService
    {
        Task SaveExerciseData(Exercise exercise);
    }

    public class ExerciseService : IExerciseService
    {
        protected DbContextOptionsBuilder<EntityDBContext>? _optionsBuilder;
        public async Task SaveExerciseData(Exercise exercise)
        {
            _optionsBuilder = Helpers.BuildOptions();
            IGenericRepoService<Exercise> genericRepoService = new GenericRepoService<Exercise>(new EntityDBContext(_optionsBuilder.Options));
            exercise.ExerciseType = null;
            if (exercise.Id == 0)
            {
                await genericRepoService.InsertAsync(exercise);
            }
            else
            {
                await genericRepoService.UpdateAsync(exercise);
            }
        }

    }

}
