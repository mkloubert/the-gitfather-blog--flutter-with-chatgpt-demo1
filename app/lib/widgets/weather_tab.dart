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

import 'dart:convert';
import 'dart:io';

import 'package:flutter/material.dart';
import 'package:logger/logger.dart';
import 'package:http/http.dart' as http;
import 'package:maps_launcher/maps_launcher.dart';

final buttonStyle = TextButton.styleFrom(
  textStyle: const TextStyle(
    color: Colors.white,
  ),
  backgroundColor: Colors.black,
  foregroundColor: Colors.white,
);

const initialPrompt = "Aachen Cathedral";

final logger = Logger(
  level: Level.debug,
);

class _WeatherData {
  late String _description;
  late String _icon;
  late double _latitude;
  late double _longitude;
  late String _name;
  late double _temperature;

  _WeatherData._();

  String get description {
    return _description;
  }

  String get icon {
    return _icon;
  }

  double get latitude {
    return _latitude;
  }

  double get longitude {
    return _longitude;
  }

  String get name {
    return _name;
  }

  double get temperature {
    return _temperature;
  }

  static _WeatherData fromMap(dynamic map) {
    final data = _WeatherData._();

    data._description = map["data"]["weather"];
    data._icon = map["data"]["icon"];
    data._latitude = double.parse("${map["data"]["latitude"]}");
    data._longitude = double.parse("${map["data"]["longitude"]}");
    data._name = map["data"]["name"];
    data._temperature = double.parse("${map["data"]["temperature"]}");

    return data;
  }
}

class WeatherTab extends StatefulWidget {
  /// Initializes a new instance of this class, with an optional
  /// and custom [key].
  const WeatherTab({
    super.key,
  });

  @override
  State<WeatherTab> createState() => _WeatherTabState();
}

class _WeatherTabState extends State<WeatherTab> {
  late TextEditingController _textController;
  String _promptText = initialPrompt;
  bool _isSubmitting = false;
  _WeatherData? _weather;

  @override
  void initState() {
    super.initState();

    _textController = TextEditingController();
    _textController.text = initialPrompt;
  }

  @override
  Widget build(BuildContext context) {
    logger.d("is submitting: $_isSubmitting");

    Widget body;
    if (_isSubmitting) {
      body = const Center(
          child: Center(
        child: CircularProgressIndicator(),
      ));
    } else {
      body = _buildPage(context);
    }

    return Padding(
      padding: const EdgeInsets.all(8.0),
      child: body,
    );
  }

  Widget _buildPage(BuildContext context) {
    logger.d("prompt text: $_promptText");

    // button to get weather
    VoidCallback? getWeatherButtonPressed;
    if (!_isSubmitting) {
      getWeatherButtonPressed = () async {
        try {
          final data = await _submitPromptAndGetWeather(context, _promptText);
          if (data == null) {
            return;
          }

          setState(() {
            _weather = data;
          });
        } catch (error) {
          _showErrorDialog(error);

          logger.e("could not update image data from gallery: $error");
        }
      };
    }

    // button list at bottom
    final List<Widget> buttons = [
      Expanded(
        flex: 1,
        child: Padding(
          padding: const EdgeInsets.all(8.0),
          child: TextButton(
            onPressed: getWeatherButtonPressed,
            style: buttonStyle,
            child: const Text("Get weather"),
          ),
        ),
      )
    ];
    if (_weather != null) {
      buttons.add(Expanded(
        flex: 1,
        child: Padding(
          padding: const EdgeInsets.all(8.0),
          child: TextButton(
            onPressed: () {
              MapsLauncher.launchCoordinates(
                _weather!.latitude,
                _weather!.longitude,
              );
            },
            style: buttonStyle,
            child: const Text("Open map"),
          ),
        ),
      ));
    }

    Widget weatherInfoBody;
    if (_weather != null) {
      weatherInfoBody = Center(
        child: Column(
          children: [
            // icon
            Expanded(
              flex: 2,
              child: Image.network(
                _weather!.icon,
                scale: 0.5,
              ),
            ),
            // location name
            Expanded(
              flex: 0,
              child: Padding(
                padding: const EdgeInsets.symmetric(
                  vertical: 16,
                  horizontal: 8,
                ),
                child: Text(
                  _weather!.name,
                  style: const TextStyle(
                    fontSize: 32.0,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ),
            // temperature
            Expanded(
              flex: 0,
              child: Padding(
                padding: const EdgeInsets.symmetric(
                  vertical: 12,
                  horizontal: 8,
                ),
                child: Text(
                  "${_weather!.temperature} °C",
                  style: const TextStyle(
                    fontSize: 24.0,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ),
            // weather description
            Expanded(
              flex: 0,
              child: Padding(
                padding: const EdgeInsets.symmetric(
                  vertical: 16,
                  horizontal: 8,
                ),
                child: Text(
                  _weather!.description,
                  style: const TextStyle(
                    fontSize: 16.0,
                  ),
                ),
              ),
            ),
          ],
        ),
      );
    } else {
      weatherInfoBody = const Center(
        child: Text("no data available yet"),
      );
    }

    return Column(
      children: [
        // prompt textbox
        Expanded(
          flex: 0,
          child: SizedBox(
            width: double.infinity,
            child: Padding(
              padding: const EdgeInsets.all(8.0),
              child: TextField(
                controller: _textController,
                onChanged: (value) => {
                  setState(() {
                    _promptText = value;
                  })
                },
                minLines: 1,
                maxLines: 5,
                decoration: const InputDecoration(
                  border: OutlineInputBorder(),
                  labelText: 'Your prompt',
                ),
              ),
            ),
          ),
        ),
        // weather info
        Expanded(
          flex: 1,
          child: Expanded(
            flex: 1,
            child: weatherInfoBody,
          ),
        ),
        // buttons
        Expanded(
          flex: 0,
          child: SizedBox(
            width: double.infinity,
            height: 80,
            child: Row(
              children: buttons,
            ),
          ),
        ),
      ],
    );
  }

  _showErrorDialog(dynamic error) {
    showDialog<void>(
      context: context,
      barrierDismissible: false, // user must tap button!
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('ERROR!'),
          content: SingleChildScrollView(
            child: ListBody(
              children: <Widget>[
                Text("$error"),
              ],
            ),
          ),
          actions: <Widget>[
            TextButton(
              child: const Text('OK'),
              onPressed: () {
                Navigator.of(context).pop();
              },
            ),
          ],
        );
      },
    ).catchError((error) {
      logger.e("$error");
    });
  }

  Future<_WeatherData?> _submitPromptAndGetWeather(
    BuildContext context,
    String prompt,
  ) async {
    setState(() {
      _isSubmitting = true;
    });

    try {
      if (prompt.trim().isEmpty) {
        return null; // no prompt
      }

      // collect the data for the request to the C# backend
      final url = Uri.parse("http://localhost:5080/api/v1/weathers");
      final headers = {
        "Content-Type": "application/json",
      };
      final body = {
        "prompt": prompt.trim(),
      };

      // do the POST request
      final response = await http.post(
        url,
        headers: headers,
        body: json.encode(body),
      );

      if (response.statusCode != 200) {
        // not what we expected

        throw HttpException(
          "Unexpected status code: ${response.statusCode}",
          uri: url,
        );
      }

      final responseBody = json.decode(response.body);

      return _WeatherData.fromMap(responseBody);
    } finally {
      setState(() {
        _isSubmitting = false;
      });
    }
  }
}
