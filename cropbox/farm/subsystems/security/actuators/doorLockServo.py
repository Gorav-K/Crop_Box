from gpiozero import Servo
from time import sleep
from gpiozero.pins.pigpio import PiGPIOFactory
from ...interfaces.IActuator import IActuator, ACommand
from ...interfaces.ISensor import AReading, ISensor

class LockServo(IActuator, ISensor):
    def __init__(self,gpio: int =12 , type: ACommand.Type = ACommand.Type.DOOR_LOCK_ACTUATOR,
                 initial_state: dict = {'value': 'on'}) -> None:
        #factory = PiGPIOFactory() May be used at a later time
        self.servo= Servo(gpio,min_pulse_width=0.5/1000,max_pulse_width=2.5/1000)
        self.servo.max()
        self._current_state = initial_state
        self.type = type

    def validate_command(self, command: ACommand) -> bool:
        
        if command.target_type == ACommand.Type.DOOR_LOCK_ACTUATOR:
            if command.data["value"].lower() == "off" or command.data["value"].lower() == "on":
                return True
        return False
        
    def control_actuator(self, data: dict) -> list[AReading]:
        value = data["value"].lower()
        if value=="on": # equal to closed/locked
            self._current_state = data
            self.servo.min()
        elif value=="off": # equal to open/unlocked
            self._current_state = data
            self.servo.max()

        return self.read_sensor()

    
    def read_sensor(self) -> list[AReading]:
        state = AReading(AReading.Type.DOOR_LOCK_ACTUATOR, AReading.Unit.UNIT, str(self._current_state))
        return [state]


            
if __name__ == '__main__':
    servo = LockServo(12,ACommand.Type.DOOR_LOCK_ACTUATOR,{"value": "closed"})
    fake_servo_unlock_message_body = '{"value": "open"}'
    fake_servo_lock_message_body = '{"value": "closed"}'
    
    while True:
        if servo.validate_command(ACommand(ACommand.Type.DOOR_LOCK_ACTUATOR,fake_servo_unlock_message_body)):
            servo.control_actuator({"value": "open"})
            sleep(5)
        
        if servo.validate_command(ACommand(ACommand.Type.DOOR_LOCK_ACTUATOR,fake_servo_lock_message_body)):
            servo.control_actuator({"value": "closed"})
            sleep(5)
    