using System.Security.Cryptography.X509Certificates;
using ZeroDocSigner.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(_ =>
{
    using var storage = new X509Store();
    storage.Open(OpenFlags.ReadOnly);
    var cert = storage.Certificates.First();
    storage.Close();
    return cert;
});

builder.Services.AddControllers();
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
