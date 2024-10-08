from grove.grove_ws2813_rgb_led_strip import GroveWS2813RgbStrip
from time import sleep
from ...interfaces.IActuator import IActuator, ACommand
from rpi_ws281x import Color
from ...interfaces.ISensor import AReading, ISensor
from enum import Enum

class RGBColor(Enum):
        """Enum defining all colors to use for the ws281x rgb led.
        """
        # Add new colors here.
        RED = Color(255, 0, 0)          # immediate problem
        GREEN = Color(0, 255, 0)        # current value good
        BLUE = Color(0, 0, 255)         # standby (?)
        WHITE = Color(255, 255, 255)    # on standby/power on
        ORANGE = Color(255, 150, 0)     # warning
        BLACK = Color(0, 0, 0)          # off

class RGBLedStrip(IActuator, ISensor):
    def __init__(self, gpio: int = 18, type: ACommand.Type = ACommand.Type.RGB_LED,
                 initial_state: dict = {"value": "on"} ) -> None:
        pin_count = 10
        self._light_time = 50/1000
        self.led_strip = GroveWS2813RgbStrip(gpio, pin_count)
        self.color = RGBColor.WHITE.value
        self._current_state = initial_state
        self.type = type
        self.change_color(self.color)

    def change_color(self, color: Color) -> None:
        if color is not RGBColor.BLACK.value:
            self.color = color
        for i in range(self.led_strip.numPixels()):
                    self.led_strip.setPixelColor(i, color)
                    self.led_strip.show()
                    sleep(self._light_time)
    
    def control_actuator(self, data: dict) -> list[AReading]:
        value = data["value"].lower()
        if "color" in data:
            color = data["color"].upper()
            if color in RGBColor.__members__ and value == "on":
                self.change_color(RGBColor.__members__[color].value)
            else:
                self.change_color(RGBColor.__members__[color].value)

        if value == "on":
            if self._current_state != data:
                self._current_state = data
                self.change_color(self.color)


        elif value == "off":
            if self._current_state != data:
                self._current_state = data
                self.change_color(RGBColor.BLACK.value)

        return self.read_sensor()

    def validate_command(self, command: ACommand) -> bool:
        
        if command.target_type == ACommand.Type.RGB_LED:
            if command.data["value"].lower() == "off" or command.data["value"].lower() == "on":
                return True

        return False
            
    def read_sensor(self) -> list[AReading]:
        
        state = AReading(AReading.Type.RGB_LED, AReading.Unit.UNIT, str(self._current_state))
        return [state]

def main():
    led_strip = RGBLedStrip(18,
        ACommand.Type.RGB_LED,
        {"value": "off"}
        )
    led_strip.control_actuator("on")
    sleep(2)
    led_strip.control_actuator("off")

if __name__ == '__main__':
    while True:
        main()

