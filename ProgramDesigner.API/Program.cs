using Microsoft.EntityFrameworkCore;
using ProgramDesigner.Application.ServicesExtension;
using ProgramDesigner.Domain.Services;
using ProgramDesigner.Infrastructure.Data.Context;
using ProgramDesigner.Infrastructure.SeedDataProvider;
using ProgramDesigner.Infrastructure.ServicesExtension;
using Scalar.AspNetCore;

namespace ProgramDesigner.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.UnmappedMemberHandling =
                    System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow;
            });
            builder.Services.AddOpenApi();

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication();

            builder.Services.AddScoped<IPrerequisiteValidationService, PrerequisiteValidationService>();
            builder.Services.AddScoped<IProgramSimulationService, ProgramSimulationService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            using (IServiceScope scope = app.Services.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Database.MigrateAsync();
                await ComputerScienceSeeder.SeedAsync(dbContext);
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}