// See https://aka.ms/new-console-template for more information
using GymTrackerBusinessService.Repository;
using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using GymTrackerHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.ComponentModel;


IConfigurationRoot _configuration;
DbContextOptionsBuilder<EntityDBContext> _optionsBuilder;
_configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
_optionsBuilder = new DbContextOptionsBuilder<EntityDBContext>();
_optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));

var db = new EntityDBContext(_optionsBuilder.Options);

//IWorkoutExerciseSetService test = new WorkoutExerciseSetService(db) ;
//List<WorkoutGroupVM> model = await test.GetHeaderDataAsync(1);
//Dictionary<object, List<WorkoutSetVM>> data = new Dictionary<object, List<WorkoutSetVM>>();
//foreach(WorkoutGroupVM wgvm in model)
//{
//    data.Add(wgvm, await test.GetDetailDataAsync(wgvm));
//}
//await test.SaveExerciseSetsData(data);
IWorkoutScheduleService test = new WorkoutScheduleService();
var result = test.LoadDOWDataPerUser("2b27464b-e66e-46eb-a533-b0b2b62719b6");
Console.WriteLine("Hello, World!" + result.Result.FirstOrDefault().SortOrder.ToString());