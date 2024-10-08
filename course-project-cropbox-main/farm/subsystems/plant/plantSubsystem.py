from ..interfaces.ISubsystem import ISubsystem
from ..interfaces.ISensor import ISensor, AReading
from ..interfaces.IActuator import IActuator, ACommand

from .actuators.cooling_fan import Fan
from .actuators.rgb_led_stick import RGBLedStrip

from .sensors.temp_humi_sensor import AHT20Sensor
from .sensors.soil_moisture_sensor import MoistureSensor
from .sensors.water_sensor import WaterSensor


from time import sleep

class PlantSubsystem(ISubsystem):

    def __init__(self) -> None:
        """DeviceController constructor to manage a group of sensors and actuators.

        :param list[ISensor] sensors: List of sensors to be read.
        :param list[IActuator] actuators: List of actuators to be controlled.
        """
        fan = Fan()
        led = RGBLedStrip()
        self.sensors: list[ISensor] = [ fan, led, WaterSensor(), MoistureSensor() ]
        self.actuators: list[IActuator] = [fan, led]

    def read_sensors(self) -> list[AReading]:
        """Reads data from all sensors. 

        :return list[AReading]: a list containing all readings collected from sensors.
        """
        readings: list[AReading] = []

        warning_level = 0

        for sensor in self.sensors:
            reading = sensor.read_sensor()
            # check if sensor has warning
            # if warning exists in sensor, set warning level to the highest warning level
            if hasattr(sensor, 'warning'):
                if sensor.warning > warning_level:
                    warning_level = sensor.warning
                if hasattr(sensor, 'fan'):
                    if sensor.fan == True:
                        print('fan on')
                        self.control_actuator(ACommand(ACommand.Type.FAN, '{"value": "on"}'))
                    else:
                        print('fan off')
                        self.control_actuator(ACommand(ACommand.Type.FAN, '{"value": "off"}'))
            readings.extend(reading)

        # if warning level is 2, turn on led and color it red
        if warning_level == 2:
            self.control_actuator(ACommand(ACommand.Type.RGB_LED, '{"value": "on", "color": "red"}'))
        # if warning level is 1, turn on led and color it orange
        elif warning_level == 1:
            self.control_actuator(ACommand(ACommand.Type.RGB_LED, '{"value": "on", "color": "orange"}'))
        # if warning level is 0, set led to white
        else:
            self.control_actuator(ACommand(ACommand.Type.RGB_LED, '{"value": "on", "color": "white"}'))
            
        return readings

    def control_actuator(self, command: ACommand) -> list[AReading]:
        readings = []

        for actuator in self.actuators:
            if(actuator.validate_command(command)):
                readings.extend(actuator.control_actuator(command.data)) 
        
        return readings

    def set_sensor_thresholds(self, thresholds: dict[str, int]):
        for sensor in self.sensors:
            if hasattr(sensor, 'set_threshold'):
                sensor.set_threshold(thresholds)

if __name__ == "__main__":
    soil_moisture_sensor = MoistureSensor(0)
    water_sensor = WaterSensor(2)
    temp_humi_sensor = AHT20Sensor(4)
    sensors: list[ISensor] = [soil_moisture_sensor, water_sensor, temp_humi_sensor]
    
    led_strip = RGBLedStrip(18)
    fan = Fan(5)
    actuators: list[IActuator] = [fan, led_strip]
    
    s = PlantSubsystem()
    
    fan_on_message_body = '{"value": "on"}'
    fan_off_message_body = '{"value": "off"}'
    led_on_message_body = '{"value": "on"}'
    led_off_message_body = '{"value": "off"}'
    
    fan_on_command=ACommand(ACommand.Type.FAN,fan_on_message_body)
    fan_off_command=ACommand(ACommand.Type.FAN,fan_off_message_body)
    led_on_command=ACommand(ACommand.Type.RGB_LED,fan_on_message_body)
    led_off_command=ACommand(ACommand.Type.RGB_LED,fan_off_message_body)
    while True:
        readings = s.read_sensors()
        for reading in readings:
            print(reading)
        sleep(1)