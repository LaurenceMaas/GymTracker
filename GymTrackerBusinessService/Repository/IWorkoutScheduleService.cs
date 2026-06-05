using GymTrackerBusinessService.Generic;
using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerBusinessService.Repository
{
    public interface IWorkoutScheduleService
    {
        Task SaveWorkoutScheduleData(DOWWorkout dowWorkout,string userId);
        Task<List<DOWWorkout>> LoadDOWDataPerUser(string UserId);
    }

    public class WorkoutScheduleService : IWorkoutScheduleService
    {
        protected DbContextOptionsBuilder<EntityDBContext>? _optionsBuilder;
        private readonly AuthenticationStateProvider _authStateProvider;
        public async Task<List<DOWWorkout>> LoadDOWDataPerUser(string UserId)
        {
            _optionsBuilder = Helpers.BuildOptions();
            IGenericRepoService<DOWWorkout> genericRepoService = new GenericRepoService<DOWWorkout>(new EntityDBContext(_optionsBuilder.Options));
            return (await genericRepoService.GetAllAsync()).ToList().FindAll(x => x.UserId.ToString().Equals(UserId)).OrderBy(x => x.DayOfWeek).ToList();
        }

        public async Task SaveWorkoutScheduleData(DOWWorkout dowWorkout, string userId)
        {

            _optionsBuilder = Helpers.BuildOptions();
            IGenericRepoService<DOWWorkout> genericRepoService = new GenericRepoService<DOWWorkout>(new EntityDBContext(_optionsBuilder.Options));

            dowWorkout.WorkoutPeriod = null;
            dowWorkout.TemplateWorkout = null;
            dowWorkout.UserId = Guid.Parse(userId);
            if (dowWorkout.Id ==0)
            {
               await genericRepoService.InsertAsync(dowWorkout);
            }
            else
            {
                await genericRepoService.UpdateAsync(dowWorkout);
            }
        }
    }
}
