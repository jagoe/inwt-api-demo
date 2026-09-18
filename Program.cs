using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();

app.UseHttpsRedirection();

app.MapGet("/predictions/load", ([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? groupBy, [FromQuery] int? movieId, [FromQuery] string? region) =>
{

    var predictions = DummyPredictions.Predictions.AsEnumerable();

    if (from is not null)
    {
        predictions = predictions.Where(p => p.From >= from);
    }

    if (to is not null)
    {
        predictions = predictions.Where(p => p.To <= to);
    }

    if (movieId is not null)
    {
        predictions = predictions.Where(p => p.MovieId == movieId);
    }

    if (region is not null)
    {
        predictions = predictions.Where(p => p.Region == region);
    }

    if (groupBy is not null)
    {
        switch (groupBy)
        {
            case "movie":
                return Results.Ok(predictions
                    .GroupBy(p => p.MovieId)
                    .Select(group => new PredictionDto
                    {
                        MovieIds = [group.Key],
                        Viewers = group.Sum(p => p.Viewers),
                        Regions = group.Select(p => p.Region).Distinct().ToArray(),
                        From = group.Min(p => p.From),
                        To = group.Max(p => p.To)
                    }));

            case "region":
                return Results.Ok(predictions
                    .GroupBy(p => p.Region)
                    .Select(group => new PredictionDto
                    {
                        Regions = [group.Key],
                        Viewers = group.Sum(p => p.Viewers),
                        MovieIds = group.Select(p => p.MovieId).Distinct().ToArray(),
                        From = group.Min(p => p.From),
                        To = group.Max(p => p.To)
                    }));

            default: return Results.BadRequest();
        }
    }

    return Results.Ok(predictions.Select(p => new PredictionDto
    {
        Regions = [p.Region],
        Viewers = p.Viewers,
        MovieIds = [p.MovieId],
        From = p.From,
        To = p.To,
    }));
})
.WithName("Load predictions").AddEndpointFilter(async (invocationContext, next) =>
    {
        var from = invocationContext.GetArgument<DateTime?>(0);
        var to = invocationContext.GetArgument<DateTime?>(1);
        var groupBy = invocationContext.GetArgument<string?>(2);

        if (to < from)
        {
            return Results.BadRequest(@"""to"" must be greater than ""from"".");
        }

        if (groupBy != "movie" && groupBy != "region")
        {
            return Results.BadRequest(@"""groupBy"" must be one of ""movie"" or ""region"".");
        }

        return await next(invocationContext);
    });

app.MapSwagger();
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("v1/swagger.json", "My API V1");
});

app.Run();
