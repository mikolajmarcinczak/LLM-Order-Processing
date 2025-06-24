using OrderProcessor.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderProcessor.Application.Interfaces;

public interface ILanguageModelService
{
  Task<List<ExtractedOrderItem>> ExtractOrderDetailsAsync(string emailContent);
}