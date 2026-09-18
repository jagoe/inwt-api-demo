public struct PredictionDto
{
  public DateTime From { get; set; }
  public DateTime To { get; set; }
  public int[] MovieIds { get; set; }
  public string[] Regions { get; set; }
  public int Viewers { get; set; }
}
