using Microsoft.EntityFrameworkCore;
using AddressBookApi.Data;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using AddressBookApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/addressbookapi.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddDbContext<AddressBookContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "yourIssuer",
            ValidAudience = "yourAudience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("yourSecretKey"))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Willkommen bei der Adressbuch-API");

app.MapGet("/personen", async (AddressBookContext db) =>
{
    return await db.Personen.Include(p => p.Adressen).ToListAsync();
});

app.MapGet("/personen/{id}", async (int id, AddressBookContext db) =>
{
    var person = await db.Personen.Include(p => p.Adressen).FirstOrDefaultAsync(p => p.Id == id);
    return person is not null ? Results.Ok(person) : Results.NotFound();
});

app.MapPost("/personen", async (Person person, AddressBookContext db) =>
{
    foreach (var adresse in person.Adressen)
    {
        adresse.Person = person;
    }

    db.Personen.Add(person);
    await db.SaveChangesAsync();
    return Results.Created($"/personen/{person.Id}", person);
});

app.MapPut("/personen/{id}", async (int id, Person inputPerson, AddressBookContext db) =>
{
    var person = await db.Personen.Include(p => p.Adressen).FirstOrDefaultAsync(p => p.Id == id);
    if (person is null) return Results.NotFound();

    person.Vorname = inputPerson.Vorname;
    person.Nachname = inputPerson.Nachname;
    person.Adressen = inputPerson.Adressen;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/personen/{id}", async (int id, AddressBookContext db) =>
{
    var person = await db.Personen.FindAsync(id);
    if (person is not null)
    {
        db.Personen.Remove(person);
        await db.SaveChangesAsync();
        return Results.Ok(person);
    }
    return Results.NotFound();
});

app.MapGet("/personen/suche", async (string name, AddressBookContext db) =>
{
    var lowerCaseName = name.ToLower();

    var personen = await db.Personen.Include(p => p.Adressen)
        .Where(p => p.Vorname.ToLower()==lowerCaseName || p.Nachname.ToLower()==lowerCaseName)
        .ToListAsync();

    return Results.Ok(personen);
});

app.Run();
