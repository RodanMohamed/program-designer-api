using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProgramDesigner.Domain.Repositories;
using ProgramDesigner.Infrastructure.Data.Context;
using ProgramDesigner.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Infrastructure.ServicesExtension
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ILearningProgramRepository, LearningProgramRepository>();

            return services;
        }
    }
}
