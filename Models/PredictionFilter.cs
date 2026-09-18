public record PredictionFilter
{
  public DateTime? From { get; set; }
  public DateTime? To { get; set; }
  public string? GroupBy { get; set; }
  public int? MovieId { get; set; }
  public string? Region { get; set; }
}
