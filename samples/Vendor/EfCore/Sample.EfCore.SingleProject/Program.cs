using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.EfCore.SingleProject.Data;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<SingleProjectDbContext>(options =>
            options.UseSqlite("Data Source=single.db"));
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SingleProjectDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    
    Console.WriteLine("EfCore SingleProject sample - Database created successfully");
    Console.WriteLine("Check the Generated folder for MetaTypes generated files");
}