### Link to MS6 video demonstration

[Link](https://johnabbott-my.sharepoint.com/personal/1972185_johnabbottcollege_net/_layouts/15/stream.aspx?id=%2Fpersonal%2F1972185%5Fjohnabbottcollege%5Fnet%2FDocuments%2F20230515%5F174552000%5FiOS%2EMOV&ga=1)

### Azure CLI commands for the actuators

#### Set buzzer to on:
```
az iot hub invoke-device-method --hub-name {iothub_name} --device-id {device_id} --method-name actuate_buzzer --method-payload "{'value': 'on'}"
```

#### Set buzzer to off:
```
az iot hub invoke-device-method --hub-name {iothub_name} --device-id {device_id} --method-name actuate_buzzer --method-payload "{'value': 'off'}"
```

#### Set door lock to closed:
```
az iot hub invoke-device-method --hub-name {iothub_name} --device-id {device_id} --method-name actuate_lock --method-payload "{'value': 'on'}"
```

#### Set door lock to open:
```
az iot hub invoke-device-method --hub-name {iothub_name} --device-id {device_id} --method-name actuate_lock --method-payload "{'value': 'off'}"
```
#### Set RGBLED to on:
```
az iot hub invoke-device-method --hub-name {iothub_name} --device-id {device_id} --method-name actuate_rgbled --method-payload "{'value': 'on'}"
```

#### Set RGBLED to off:
```
az iot hub invoke-device-method --hub-name {iothub_name} --device-id {device_id} --method-name actuate_rgbled --method-payload "{'value': 'off'}"
```

#### Set RGBLED to on with a specified color:
```
az iot hub invoke-device-method --hub-name {iothub_name} --device-id {device_id} --method-name actuate_rgbled --method-payload "{'value': 'on', 'color': 'red'}"
```

The following are valid colors: red, green, blue, white, orange, black

#### Set fan to on:
```
az iot hub invoke-device-method --hub-name {iothub_name} --device-id {device_id} --method-name actuate_fan --method-payload "{'value': 'on'}"
```

#### Set fan to off:
```
az iot hub invoke-device-method --hub-name {iothub_name} --device-id {device_id} --method-name actuate_fan --method-payload "{'value': 'off'}"
```


### Justification for using direct methods


#### Why direct methods over device twins:

Direct methods are synchronous with a success or fail condition after a timeout period determined by the response and/or connection time. Since it is expecting a response after a certain amount of seconds, it definitely return an answer to our app, whether it is a payload or an error. Device twins on the other hand are asynchronous which means that it is possible to have the code in our reterminal halt at a certain point and to not receive any reported properties.

#### Why direct methods over C2D:

Since C2D is fire-and-forget, you do not get a return value of any type to indicate that the actuator command was successful. Direct methods not only allow for a status code to be returned to indicate the end result of the direct method, but also a payload with custom values to be returned for our application's needs. Direct methods also have response and connection timeouts which allow us to expect a response and connection respectively after a certain period. C2D does not have this which means that you could be sending a message to a device that is not on or had a bug that halted its execution.


### Ports to use for sensors and actuators

| Actuators      | Grove Hat Port |
| -------------- | -------------- |
| Buzzer         | Built-in       |
| Fan            | D5             |
| RGB-LED        | D18            |
| MG90S Servo    | PWM            |



| Sensors        | Grove Hat Port |
| -------------- | -------------- |
| Accelerometer  | Built-in       |
| GPS (Air530)   | UART           |
| Luminosity     | Built-in       |
| AHT20 Temp/Hum | D26            |
| Soil-moisture  | A0             |
| Water-level    | A2             |
| Magnetic Door  | D24            |
| PIR Motion     | D16            |
| Noise Detector | A4             |

Credit To [Mauricio Andres Buschinelli](https://github.com/maujac) for the following:

[Configuring i2c for the AHT20 on port D26](https://docs.google.com/document/d/1M3LKarz0A299S5vM3ysqtHiEnGWiEEBzVWy4iCj3lYc/edit#heading=h.cjznoz59ifrw)

[Bug fix for the i2c library that effects the plant subsystem](https://github.com/Seeed-Studio/grove.py/issues/56)
