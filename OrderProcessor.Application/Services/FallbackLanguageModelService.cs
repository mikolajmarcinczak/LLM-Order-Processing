using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System;
using System.Text.RegularExpressions;
using OrderProcessor.Application.Data;
using OrderProcessor.Application.Interfaces;
using OrderProcessor.Domain.Entities;

namespace OrderProcessor.Application.Services;

public class FallbackLanguageModelService : ILanguageModelService
{
  private readonly HttpClient _httpClient;
  private readonly string _ollamaApiUrl;

  public FallbackLanguageModelService(HttpClient httpClient, string ollamaApiUrl)
  {
    _httpClient = httpClient;
    _ollamaApiUrl = ollamaApiUrl;
  }

  public async Task<List<ExtractedOrderItem>> ExtractOrderDetailsAsync(string emailContent)
  {
    try
    {
      Console.WriteLine($"Using fallback Ollama model at: {_ollamaApiUrl}");

      var requestBody = new
      {
        model = "llama3.2:3b",
        prompt = $@"Extract product information from this email and return ONLY a JSON array. Each object should have 'ProductName', 'Quantity', and 'Price' properties.

                            Email content:
                            {emailContent}

                            Return format example:
                            [
                            {{""ProductName"": ""Name of the product"", ""Quantity"": 2, ""Price"": $999.00}},
                            {{""ProductName"": ""Name of another product"", ""Quantity"": 5, ""Price"": $49.99}}
                            ]",
        format = "json",
        stream = false
      };

      var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
      Console.WriteLine($"Sending request to Ollama with prompt length: {requestBody.prompt.Length}");

      var response = await _httpClient.PostAsync(_ollamaApiUrl, jsonContent);
      response.EnsureSuccessStatusCode();

      var responseBody = await response.Content.ReadAsStringAsync();
      Console.WriteLine($"Ollama response received: {responseBody.Length} characters");

      var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

      if (ollamaResponse?.Response != null)
      {
        Console.WriteLine($"Ollama response content: {ollamaResponse.Response}");
        try
        {
          var extractedItems = JsonSerializer.Deserialize<List<ExtractedOrderItem>>(ollamaResponse.Response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
          Console.WriteLine($"Successfully parsed {extractedItems?.Count ?? 0} items from Ollama response");
          return extractedItems ?? new List<ExtractedOrderItem>();
        }
        catch (JsonException ex)
        {
          //  If the JsonSerializer fails, parse manually
          Console.WriteLine($"JSON deserialization error from Ollama response: {ex.Message}");
          Console.WriteLine($"Attempting to parse malformed JSON manually...");

          var manuallyParsedItems = ParseMalformedJson(ollamaResponse.Response);
          if (manuallyParsedItems.Count > 0)
          {
            Console.WriteLine($"Successfully parsed {manuallyParsedItems.Count} items manually");
            return manuallyParsedItems;
          }

          Console.WriteLine($"Raw response: {ollamaResponse.Response}");
          return new List<ExtractedOrderItem>();
        }
      }

      Console.WriteLine("Ollama response was null or empty");
      return new List<ExtractedOrderItem>();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error using fallback Ollama model: {ex.Message}");
      Console.WriteLine($"Exception type: {ex.GetType().Name}");
      return new List<ExtractedOrderItem>();
    }
  }

  private List<ExtractedOrderItem> ParseMalformedJson(string response)
  {
    var items = new List<ExtractedOrderItem>();

    try
    {
      //  Regex to match targeted JSON format
      var pattern = @"""ProductName"":\s*""([^""]+)"",\s*""Quantity"":\s*(\d+),\s*""Price"":\s*([\d.]+)";
      var matches = Regex.Matches(response, pattern);

      foreach (Match match in matches)
      {
        if (match.Groups.Count >= 4)
        {
          var productName = match.Groups[1].Value;
          var quantity = int.Parse(match.Groups[2].Value);
          var price = decimal.Parse(match.Groups[3].Value);

          items.Add(new ExtractedOrderItem
          {
            ProductName = productName,
            Quantity = quantity,
            Price = price
          });
        }
      }

      Console.WriteLine($"Manually parsed {items.Count} items from malformed JSON");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error parsing malformed JSON: {ex.Message}");
    }

    return items;
  }
}