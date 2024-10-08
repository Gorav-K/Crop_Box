import seeed_python_reterminal.core as rt
from ...interfaces.IActuator import IActuator, ACommand
from ...interfaces.ISensor import AReading, ISensor


from time import sleep

class Buzzer(IActuator, ISensor):
        def __init__(self, gpio: int = -1, type: ACommand.Type = ACommand.Type.BUZZER, initial_state: dict = {'value': 'off'}) -> None:
            self.sensor = rt
            self.type: ACommand.Type = type
            self._current_state: dict = initial_state
            self.turn_on = [False, False]
        
        def validate_command(self, command: ACommand) -> bool:
            
            if command.target_type == ACommand.Type.BUZZER:
                if command.data["value"].lower() == "on" or command.data["value"].lower() == "off":
                    return True
            
            return False
        
        def on(self): self.sensor.buzzer = True

        def off(self): self.sensor.buzzer = False

        def control_actuator(self, data: dict) -> list[AReading]:
            
            if(str(data['value']).lower() == 'off'):
                self.off()
            else: self.on()
            self._current_state = data
            
            return self.read_sensor()

        def read_sensor(self) -> list[AReading]:
            
            state = AReading(AReading.Type.BUZZER, AReading.Unit.UNIT, str(self._current_state))
            return [state]
        

if __name__ == "__main__":

    b = Buzzer()
    while True:
        if b.validate_command(ACommand(ACommand.Type.BUZZER, '{"value": "on"}')):
            b.control_actuator({'value': 'on'})
            sleep(1)
        if b.validate_command(ACommand(ACommand.Type.BUZZER, '{"value": "off"}')):
            b.control_actuator({'value': 'off'})
            sleep(1)