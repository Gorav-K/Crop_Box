from grove.grove_water_sensor import GroveWaterSensor
import time

from grove.adc import ADC
from ...interfaces.ISensor import ISensor, AReading

# this is an analog/adc class 

class WaterSensor(ISensor):
    def __init__(self, gpio: int = 0x04, channel: int = 2, model: str = "", type: AReading.Type = AReading.Type.WATER_DEPTH):
        self._sensor_model = model
        self.reading_type = type
        self.channel = channel
        self.sensor = ADC(gpio)
        self.water_depth_threshold = 0
        self.warning = 0

    def read_sensor(self) -> list[AReading]:
        value = self.sensor.read(self.channel)

        self.evaluate_warning(value = value)

        water_level = AReading(
            AReading.Type.WATER_DEPTH,
            AReading.Unit.MILLIMETERS,
            value
            )

        return [water_level]

    def evaluate_warning(self, value: int):
        if value >= self.water_depth_threshold:
            self.warning = 2
        elif value >= self.water_depth_threshold * 0.8:
            self.warning = 1
        else:
            self.warning = 0

    def set_threshold(self, thresholds: dict[str, int]):
        self.water_depth_threshold = thresholds["waterDepthThreshold"]

def main():
    sensor = WaterSensor(
        6,
        "GroveWaterSensor",
        AReading.Type.WATER_DEPTH
        )
    while True:
        print(sensor.read_sensor())
        time.sleep(1)

if __name__ == "__main__":
    main()