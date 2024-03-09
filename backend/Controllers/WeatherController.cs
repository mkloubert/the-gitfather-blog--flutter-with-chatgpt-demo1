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

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using ChatApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatApi.Controllers;

/// <summary>
/// Controller for weather operations.
/// </summary>
[ApiController]
[Route("api/v1/weathers")]
public class WeatherController : ControllerBase
{
    private readonly ILogger<WeatherController> _logger;

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="logger">Logger instance from WebAPI framework.</param>
    public WeatherController(ILogger<WeatherController> logger)
    {
        _logger = logger;
    }

    private static JsonObject _CreateOpenAIRequestBody(string prompt)
    {
        return new JsonObject()
        {
            ["model"] = "gpt-3.5-turbo-0125",
            ["functions"] = new JsonArray()
            {
                new JsonObject()
                {
                    ["name"] = "get_weather_of_location",
                    ["description"] = "Gets weather information by geo location.",
                    ["parameters"] = new JsonObject()
                    {
                        ["type"] = "object",
                        ["required"] = new JsonArray()
                        {
                            "latitude",
                            "longitude",
                            "location_name",
                            "postcode",
                            "country_name"
                        },
                        ["properties"] = new JsonObject()
                        {
                            ["latitude"] = new JsonObject()
                            {
                                ["description"] = "The latitude part of the geo location.",
                                ["type"] = "number"
                            },
                            ["longitude"] = new JsonObject()
                            {
                                ["description"] = "The longitude part of the geo location.",
                                ["type"] = "number"
                            },
                            ["location_name"] = new JsonObject()
                            {
                                ["description"] = "The official name of the location.",
                                ["type"] = "string"
                            },
                            ["postcode"] = new JsonObject()
                            {
                                ["description"] = "The postcode of the location.",
                                ["type"] = "string"
                            },
                            ["country_name"] = new JsonObject()
                            {
                                ["description"] = "The name of the location's country.",
                                ["type"] = "string"
                            }
                        }
                    }
                }
            },
            ["function_call"] = new JsonObject()
            {
                ["type"] = "function",
                ["name"] = "get_weather_of_location"
            },
            ["messages"] = new JsonArray()
            {
                new JsonObject()
                {
                    ["role"] = "system",
                    ["content"] = "You are a helpful assistant that helps the user to get weather information about a location."
                },
                new JsonObject()
                {
                    ["role"] = "user",
                    ["content"] = prompt
                }
            }
        };
    }

    private async Task<object> _GetWeather(string functionArguments)
    {
        var options = JsonSerializer.Deserialize<WeatherOptions>(functionArguments);

        var apiKey = Environment.GetEnvironmentVariable("OPEN_WEATHER_MAP_API_KEY");

        using (var client = new HttpClient())
        {
            // query suffix
            var q = "?lat=" + options!.Latitude.ToString();
            q += "&lon=" + options!.Longitude.ToString();
            q += "&units=metric";
            q += "&lang=en";
            q += "&appid=" + HttpUtility.UrlEncode(apiKey);

            using (var owmResponse = await client.GetAsync("https://api.openweathermap.org/data/2.5/weather" + q))
            {
                if (owmResponse.StatusCode != HttpStatusCode.OK)
                {
                    // not what we expected

                    var owmResponseBodyContent = await owmResponse.Content.ReadAsStringAsync();

                    return StatusCode(500, new Dictionary<string, object>()
                    {
                        ["success"] = false,
                        ["data"] = null!,
                        ["messages"] = new IDictionary<string, object>[]
                        {
                            new Dictionary<string, object>()
                            {
                                ["code"] = (int)owmResponse.StatusCode,
                                ["type"] = "error",
                                ["message"] = owmResponseBodyContent
                            }
                        }
                    });
                }

                var openAIResponseBodyData = await owmResponse.Content.ReadAsStringAsync();
                var openAIResponseBody = JsonSerializer.Deserialize<OpenWeatherMapResponseBody>(openAIResponseBodyData);

                // from this part here anything seems to be fine
                return new Dictionary<string, object>()
                {
                    ["success"] = true,
                    ["data"] = new Dictionary<string, object>()
                    {
                        ["icon"] = $"https://openweathermap.org/img/w/{openAIResponseBody!.Weather[0].Icon}.png",
                        ["latitude"] = openAIResponseBody!.Coordinates.Latitude,
                        ["longitude"] = openAIResponseBody!.Coordinates.Longitude,
                        ["name"] = openAIResponseBody!.Name,
                        ["temperature"] = openAIResponseBody!.Main.Temperature,
                        ["weather"] = openAIResponseBody!.Weather[0].Description
                    },
                    ["messages"] = Array.Empty<object>()
                };
            }
        }
    }

    /// <summary>
    /// Sends a chat request to OpenAI with a function request.
    /// </summary>
    /// <param name="body">The request body.</param>
    /// <returns>The response data.</returns>
    [HttpPost]
    public async Task<object> Post(WeatherRequestBody body)
    {
        _logger.LogDebug("client submitted '{Prompt}' as prompt", body.Prompt);

        using (var client = new HttpClient())
        {
            // setup HTTP headers
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            );

            var openAIBody = _CreateOpenAIRequestBody(body.Prompt);

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

                var openAIResponseBody = await openAIChatResponse.Content.ReadFromJsonAsync<OpenAIFunctionResponseBody>();

                var functionName = openAIResponseBody!.Choices[0].Message.FunctionCall.Name;
                var functionArguments = openAIResponseBody!.Choices[0].Message.FunctionCall.Arguments;

                if (functionName != "get_weather_of_location")
                {
                    // we currently only support `get_weather_of_location`

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
                                ["message"] = $"Unsupported function: {functionName}"
                            }
                        }
                    });
                }

                return await _GetWeather(functionArguments);
            }
        }
    }
}
