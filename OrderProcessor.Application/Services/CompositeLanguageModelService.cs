using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderProcessor.Application.Interfaces;
using OrderProcessor.Domain.Entities;

namespace OrderProcessor.Application.Services;

public class CompositeLanguageModelService : ILanguageModelService
{
  private readonly LanguageModelService _primaryService;
  private readonly FallbackLanguageModelService _fallbackService;

  public CompositeLanguageModelService(LanguageModelService primaryService, FallbackLanguageModelService fallbackService)
  {
    _primaryService = primaryService;
    _fallbackService = fallbackService;
  }

  //  If OpenAI fails, use Ollama as language model
  public async Task<List<ExtractedOrderItem>> ExtractOrderDetailsAsync(string emailContent)
  {
    try
    {
      Console.WriteLine("Attempting to use primary OpenAI model...");
      var result = await _primaryService.ExtractOrderDetailsAsync(emailContent);
      Console.WriteLine($"Primary model successful, extracted {result.Count} items");
      return result;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error with primary language model: {ex.Message}");
      Console.WriteLine("Attempting to use fallback Ollama model...");

      try
      {
        var fallbackResult = await _fallbackService.ExtractOrderDetailsAsync(emailContent);
        Console.WriteLine($"Fallback model successful, extracted {fallbackResult.Count} items");
        return fallbackResult;
      }
      catch (Exception fallbackEx)
      {
        Console.WriteLine($"Error with fallback language model: {fallbackEx.Message}");
        Console.WriteLine("Both models failed, returning empty result");
        return new List<ExtractedOrderItem>();
      }
    }
  }
}