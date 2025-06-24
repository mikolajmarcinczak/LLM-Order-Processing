using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace OrderProcessor.Infrastructure.Data;

public class OrderProcessorDbContextFactory : IDesignTimeDbContextFactory<OrderProcessorDbContext>
{
  public OrderProcessorDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<OrderProcessorDbContext>();

    #region Connection string variables
    var server = Environment.GetEnvironmentVariable("MYSQL_SERVER") ?? "mysql";
    var port = Environment.GetEnvironmentVariable("MYSQL_PORT") ?? "3306";
    var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "orderprocessor_db";
    var username = Environment.GetEnvironmentVariable("MYSQL_USERNAME") ?? "root";
    var password = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD") ?? "SecurePassword123!";
    #endregion

    var connectionString = $"Server={server};Port={port};Database={database};Uid={username};Pwd={password};";

    optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 32)));

    return new OrderProcessorDbContext(optionsBuilder.Options);
  }
}