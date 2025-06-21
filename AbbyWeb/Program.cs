using Abby.DAL.Data;
using Abby.DAL.Repository;
using Abby.DAL.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, lc) => lc
       .WriteTo.Console().WriteTo.Debug()
       .ReadFrom.Configuration(ctx.Configuration));

    // Add services to the container.
    builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")
        ));

    builder.Services.AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

	// Logging
	Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
	builder.Services.AddLogging(x => x.AddSerilog());

	// Configure your DbContext (example using SQL Server)
	builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
	// We need this part for monitoring the app
	builder.Services.AddHealthChecks();
	var app = builder.Build();

	

	// --- Apply migrations on startup ---
	using (var scope = app.Services.CreateScope())
	{
		var services = scope.ServiceProvider;
		try
		{
			var context = services.GetRequiredService<ApplicationDbContext>();
			context.Database.Migrate(); // This is the magic line!
		}
		catch (Exception ex)
		{
			var logger = services.GetRequiredService<ILogger<Program>>();
			logger.LogError(ex, "An error occurred while migrating the database.");
			// You might want to rethrow or handle this more gracefully depending on your needs.
		}
	}
	// --- End apply migrations on startup ---


	// Build the app


	

    //Serilog
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();




	app.Run();
}
catch (Exception ex)
{

    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}




