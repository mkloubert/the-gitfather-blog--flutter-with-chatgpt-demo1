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
import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:image/image.dart' as img;
import 'package:logger/logger.dart';
import 'package:http/http.dart' as http;

final buttonStyle = TextButton.styleFrom(
  textStyle: const TextStyle(
    color: Colors.white,
  ),
  backgroundColor: Colors.black,
  foregroundColor: Colors.white,
);

const initialPrompt = "What do you see?";

// max image sizes
const maxImageHeight = 1024;
const maxImageQuality = 67;
const maxImageWidth = 1280;

final logger = Logger(
  level: Level.debug,
);

class ImageRecognitionTab extends StatefulWidget {
  /// Initializes a new instance of this class, with an optional
  /// and custom [key].
  const ImageRecognitionTab({
    super.key,
  });

  @override
  State<ImageRecognitionTab> createState() => _ImageRecognitionTabState();
}

class _ImageRecognitionTabState extends State<ImageRecognitionTab> {
  final ImagePicker _picker = ImagePicker();
  Uint8List? _imageData;
  late TextEditingController _textController;
  String _promptText = initialPrompt;
  bool _isSubmitting = false;

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

    // selected image
    Widget imageWidget;
    if (_imageData != null) {
      imageWidget = Image.memory(_imageData!);

      logger.d("created Image instance");
    } else {
      imageWidget = Container();
    }

    // submit button
    Widget submitButton;
    if (_promptText.trim().isNotEmpty && _imageData != null) {
      VoidCallback? onSubmitPressed;
      String submitButtonText;
      if (!_isSubmitting) {
        onSubmitPressed = () async {
          try {
            await _submitPromptAndImage(context, _promptText, _imageData);
          } catch (error) {
            logger.e("submit prompt and image failed: $error");

            _showErrorDialog(error);
          }
        };

        submitButtonText = "SUBMIT";
      } else {
        submitButtonText = "Please wait ...";
      }

      submitButton = SizedBox(
        width: double.infinity,
        height: 64,
        child: Padding(
          padding: const EdgeInsets.all(8.0),
          child: TextButton(
            onPressed: onSubmitPressed,
            style: buttonStyle,
            child: Text(submitButtonText),
          ),
        ),
      );
    } else {
      submitButton = Container();
    }

    // buttons to select image or photo
    VoidCallback? selectImageButtonPressed;
    VoidCallback? takePhotoButtonPressed;
    if (!_isSubmitting) {
      selectImageButtonPressed = () async {
        try {
          final imageData = await _getImageFrom(ImageSource.gallery);
          if (imageData == null) {
            return;
          }

          setState(() {
            _imageData = imageData;

            logger.d("update image data from gallery");
          });
        } catch (error) {
          logger.e("could not update image data from gallery: $error");
        }
      };

      takePhotoButtonPressed = () async {
        try {
          final imageData = await _getImageFrom(ImageSource.camera);
          if (imageData == null) {
            return;
          }

          setState(() {
            _imageData = imageData;

            logger.d("update image data from camera");
          });
        } catch (error) {
          logger.e("could not update image data from camera: $error");
        }
      };
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
        // submit button
        Expanded(
          flex: 0,
          child: Expanded(
            child: submitButton,
          ),
        ),
        // selected image
        Expanded(
          flex: 1,
          child: Expanded(
            child: SizedBox(
              width: double.infinity,
              height: double.infinity,
              child: imageWidget,
            ),
          ),
        ),
        // buttons to select an image or photo
        Expanded(
          flex: 0,
          child: SizedBox(
            width: double.infinity,
            height: 80,
            child: Row(
              children: [
                Expanded(
                  flex: 1,
                  child: Padding(
                    padding: const EdgeInsets.all(8.0),
                    child: TextButton(
                      onPressed: selectImageButtonPressed,
                      style: buttonStyle,
                      child: const Text("Select image"),
                    ),
                  ),
                ),
                Expanded(
                  flex: 1,
                  child: Padding(
                    padding: const EdgeInsets.all(8.0),
                    child: TextButton(
                      onPressed: takePhotoButtonPressed,
                      style: buttonStyle,
                      child: const Text("Take photo"),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Future<Uint8List?> _getImageFrom(ImageSource source) async {
    final imageFile = await _picker.pickImage(source: source);
    if (imageFile == null) {
      logger.d("image picker cancellation");
      return null; // user
    }

    final imageData = await imageFile.readAsBytes();
    final image = img.decodeImage(imageData);
    if (image == null) {
      logger.d("no image data after decoding");
      return null;
    }

    logger.d("got image");

    int newHeight;
    int newWidth;
    if (image.width > image.height) {
      // landscape

      newWidth = maxImageWidth;
      newHeight = (image.height / image.width * maxImageWidth).round();
    } else {
      newHeight = maxImageHeight;
      newWidth = (image.width / image.height * maxImageHeight).round();
    }

    logger.d("new image size: $newWidth x $newHeight");

    // resize image
    final resizedImage = img.copyResize(
      image,
      width: newWidth,
      height: newHeight,
    );
    logger.d("resized image");

    // create JPEG with relative small size
    final encodedImageData = img.encodeJpg(
      resizedImage,
      quality: maxImageQuality,
    );
    logger.d("encoded image to JPEG with size of ${encodedImageData.length}");

    return encodedImageData;
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

  Future<void> _submitPromptAndImage(
    BuildContext context,
    String prompt,
    Uint8List? image,
  ) async {
    setState(() {
      _isSubmitting = true;
    });

    try {
      if (prompt.trim().isEmpty) {
        return; // no prompt
      }
      if (image == null) {
        return; // no image
      }

      final base64Encoder = base64.encoder;

      // create a data URI from JPEG image
      final base64Image = base64Encoder.convert(image);
      final imageDataUri = "data:image/jpeg;base64,$base64Image";

      logger.d("data uri to submit: $imageDataUri");

      // collect the data for the request to the C# backend
      final url = Uri.parse("http://localhost:5080/api/v1/chats");
      final headers = {
        "Content-Type": "application/json",
      };
      final body = {
        'image': imageDataUri,
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

      if (context.mounted) {
        // show the answer from LLM

        showDialog<void>(
          context: context,
          barrierDismissible: false, // user must tap button!
          builder: (BuildContext context) {
            return AlertDialog(
              title: const Text("Answer"),
              content: SingleChildScrollView(
                child: ListBody(
                  children: <Widget>[
                    Text("${responseBody["data"]["answer"]}"),
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
    } finally {
      setState(() {
        _isSubmitting = false;
      });
    }
  }
}
