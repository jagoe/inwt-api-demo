using System.ComponentModel;

[Description("Predicted viewer count within a time frame, for one or more movies and one or more viewer regions")]
public struct PredictionDto
{
  [Description("Beginning of the time frame")]
  public DateTime From { get; set; }

  [Description("End of the time frame")]
  public DateTime To { get; set; }

  [Description("Relevant movie IDs")]
  public int[] MovieIds { get; set; }

  [Description("Relevant regions")]
  public string[] Regions { get; set; }

  [Description("Total predicted viewer count")]
  public int Viewers { get; set; }
}
