from .actuators.buzzer import Buzzer

from .sensor.gps import GPS
from .sensor.Accelerometer import Accelerometer

from time import sleep

from ..interfaces.ISubsystem import ISubsystem
from ..interfaces.ISensor import ISensor, AReading
from ..interfaces.IActuator import IActuator, ACommand

class GeoLocationSubsystem(ISubsystem):
    def __init__(self) -> None:
        self.sensors: list[ISensor] = [Accelerometer()]
        self.actuators: list[IActuator] = []
        self.has_shared_components = True
    
    def read_sensors(self) -> list[AReading]:
        readings = []

        warning_level = 0

        for sensor in self.sensors:
            reading = sensor.read_sensor()

            if sensor.type == ACommand.Type.BUZZER:
                buzzer = sensor

            if hasattr(sensor, 'model'):
                if sensor.model == "reterminal built-in accelerometer":
                    # cast sensor as Accelerometer
                    sensor.__class__ = Accelerometer
                    warning_level = sensor.warning
                readings.extend(reading)

        # if warning level is 2, turn on buzzer
        if warning_level == 2:
            if buzzer is not None:
                buzzer.turn_on[0] = True
                
                if buzzer.turn_on[0] == True or buzzer.turn_on[1] == True:
                    self.control_actuator(ACommand(target=ACommand.Type.BUZZER, raw_message_body='{"value": "on"}'))
        else:
            if buzzer is not None:
                buzzer.turn_on[0] = False
                
                if buzzer.turn_on[0] == False and buzzer.turn_on[1] == False:
                    self.control_actuator(ACommand(target=ACommand.Type.BUZZER, raw_message_body='{"value": "off"}'))
        
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
    g = GPS()
    a = Accelerometer()
    b = Buzzer()
    
    s = GeoLocationSubsystem()

    buzzer_command_on = ACommand(target=ACommand.Type.BUZZER, raw_message_body='{"value": "on"}')
    buzzer_command_off = ACommand(target=ACommand.Type.BUZZER, raw_message_body='{"value": "off"}')

    while True:
        readings = s.read_sensors()
        for reading in readings:
            print(reading)
        s.control_actuator(buzzer_command_on)        
        sleep(1)
        s.control_actuator(buzzer_command_off)        
        sleep(1)