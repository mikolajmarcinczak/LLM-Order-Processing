using System;
using System.Collections.Generic;

namespace OrderProcessor.Domain.Entities;

public class Order
{
  public int Id { get; set; }
  public string? EmailContent { get; set; }
  public string? AttachmentEml { get; set; }
  public DateTime ReceivedDate { get; set; }
  public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}