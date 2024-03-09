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
/// A response body of an OpenWeatherMap weather request.
/// </summary>
public class OpenWeatherMapResponseBody
{
    /// <summary>
    /// The coordinates of the final location.
    /// </summary>
    [JsonPropertyName("coord")]
    public OpenWeatherMapResponseBodyCoordinates Coordinates { get; set; }

    /// <summary>
    /// The main information.
    /// </summary>
    [JsonPropertyName("main")]
    public OpenWeatherMapResponseBodyMain Main { get; set; }

    /// <summary>
    /// The name of the location.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Timezone in seconds from UTC.
    /// </summary>
    [JsonPropertyName("timezone")]
    public long TimezoneOffset { get; set; }

    /// <summary>
    /// The list of weather data.
    /// </summary>
    [JsonPropertyName("weather")]
    public OpenWeatherMapResponseBodyWeatherItem[] Weather { get; set; }
}

/// <summary>
/// Coordinates inside Coordinates property of OpenWeatherMapResponseBody class.
/// </summary>
public class OpenWeatherMapResponseBodyCoordinates
{
    /// <summary>
    /// The latitude.
    /// </summary>
    [JsonPropertyName("lat")]
    public double Latitude { get; set; }

    /// <summary>
    /// The longitude.
    /// </summary>
    [JsonPropertyName("lon")]
    public double Longitude { get; set; }
}

/// <summary>
/// Main part of OpenWeatherMapResponseBody class.
/// </summary>
public class OpenWeatherMapResponseBodyMain
{
    /// <summary>
    /// The temperature.
    /// </summary>
    [JsonPropertyName("temp")]
    public double Temperature { get; set; }
}

/// <summary>
/// An item of OpenWeatherMapResponseBody inside Weather property.
/// </summary>
public class OpenWeatherMapResponseBodyWeatherItem
{
    /// <summary>
    /// The description text.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// The icon.
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }
}
