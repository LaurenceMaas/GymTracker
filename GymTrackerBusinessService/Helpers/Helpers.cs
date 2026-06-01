using GymTrackerDataModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerBusinessService
{
    public static class Helpers
    {
        public static DbContextOptionsBuilder<EntityDBContext> BuildOptions()
        {
            var _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
            var _optionsBuilder = new DbContextOptionsBuilder<EntityDBContext>();
            return _optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
        }
    }
}
