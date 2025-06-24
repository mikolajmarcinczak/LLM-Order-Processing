using OrderProcessor.Domain.Entities;
using OrderProcessor.Domain.Interfaces;
using MimeKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using OrderProcessor.Application.Interfaces;

namespace OrderProcessor.Application.Services;

public class OrderProcessingService
{
  private readonly IMAPEmailService _emailService;
  private readonly ILanguageModelService _languageModelService;
  private readonly IOrderRepository _orderRepository;

  public OrderProcessingService(IMAPEmailService emailService, ILanguageModelService languageModelService, IOrderRepository orderRepository)
  {
    _emailService = emailService;
    _languageModelService = languageModelService;
    _orderRepository = orderRepository;
  }

  public async Task ProcessNewOrdersAsync()
  {
    var unreadEmails = await _emailService.FetchUnreadEmailsAsync();

    foreach (var email in unreadEmails)
    {
      var order = new Order
      {
        EmailContent = email.TextBody,
        ReceivedDate = email.Date.DateTime,
        AttachmentEml = GetEmlAttachment(email)
      };

      var extractedItems = await _languageModelService.ExtractOrderDetailsAsync(email.TextBody);

      foreach (var item in extractedItems)
      {
        order.OrderItems.Add(new OrderItem
        {
          ProductName = item.ProductName,
          Quantity = item.Quantity,
          Price = item.Price
        });
      }

      await _orderRepository.AddOrderAsync(order);
    }
  }

  public async Task<IEnumerable<Order>> GetAllOrdersAsync()
  {
    return await _orderRepository.GetAllOrdersAsync();
  }

  public async Task ProcessLocalEmlFilesAsync(IEnumerable<string> filePaths)
  {
    foreach (var filePath in filePaths)
    {
      MimeMessage message;
      using (var stream = File.OpenRead(filePath))
      {
        message = await MimeMessage.LoadAsync(stream);
      }

      var order = new Order
      {
        EmailContent = message.TextBody,
        ReceivedDate = message.Date.DateTime,
        AttachmentEml = GetEmlAttachment(message)
      };

      var extractedItems = await _languageModelService.ExtractOrderDetailsAsync(message.TextBody);

      foreach (var item in extractedItems)
      {
        order.OrderItems.Add(new OrderItem
        {
          ProductName = item.ProductName,
          Quantity = item.Quantity,
          Price = item.Price
        });
      }

      await _orderRepository.AddOrderAsync(order);
    }
  }

  private string GetEmlAttachment(MimeMessage message)
  {
    foreach (var attachment in message.Attachments)
    {
      if (attachment is MimePart mimePart && mimePart.ContentType.IsMimeType("message", "rfc822"))
      {
        using (var stream = new MemoryStream())
        {
          mimePart.Content.DecodeTo(stream);
          stream.Position = 0;
          using (var reader = new StreamReader(stream))
          {
            return reader.ReadToEnd();
          }
        }
      }
    }
    return null;
  }
}