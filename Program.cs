

using Amazon;
using Amazon.Runtime;



var builder = WebApplication.CreateBuilder(args);

// Base de datos (PostgreSQL)
builder.Services.AddDbContext<EventsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Mapster
builder.Services.AddMapster();

builder.Services.AddHttpClient<MediaServerStats>(client => client.Timeout = TimeSpan.FromSeconds(3));
builder.Services.AddHostedService<StreamSyncService>();

// JWT + políticas de autorización
builder.Services.AddJwtSecurity();

// Configuración de AWS (llega por variables de entorno en el compose)
builder.Services.Configure<AwsOptions>(builder.Configuration.GetSection(AwsOptions.SectionName));

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var options = builder.Configuration.GetSection(AwsOptions.SectionName).Get<AwsOptions>()!;
    var credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey);
    var config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region) };
    return new AmazonS3Client(credentials, config);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCarter();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Manejo global de excepciones
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

app.Run();