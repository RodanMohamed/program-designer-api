using ProgramDesigner.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using ProgramDesigner.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Application.ServicesExtension
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IProgramService, ProgramService>();
            return services;
        }
    }
}

