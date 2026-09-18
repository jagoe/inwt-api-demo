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

app.UseHttpsRedirection();

app.MapGet("/predictions/load",
([AsParameters] PredictionFilter filter) =>
{

    var predictions = DummyPredictions.Predictions.AsEnumerable();

    if (filter.From is not null)
    {
        predictions = predictions.Where(p => p.From >= filter.From);
    }

    if (filter.To is not null)
    {
        predictions = predictions.Where(p => p.To <= filter.To);
    }

    if (filter.MovieId is not null)
    {
        predictions = predictions.Where(p => p.MovieId == filter.MovieId);
    }

    if (filter.Region is not null)
    {
        predictions = predictions.Where(p => p.Region == filter.Region);
    }

    if (filter.GroupBy is not null)
    {
        switch (filter.GroupBy)
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
.WithName("Load predictions")
.WithDescription("Return predicted load (simultaneous viewers) within a 24 hour window.")
.Produces(200, typeof(PredictionDto))
.Produces(400)
.AddEndpointFilter(async (invocationContext, next) =>
    {
        var filter = invocationContext.GetArgument<PredictionFilter>(0);

        if (filter.To < filter.From)
        {
            return Results.BadRequest(@"""to"" must be greater than ""from"".");
        }

        if (filter.GroupBy is not null && filter.GroupBy != "movie" && filter.GroupBy != "region")
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
