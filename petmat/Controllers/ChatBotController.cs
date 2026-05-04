using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CoreLayer.Dtos.ChatBot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace petmat.Controllers
{
    [Authorize]
    public class ChatBotController : BaseApiController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _chatbotBaseUrl;

        public ChatBotController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            // Updated fallback to host.docker.internal for Docker compatibility
            _chatbotBaseUrl = configuration["AleefChatbot:BaseUrl"] ?? "http://host.docker.internal:8000";
        }

        [HttpPost("chat")]
        public async Task StreamChat([FromBody] ChatRequest request)
        {
            // 1. Disable buffering so Kestrel sends the chunks immediately
            var responseFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>();
            responseFeature?.DisableBuffering();

            // 2. Use octet-stream to prevent browsers from buffering text
            Response.ContentType = "application/octet-stream";

            var client = _httpClientFactory.CreateClient();
            var chatRequest = new HttpRequestMessage(HttpMethod.Post, $"{_chatbotBaseUrl}/chat");
            var jsonContent = JsonSerializer.Serialize(new { query = request.Query });
            chatRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Use ResponseHeadersRead to avoid buffering the full response from Python
            using var response = await client.SendAsync(chatRequest, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            // Keep track of the previous chunk to detect patterns that span chunks
            string lastChar = "";

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                // Check for null so we don't crash, and grab any line starting with data:
                if (line != null && line.StartsWith("data:"))
                {
                    string chunk = "";
                    
                    if (line == "data:" || line == "data: ")
                    {
                        // If the AI sent an empty data line, it means it's a line break / new paragraph
                        chunk = "\n\n"; // Double line break for proper paragraph spacing
                    }
                    else
                    {
                        // Otherwise, get the text normally
                        chunk = line.StartsWith("data: ") ? line.Substring(6) : line.Substring(5);
                        
                        // Some AI models write literal string "\n" instead of actual line breaks, let's fix that if it occurs
                        chunk = chunk.Replace("\\n", "\n\n");

                        // SMART FORMATTING: Add a line break BEFORE numbers followed by a dot (e.g., "1.", "2.")
                        // Only if the previous character wasn't a space/newline already.
                        if (Regex.IsMatch(chunk, @"^[0-9]+\.") && !string.IsNullOrWhiteSpace(lastChar) && !lastChar.EndsWith("\n"))
                        {
                            chunk = "\n\n" + chunk;
                        }
                    }

                    // Save the last character to detect formatting boundaries
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        lastChar = chunk.Substring(chunk.Length - 1);
                    }

                    await Response.WriteAsync(chunk);
                    await Response.Body.FlushAsync();
                }
            }
        }
    }
}
