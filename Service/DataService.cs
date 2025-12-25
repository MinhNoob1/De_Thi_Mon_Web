using DAL;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public static class DataService
    {
        public static IServiceCollection AddDataServices( this IServiceCollection services)
        {
            services.AddScoped<BaseDAL>();
            return services;
        }
    }
}
