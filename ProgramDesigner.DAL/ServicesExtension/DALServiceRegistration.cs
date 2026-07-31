//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using ProgramDesigner.DAL.Data.Context;
//using ProgramDesigner.DAL.UnitOfWork;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ProgramDesigner.DAL.ServicesExtension
//{
//    public static class DALServiceRegistration
//    {
//        public static IServiceCollection AddDALServices(this IServiceCollection services, IConfiguration configuration)
//        {
//            string connectionString = configuration.GetConnectionString("DefaultConnection")!;

//            services.AddDbContext<ApplicationDbContext>(options =>
//            {
//                options.UseSqlServer(connectionString);
//            });

           
//            services.AddScoped<IUnitOfWork, DAL.UnitOfWork.UnitOfWork>();

//            return services;
//        }
//    }
//}
