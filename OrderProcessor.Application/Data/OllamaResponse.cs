namespace OrderProcessor.Application.Data;

public class OllamaResponse
{
  public string Model { get; set; }
  public string CreatedAt { get; set; }
  public string Response { get; set; }
  public bool Done { get; set; }
}