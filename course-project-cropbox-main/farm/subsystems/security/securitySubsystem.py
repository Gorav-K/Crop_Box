# from time import sleep

# from devices.sensors import ISensor, AReading
# from devices.actuators import IActuator, ACommand
# from devices.temp_humi_sensor import AHT20
# from devices.fan_control import FanController
# from devices.led_pwm import LedController
from ..interfaces.ISubsystem import ISubsystem
from ..interfaces.ISensor import ISensor, AReading
from ..interfaces.IActuator import IActuator, ACommand

from .actuators.buzzer import Buzzer
from .actuators.doorLockServo import LockServo  
from ..interfaces.ISubsystem import ISubsystem
from .sensors.luminosity import Luminosity
from .sensors.motion import Motion
from .sensors.magneticDoorSensor import MagneticDoorSensor
from .sensors.noise import Noise
from time import sleep

class SecuritySubsystem(ISubsystem):

    def __init__(self) -> None:
        """DeviceController constructor to manage a group of sensors and actuators.

        :param list[ISensor] sensors: List of sensors to be read.
        :param list[IActuator] actuators: List of actuators to be controlled.
        """
        door = MagneticDoorSensor()
        door_lock_servo = LockServo()

        self.sensors : list[ISensor] = [door, Luminosity(), Motion(), Noise(),door_lock_servo]
        self.actuators : list[IActuator]  = [door_lock_servo]
        self.has_shared_components = True

    def read_sensors(self) -> list[AReading]:
        """Reads data from all sensors. 

        :return list[AReading]: a list containing all readings collected from sensors.
        """
        readings: list[AReading] = []

        for sensor in self.sensors:
            reading = sensor.read_sensor()

            if sensor.type == ACommand.Type.BUZZER:
                buzzer = sensor

            readings.extend(reading)
    
    
        if (self.get_door_state() == AReading.State.OPEN and    # we want buzzer on
            self.get_door_lock_state()["value"].lower() == "on"):
            if buzzer is not None:
                buzzer.turn_on[1] = True
                if buzzer.turn_on[0] == True or buzzer.turn_on[1] == True:
                    self.control_actuator(ACommand(target= ACommand.Type.BUZZER, raw_message_body= '{"value": "on"}'))

        elif(self.get_door_state() == AReading.State.CLOSED and 
             self.get_door_lock_state()["value"].lower() == "on" and
             self.get_motion_sensor_state() == AReading.State.MOTION): # we want buzzer on
            if buzzer is not None:
                buzzer.turn_on[1] = True
                if buzzer.turn_on[0] == True or buzzer.turn_on[1] == True:
                    self.control_actuator(ACommand(target= ACommand.Type.BUZZER, raw_message_body= '{"value": "on"}'))
        else:   # we want buzzer off
            if buzzer is not None:
                buzzer.turn_on[1] = False
                if buzzer.turn_on[0] == False and buzzer.turn_on[1] == False:
                    self.control_actuator(ACommand(target= ACommand.Type.BUZZER, raw_message_body= '{"value": "off"}'))
 
        return readings

    def get_door_state(self):
        # loop through self.sensor and find the door sensor
        # return the door sensor state
        for sensor in self.sensors:
            if sensor.type == AReading.Type.MAGNETIC_DOOR:
                return sensor._current_reading
    
    def get_door_lock_state(self):
        for actuator in self.actuators:
            if actuator.type == ACommand.Type.DOOR_LOCK_ACTUATOR:
                return actuator._current_state

    def get_motion_sensor_state(self):
        for sensor in self.sensors:
            if sensor.type == AReading.Type.MOTION:
                return sensor._current_reading
                
    def control_actuator(self, command: ACommand) -> list[AReading]:
        
        for actuator in self.actuators:
            if(actuator.validate_command(command)):
                return actuator.control_actuator(command.data)
        
        return []
    
    def set_sensor_thresholds(self, thresholds: dict[str, float]):
        pass
            
        

if __name__ == "__main__":
    luminosity = Luminosity()
    magneticDoorSensor = MagneticDoorSensor(18)
    motion = Motion(16)
    noise = Noise(2)
    sensors: list[ISensor] = [luminosity, magneticDoorSensor, motion, noise]
        
    buzzer = Buzzer()
    actuators: list[IActuator] = [buzzer]
    

    buzzer_command_on = ACommand(target=ACommand.Type.BUZZER, raw_message_body='{"value": "on"}')
    buzzer_command_off = ACommand(target=ACommand.Type.BUZZER, raw_message_body='{"value": "off"}')
    
    fake_servo_unlock_message_body = '{"value": "open"}'
    fake_servo_lock_message_body = '{"value": "closed"}'
    fake_servo_unlock_command=ACommand(ACommand.Type.DOOR_LOCK_ACTUATOR,fake_servo_unlock_message_body)
    fake_servo_lock_command=ACommand(ACommand.Type.DOOR_LOCK_ACTUATOR,fake_servo_lock_message_body)
    
