using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using GymTrackerDataModel.Models;
using GymTrackerDataModel;
using Microsoft.Extensions.DependencyInjection;
using GymTrackerBusinessService.Generic;
using System.Collections;
using System.Xml.Linq;
using GymTrackerBusinessService.Repository;

namespace GymTrackerHelpers
{
    public static class ReflectionHelper
    {
        public static string GetDisplayName(PropertyInfo prop)
        {
            var displayNameAttr = prop.GetCustomAttribute<DisplayNameAttribute>();
            if (!string.IsNullOrWhiteSpace(displayNameAttr?.DisplayName))
            {
                return displayNameAttr.DisplayName;
            }
            else if (displayNameAttr?.DisplayName == "")
            {
                return "";
            }

            return prop.Name;
        }

        public static string GetDisplayName(object obj, PropertyInfo prop)
        {
            if (obj is IDynamicDisplayName dynamicDisplay)
            {
                return dynamicDisplay.GetDisplayName(prop);
            }

            return GetDisplayName(prop);
        }
        public static object? GetValue<TEntity>(TEntity item, PropertyInfo prop)
        {
            var value = prop.GetValue(item);

            if (value is DateTime dt)
                return dt.ToString("yyyy-MM-dd");

            // Handle navigation property
            if (prop.GetCustomAttribute<NavigationPropertyAttribute>() != null)
            {
                // Try to get "Description" or "Name"
                var navProp = value.GetType().GetProperty("Description")
                           ?? value.GetType().GetProperty("Name");

                if (navProp != null)
                    return navProp.GetValue(value)?.ToString();

                return value.ToString();
            }

            return value ?? "";
        }
        public static string? GetEmbeddedUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            if (url.Contains("youtube.com/watch"))
            {
                var uri = new Uri(url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var videoId = query["v"];
                return $"https://www.youtube.com/embed/{videoId}";
            }

            if (url.Contains("youtu.be"))
            {
                var videoId = url.Split('/').Last();
                return $"https://www.youtube.com/embed/{videoId}";
            }

            return url;
        }
        public static LambdaExpression GetExpression(PropertyInfo prop, object model)
        {
            var modelExpr = Expression.Constant(model);
            var propertyExpr = Expression.Property(modelExpr, prop);

            var delegateType = typeof(Func<>).MakeGenericType(prop.PropertyType);

            return Expression.Lambda(delegateType, propertyExpr);
        }
        public static PropertyInfo? GetForeignKey<TEntity>(PropertyInfo navProp)
        {
            var fkName = navProp.Name + "Id";
            return typeof(TEntity).GetProperty(fkName);
        }
        public static PropertyInfo? GetForeignKeyGrid<TGridEntity>(PropertyInfo navProp)
        {
            var fkName = navProp.Name + "Id";
            return typeof(TGridEntity).GetProperty(fkName);
        }
        public static PropertyInfo? GetForeignKeyGrid(PropertyInfo navProp)
        {
            var fkName = navProp.Name + "Id";
            return navProp.DeclaringType?.GetProperty(fkName);
        }
        public static async Task<IEnumerable<object>> GetNavigationData(PropertyInfo prop, IServiceProvider serviceProvider)
        {
            var attr = prop.GetCustomAttribute<NavigationPropertyAttribute>();
            if (attr == null) return Enumerable.Empty<object>();

            var dbContext = serviceProvider.GetRequiredService<EntityDBContext>();

            var set = dbContext.GetType()
                .GetMethod("Set", Type.EmptyTypes)!
                .MakeGenericMethod(attr.EntityType)
                .Invoke(dbContext, null);

            var toListAsync = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .First(m => m.Name == "ToListAsync" && m.GetParameters().Length == 2)
                .MakeGenericMethod(attr.EntityType);

            var task = (Task)toListAsync.Invoke(null, new object[] { set, CancellationToken.None })!;
            await task.ConfigureAwait(false);

            var result = task.GetType().GetProperty("Result")!.GetValue(task);
            return (IEnumerable<object>)result!;
            ////creates something like IGenericRepoService<ExerciseType> - exact repo type needed for the navigation property
            //var repoType = typeof(IGenericRepoService<>).MakeGenericType(prop.PropertyType);
            ////Get the actual repo instance from DI - This returns object, because the type is only known at runtime
            //var repoInstance = serviceProvider.GetService(repoType);
            ////Find the method called GetAllAsync on this repo
            //var method = repoType.GetMethod("GetAllAsync");

            //// Invoke method → returns Task<T>
            ////The result is: e.g. Task<IEnumerable<ExerciseType>> You cast it to:Task because you don’t know T
            //var task = (Task)method.Invoke(repoInstance, null)!;

            //// Await the task: This waits for the database call to complete.
            //await task;

            //// Get the Result property from Task<T>
            //var resultProperty = task.GetType().GetProperty("Result");

            //var result = resultProperty!.GetValue(task);

            //// Cast to IEnumerable and then to object
            //return ((IEnumerable)result!).Cast<object>();
        }
        public static object FormatGridValue(object item, PropertyInfo prop)
        {
            var value = prop.GetValue(item);

            if (value is DateTime dt)
                return dt.ToString("yyyy-MM-dd");

            return value ?? null;
        }
        public static async Task<IEnumerable<int>> SearchNavData(PropertyInfo prop, string value, IServiceProvider serviceProvider, CancellationToken token)
        {
            var items = await GetNavigationData(prop, serviceProvider);
            IEnumerable<int> result = null;
            if (!string.IsNullOrEmpty(value))
            {
                result = items
                   .Where(x =>
                   {
                       var text = GetDisplayProperty(x)?.ToLower();
                       return string.IsNullOrWhiteSpace(value) || text.Contains(value.ToLower());
                   })
                   .Select(x => (int)x.GetType().GetProperty("Id")!.GetValue(x)!);
            }
            else
            {
                result = items.Select(x => (int)x.GetType().GetProperty("Id")!.GetValue(x)!);
            }

            return await Task.FromResult(result);
        }
        public static string GetDisplayProperty(object item)
        {
            var prop = item.GetType().GetProperty("Description")
                   ?? item.GetType().GetProperty("Name");

            return prop?.GetValue(item)?.ToString() ?? "";
        }
        public static void SetGridValue(PropertyInfo prop, object? value, object? item)
        {
            prop.SetValue(item, value);
        }
        public static object FormatDetailGridValue(object item, PropertyInfo prop)
        {
            var value = prop.GetValue(item);
            return value ?? null;
        }
        public static string FormatDetailValue(object item, PropertyInfo prop)
        {
            var value = prop.GetValue(item);

            if (value is DateTime dt)
                return dt.ToString("yyyy-MM-dd");

            return value?.ToString() ?? string.Empty;
        }
    }

}
