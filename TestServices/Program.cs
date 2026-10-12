// See https://aka.ms/new-console-template for more information
using GymTrackerBusinessService.Repository;
using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using GymTrackerHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using TestServices;


IConfigurationRoot _configuration;
DbContextOptionsBuilder<EntityDBContext> _optionsBuilder;
_configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
_optionsBuilder = new DbContextOptionsBuilder<EntityDBContext>();
_optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));

var db = new EntityDBContext(_optionsBuilder.Options);

