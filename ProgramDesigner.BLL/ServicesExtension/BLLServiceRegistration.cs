using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ProgramDesigner.BLL.Manager.ProgramManagers;
using ProgramDesigner.BLL.Simulators.ProgramSimulationEngine;
using ProgramDesigner.BLL.Validators;
using ProgramDesigner.BLL.Validators.ProgramValidationEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.ServicesExtension
{
    public static class BLLServiceRegistration
    {
        public static IServiceCollection AddBLLServices(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<ProgramDesigner.BLL.Mappers.ProgramMappingProfile>();
            });
            services.AddValidatorsFromAssemblyContaining<CreateProgramDtoValidator>();
            services.AddScoped<IProgramValidationEngine, ProgramValidationEngine>();
            services.AddScoped<IProgramManager, ProgramManager>();
            services.AddScoped<IProgramSimulationEngine, ProgramSimulationEngine>();
            return services;
        }
    }
}
