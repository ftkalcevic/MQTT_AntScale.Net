# MQTT_AntScale.Net
Ant Weight Scale monitor and publish via MQTT

This is a .Net version of my other repository, [MQTT_ant_scale](https://github.com/ftkalcevic/MQTT_ant_scale).  The orignal plan was to write the python version and run it on a RaspberryPi Zero W near the scales, but I didn't need to do that because the Ant USB Stick 2 has a long range (5m+) so I can plug it in directly to my always-on server which is a Windows 10 server.

This is built from the thisisant.com sdk sample.  It was a quick and dirty port, so there is a lot of existing unused code.

The pc sdk wrapper projects are included, but not the binaries - you need to copy them from the sdk.

You'll need to update the App.config files and set the network key - this is free when you join thisisant.com and become a adopter member.

This project is a .Net Service (MQTT_AntScale) that listens for Ant+ weight scales to activate, reads the data and then publishes it via MQTT.  There is also a command line version (MQTT_AntScale_Test).

The process listens for datapages 1 Body Weight, 2 Body Composition Percentage, 3 Metabolic Information, 4 Body Composition Mass, and 58 User Profile.  It only publishes if all pages are received.  It publishes when the connection to the scale is broken.  I'm using the SmartLab W scales.

The data is published using topic tele/scales/weight.
The json data is...

```javascript
{ 
	"timestamp": "2020-01-21T06:32:45", "
	userProfile": 1, 
	"weight": 92.8, 
	"gender": "M", 
	"age": 22, 
	"height": 179, 
	"hydrationPercentage": 46.3, 
	"bodyFatPercentage": 16.7, 
	"activeMetabolicRate": 2199, 
	"basalMetabolicRate": 1912, 
	"muscleMass": 42.07, 
	"boneMass": 3.5 
}
```

