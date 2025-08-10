using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.EfCore.MultiProject.Data;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<LocalDbContext>(options =>
            options.UseSqlite("Data Source=local.db"));
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    
    Console.WriteLine("EfCore MultiProject sample - Database created successfully");
    Console.WriteLine("Check the Generated folder for MetaTypes generated files");
}
