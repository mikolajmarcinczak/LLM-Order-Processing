namespace OrderProcessor.Domain.Entities;

public class ExtractedOrderItem
{
  public string ProductName { get; set; }
  public int Quantity { get; set; }
  public decimal Price { get; set; }
}