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
/// A response body of a successful OpenAI chat call.
/// </summary>
public class OpenAIResponseBody
{
    /// <summary>
    /// The list of choices.
    /// </summary>
    [JsonPropertyName("choices")]
    public OpenAIResponseBodyChoice[] Choices { get; set; } = [];
}

/// <summary>
/// Stores data of a chat completion choice.
/// </summary>
public class OpenAIResponseBodyChoice
{
    /// <summary>
    /// The message data of this choice.
    /// </summary>
    [JsonPropertyName("message")]
    public OpenAIResponseBodyChoiceMessage Message { get; set; }
}

/// <summary>
/// Stores the message data of a chat completion choice.
/// </summary>
public class OpenAIResponseBodyChoiceMessage
{
    /// <summary>
    /// The content.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }

    /// <summary>
    /// The role.
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; }
}
