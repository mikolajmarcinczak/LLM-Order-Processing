using System;
using OpenAI.Chat;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OpenAI;
using OrderProcessor.Domain.Entities;

namespace OrderProcessor.Application.Services;

public class LanguageModelService
{
  private readonly OpenAIClient _openAIClient;
  private readonly ChatClient _chatClient;

  public LanguageModelService(string apiKey)
  {
    _openAIClient = new OpenAIClient(apiKey);
    _chatClient = new ChatClient(model: "gpt-4o", apiKey: apiKey);
  }

  public async Task<List<ExtractedOrderItem>> ExtractOrderDetailsAsync(string emailContent)
  {
    var messages = new List<ChatMessage>
    {
      new SystemChatMessage("You are an AI assistant that extracts product name, quantity, and price from order emails. Respond only with a JSON array of objects, where each object has 'ProductName', 'Quantity', and 'Price' properties."),
      new UserChatMessage($"Extract order details from the following email content:\n\n{emailContent}\n\nFormat the output as a JSON array of objects with ProductName, Quantity, and Price. Example: [{{'ProductName': 'Laptop', 'Quantity': 1, 'Price': 1200.00}}, {{'ProductName': 'Mouse', 'Quantity': 2, 'Price': 25.50}}]")
    };

    var chatCompletionOptions = new ChatCompletionOptions()
    {
      ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(jsonSchemaFormatName: "OrderDetailsSchema", jsonSchema: BinaryData.FromString("""{"type": "object", "additionalProperties": true}"""))
    };

    ChatCompletion response = await _chatClient.CompleteChatAsync(messages, chatCompletionOptions);

    if (response.Content != null && response.Content.Any())
    {
      string jsonResponse = response.Content[0].Text;
      try
      {
        var extractedItems = JsonSerializer.Deserialize<List<ExtractedOrderItem>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return extractedItems ?? new List<ExtractedOrderItem>();
      }
      catch (JsonException ex)
      {
        Console.WriteLine($"Błąd deserializacji JSON: {ex.Message}");
        return new List<ExtractedOrderItem>();
      }
    }

    return new List<ExtractedOrderItem>();
  }
}