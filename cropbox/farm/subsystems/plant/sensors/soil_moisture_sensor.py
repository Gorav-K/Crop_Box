from grove.grove_moisture_sensor import GroveMoistureSensor
import time

from grove.adc import ADC
from ...interfaces.ISensor import ISensor, AReading

#this is an analog/adc class

class MoistureSensor(ISensor):
    def __init__(self, gpio: int = 0x04, channel: int = 0, model: str = "", type: AReading.Type = AReading.Type.SOIL_MOISTURE):
        self._sensor_model = model
        self.reading_type = type
        self.channel = channel
        self.sensor = ADC(gpio)
        self.moisture_threshold = 0
        self.warning = 0
        

    def read_sensor(self) -> list[AReading]:
        # value = self.sensor.moisture
        value = self.sensor.read(self.channel) # returns voltage in mV
        
        self.evaluate_warning(value = value)

        moisture = AReading(
            AReading.Type.SOIL_MOISTURE,
            AReading.Unit.SOIL_MOISTURE_PERCENTAGE,
            value
            )
        
        return [ moisture ]

    def evaluate_warning(self, value: int):
        # if value is greater than or equal to threshold, warning is 2
        if value >= self.moisture_threshold:
            self.warning = 2
        # if value is 20% from the threshold, wanring is 1
        elif value >= self.moisture_threshold * 0.8:
            self.warning = 1
        # if value is less than 20% from the threshold, warning is 0
        else:
            self.warning = 0

    def set_threshold(self, thresholds: dict[str, int]):
        self.moisture_threshold = thresholds["moistureThreshold"]

def main():
    sensor = MoistureSensor(
        0,
        "GroveMoistureSensor",
        AReading.Type.SOIL_MOISTURE
        )
    while True:
        print(sensor.read_sensor())
        time.sleep(1)

if __name__ == "__main__":
    main()
