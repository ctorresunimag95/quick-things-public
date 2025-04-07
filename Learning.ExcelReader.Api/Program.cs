using Learning.ExcelReader.Api;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ExcelService>();

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapPost("test", async ([FromForm(Name = "Data")] IFormFile file,
    ExcelService excelService) =>
{
    // Validate the file extension
    var allowedExtensions = new[] { ".xlsx", ".xls" };
    var fileExtension = Path.GetExtension(file.FileName).ToLower();

    if (!allowedExtensions.Contains(fileExtension))
    {
        return Results.BadRequest("Invalid file format. Please upload an Excel file (.xlsx or .xls).");
    }

    var info = excelService.GetExcelInfo(file);

    return Results.Ok(info);
}).DisableAntiforgery();

app.Run();

