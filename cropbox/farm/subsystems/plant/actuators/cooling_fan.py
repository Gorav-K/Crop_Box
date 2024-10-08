from gpiozero import DigitalOutputDevice
from time import sleep
from ...interfaces.IActuator import IActuator, ACommand
from ...interfaces.ISensor import AReading, ISensor

class Fan(IActuator, ISensor):
    def __init__(self, gpio: int = 5, type: ACommand.Type = ACommand.Type.FAN,
                 initial_state: dict ={"value": "off"}) -> None:
        self.fan = DigitalOutputDevice(gpio)
        self._current_state = initial_state
        self.type = type

    def control_actuator(self, command: dict) -> list[AReading]:
        if command["value"] == "on":
            if self._current_state != command:
                self._current_state = command
                self.fan.on()
                

        elif command["value"] == "off":
            if self._current_state != command:
                self._current_state = command
                self.fan.off()
                
        return self.read_sensor()

    def validate_command(self, command: ACommand) -> bool:
        if command.target_type == ACommand.Type.FAN:
            if command.data["value"].lower() == "on" or command.data["value"].lower() == "off":
                return True
        return False
    
    def read_sensor(self) -> list[AReading]:
        
        state = AReading(AReading.Type.FAN, AReading.Unit.UNIT, str(self._current_state))
        return [state]

    
def main():
    fan = Fan(5,
        ACommand.Type.FAN,
        {"value": "off"}
        )
    fan.control_actuator("on")
    sleep(2)
    fan.control_actuator("off")

if __name__ == '__main__':
    while True:
        main()