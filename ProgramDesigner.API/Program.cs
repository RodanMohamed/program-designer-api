
using Microsoft.EntityFrameworkCore;
using ProgramDesigner.BLL.ServicesExtension;
using ProgramDesigner.DAL.Data.Context;
using ProgramDesigner.DAL.Data.Models;
using ProgramDesigner.DAL.SeedDataProvider;
using ProgramDesigner.DAL.ServicesExtension;
using Scalar.AspNetCore;

namespace ProgramDesigner.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDALServices(builder.Configuration);
            builder.Services.AddBLLServices();
            builder.Services.AddControllers();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }
            using (IServiceScope scope = app.Services.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await ComputerScienceSeeder.SeedAsync(dbContext);
            }
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
