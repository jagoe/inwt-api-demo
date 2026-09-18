# INWT API Demo

This app is a small demo of what an API endpoint for streaming service predictions might look like.

## Run

### Docker Compose

```bash
docker compose up
```

### `dotnet`

```bash
dotnet run
```

## Use

The API can be inspected using OpenAPI/Swagger on the `/swagger` endpoint.

The API has a single endpoint:

## `/predictions/load`

Return predicted load (simultaneous viewers) within a 24 hour window.

### Result

```typescript
{
  "from": string
  "to": string
  "movieIds": int[]
  "regions": string[]
  "viewers": int
}[]
```

### Parameters

* `?from={timestamp}`: Return only data within a time frame that begins after the given time.
* `?to={timestamp}`: Return only data within a time frame that begins before the given time.
* `?movieId={movieId}`: Return only data for the given movie
* `?region={region}`: Return only data for the given region

#### Formats

* `timestamp`: `string` of a date and time in the ISO 8601 format (e.g. `2020-01-01T12:00:00Z`)
* `movieId`: `int`, `1` through `10`
* `region`: `string`, one of `de`, `es`, `fr`, `gb`, `it`, `nl`
