var builder = WebApplication.CreateBuilder(args);

// YAHAN APNI SYNCFUSION LICENSE KEY PASTE KAREIN
// Aapko 7-din wali trial key ya permanent community key yahan daalni hai
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("eXJ6KUrOeuHJecZ/hJnJ773bKBXtOOmU2z54YTmBpQQdFgOb+2dgmAHh3fALu3AmcraXsxIBpfzx3zJWtPzFtA==");


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();