using ZeroDocSigner.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddAllowingEverythingCors();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionLogger();
app.UseCors();
app.UseSwaggerWithUI(app.Environment);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
