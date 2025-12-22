using Amazon.DynamoDBv2;
using UrlShortener.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure AWS options
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);

builder.Services.AddAWSService<IAmazonDynamoDB>();

builder.Services.AddControllers();

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

builder.Services.AddScoped<IUrlService, UrlService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCors",
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.WithOrigins("http://127.0.0.1:5500")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
            else
            {
                policy.WithOrigins("https://d2wnt86s7rdbuv.cloudfront.net")
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
        });
});


var app = builder.Build();


app.UseCors("AllowCors");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "URL Shortener API Running");

//app.MapGet("/{code}", async (string code, IUrlService urlService) =>
//{
//    var data = await urlService.GetAsync(code);
//    if (data == null) return Results.NotFound();
//    return Results.Redirect(data.OriginalUrl);
//});

app.Run();
