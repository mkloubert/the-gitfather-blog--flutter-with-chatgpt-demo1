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

public class WeatherOptions
{
    /// <summary>
    /// The name of the country the location can be found in.
    /// </summary>
    [JsonPropertyName("country_name")]
    public string Country { get; set; }

    /// <summary>
    /// The latitude.
    /// </summary>
    [JsonPropertyName("latitude")]
    public float Latitude { get; set; }

    /// <summary>
    /// The official name of the location.
    /// </summary>
    [JsonPropertyName("location_name")]
    public string Location { get; set; }

    /// <summary>
    /// The longitude.
    /// </summary>
    [JsonPropertyName("longitude")]
    public float Longitude { get; set; }

    /// <summary>
    /// The postcode of the location.
    /// </summary>
    [JsonPropertyName("postcode")]
    public string Postcode { get; set; }
}
