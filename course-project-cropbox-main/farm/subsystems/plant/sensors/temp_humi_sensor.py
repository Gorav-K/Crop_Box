from grove.grove_temperature_humidity_aht20 import GroveTemperatureHumidityAHT20 
import time

from ...interfaces.ISensor import ISensor, AReading

#0x38 is the default address for AHT20
#4 is the default i2c bus for Raspberry Pi

class AHT20Sensor(ISensor):
    def __init__(self, gpio: int = 0x38, bus: int = 4, model: str = "", type: AReading.Type = AReading.Type.SOIL_MOISTURE):
        self._sensor_model = model
        self.reading_type = type
        self.bus = bus
        self.sensor = GroveTemperatureHumidityAHT20(gpio, bus)
        self.humi_threshold = 0
        self.temp_threshold = 0
        self.warning = 0
        self.fan = False

    def read_sensor(self) -> list[AReading]:
        sensor_tuple = self.sensor.read() # get tuple (temperature, humidity)

        self.evaluate_warning(sensor_tuple = sensor_tuple)

        temperature = AReading(
            AReading.Type.TEMPERATURE,
            AReading.Unit.CELCIUS,
            sensor_tuple[0]
            )
        humidity = AReading(
            AReading.Type.HUMIDITY,
            AReading.Unit.HUMIDITY,
            sensor_tuple[1]
            )

        return [temperature, humidity]

    def evaluate_warning(self, sensor_tuple: tuple[float, float]):
        # if value is greater than or equal to threshold, warning is 2
        if sensor_tuple[0] >= self.temp_threshold or sensor_tuple[1] >= self.humi_threshold:
            self.warning = 2
            if sensor_tuple[0] >= self.temp_threshold:
                self.fan = True
        # if value is 20% from the threshold, wanring is 1
        elif sensor_tuple[0] >= self.temp_threshold * 0.8 or sensor_tuple[1] >= self.humi_threshold * 0.8:
            self.warning = 1
            if sensor_tuple[0] >= self.temp_threshold * 0.8:
                self.fan = True
        # if value is less than 20% from the threshold, warning is 0
        else:
            self.warning = 0
            if sensor_tuple[0] < self.temp_threshold:
                self.fan = False

    def set_threshold(self, thresholds: dict[str, int]):
        self.temp_threshold = thresholds["temperatureThreshold"]
        self.humi_threshold = thresholds["humidityThreshold"]

def main():
    sensor = AHT20Sensor(
        4,
        "AHT20",
        AReading.Type.TEMPERATURE
        )
    while True:
        print(sensor.read_sensor())
        time.sleep(1)

if __name__ == "__main__":
    main()