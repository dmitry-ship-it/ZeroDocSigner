using ZeroDocSigner.Api;
using ZeroDocSigner.Api.Extensions;
using ZeroDocSigner.Common.V2.Services.Abstractions;
using ZeroDocSigner.Common.V2.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(CertificatesLoader.CertificateProvider);
builder.Services.AddSingleton<IOfficeDocumentService, OfficeDocumentService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionLogger();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
