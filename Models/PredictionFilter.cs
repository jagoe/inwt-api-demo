using System.ComponentModel;

public struct PredictionFilter
{
  /// <summary>
  ///
  /// </summary>
  [Description("Start of the time frame")]
  public DateTime? From { get; set; }
  /// <summary>
  ///
  /// </summary>
  [Description("End of the time frame")]
  public DateTime? To { get; set; }
  /// <summary>
  ///
  /// </summary>
  [Description("Group results by either `movie` or `region`")]
  public string? GroupBy { get; set; }
  /// <summary>
  ///
  /// </summary>
  [Description("Return results for given movie (`1` through `10`)")]
  public int? MovieId { get; set; }
  /// <summary>
  ///
  /// </summary>
  [Description("Return results for given region (one of `de`, `es`, `fr`, `gb`, `it`, `nl`)")]
  public string? Region { get; set; }
}
