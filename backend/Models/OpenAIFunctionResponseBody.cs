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

using System.Text.Json.Serialization;

namespace ChatApi.Models;

/// <summary>
/// A response body of a successful OpenAI function call.
/// </summary>
public class OpenAIFunctionResponseBody
{
    /// <summary>
    /// The list of choices.
    /// </summary>
    [JsonPropertyName("choices")]
    public OpenAIFunctionResponseBodyChoice[] Choices { get; set; } = [];
}

/// <summary>
/// Stores data of a chat completion choice .
/// </summary>
public class OpenAIFunctionResponseBodyChoice
{
    /// <summary>
    /// The message data of this choice.
    /// </summary>
    [JsonPropertyName("message")]
    public OpenAIFunctionResponseBodyChoiceMessage Message { get; set; }
}

/// <summary>
/// Stores the message data of a chat completion choice.
/// </summary>
public class OpenAIFunctionResponseBodyChoiceMessage
{
    /// <summary>
    /// The function call information.
    /// </summary>
    [JsonPropertyName("function_call")]
    public OpenAIFunctionResponseBodyChoiceMessageFunctionCall FunctionCall { get; set; }

    /// <summary>
    /// The role.
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; }
}

/// <summary>
/// Stores function_call part of the message data of a chat completion choice.
/// </summary>
public class OpenAIFunctionResponseBodyChoiceMessageFunctionCall
{
    /// <summary>
    /// The arguments as JSON string.
    /// </summary>
    [JsonPropertyName("arguments")]
    public string Arguments { get; set; }

    /// <summary>
    /// The name of the function to call.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
}