// MIT License
//
// Copyright (c) 2024 Marcel Joachim Kloubert (https://marcel.coffee)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using ChatApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatApi.Controllers;

/// <summary>
/// Controller for chat operations.
/// </summary>
[ApiController]
[Route("api/v1/chats")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="logger">Logger instance from WebAPI framework.</param>
    public ChatController(ILogger<ChatController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Sends a chat request to OpenAI with an image.
    /// </summary>
    /// <param name="body">The request body.</param>
    /// <returns>The response data.</returns>
    [HttpPost]
    public async Task<object> Post(ChatRequestBody body)
    {
        _logger.LogDebug("client submitted '{Prompt}' as prompt", body.Prompt);

        using (var client = new HttpClient())
        {
            // setup HTTP headers
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            );

            // setup content from the user
            var content = new JsonArray()
            {
                    new JsonObject()
                    {
                        ["type"] = "text",
                        ["text"] = body.Prompt
                    }
            };
            if (!string.IsNullOrWhiteSpace(body.Image))
            {
                content.Add(
                    new JsonObject()
                    {
                        ["type"] = "image_url",
                        ["image_url"] = new JsonObject()
                        {
                            ["url"] = body.Image
                        }
                    }
                );
            }

            // prepare user message
            // with user prompt
            // and image to process
            var userMessage = new JsonObject()
            {
                ["role"] = "user",
                ["content"] = content
            };

            // collect every thing for the OpenAI request
            var openAIBody = new JsonObject
            {
                // `gpt-4-vision-preview` is able to analyze
                ["model"] = "gpt-4-vision-preview",
                ["messages"] = new JsonArray()
                {
                    userMessage
                },
                ["max_tokens"] = 4096,
                ["temperature"] = 0.7
            };

            // prepare the content as JSON string
            var openAIBodyContent = new StringContent(openAIBody.ToString(), Encoding.UTF8, "application/json");

            // now do the request ...
            using (var openAIChatResponse = await client.PostAsync("https://api.openai.com/v1/chat/completions", openAIBodyContent))
            {
                if (!openAIChatResponse.IsSuccessStatusCode)
                {
                    // not what we expected

                    var openAIResponseBodyContent = await openAIChatResponse.Content.ReadAsStringAsync();

                    return StatusCode(500, new Dictionary<string, object>()
                    {
                        ["success"] = false,
                        ["data"] = null!,
                        ["messages"] = new IDictionary<string, object>[]
                        {
                            new Dictionary<string, object>()
                            {
                                ["code"] = (int)openAIChatResponse.StatusCode,
                                ["type"] = "error",
                                ["message"] = openAIResponseBodyContent
                            }
                        }
                    });
                }

                var openAIResponseBody = await openAIChatResponse.Content.ReadFromJsonAsync<OpenAIResponseBody>();

                // from here everything seem to be fine ... return the answer from ChatGPT
                return new Dictionary<string, object>()
                {
                    ["success"] = true,
                    ["data"] = new Dictionary<string, object>()
                    {
                        ["answer"] = openAIResponseBody!.Choices[0].Message.Content
                    },
                    ["messages"] = Array.Empty<object>()
                };
            }
        }
    }
}
