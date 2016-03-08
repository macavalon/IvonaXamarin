IvonaXamarin
================

Ivona TTS Class compatible with Xamarin

Based on initial code from https://github.com/MalyutinS/DotNetIvonaAPI

Requires Json.NET from Newtonsoft

Xamarin example (on android)
============================
```
IvonaTts IvonaTtsSpeaker = new IvonaTts ();

String textToSpeak = "Hello World";

var documents = Android.OS.Environment.ExternalStorageDirectory.Path;
var destinationFileName = Path.Combine (documents, "test.mp3");

//Generate mp3 file from text

IvonaTtsSpeaker.SynthesizeToFile(textToSpeak,destinationFileName);
```
Notes
======

Please, <a href="https://www.ivona.com/us/account/speechcloud/creation/">obtain your own keys</a> to use API and replace two lines below with your values:
        
        const string AccessKey = "AccessKey";
        const string SecretKey = "SecretKey";
