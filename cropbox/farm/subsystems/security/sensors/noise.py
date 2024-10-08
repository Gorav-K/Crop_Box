import time
from ...interfaces.ISensor import ISensor,AReading
from grove.adc import ADC

class Noise (ISensor):
    def __init__(self, gpio: int = 0x04, channel: int = 4, model: str = "Luminosity" ,type: AReading.Type = AReading.Type.SOUND):
        self.channel = channel
        self.sensor = ADC(address = gpio)
        self._sensor_model = model
        self.type = type
    
    def read_sensor(self) -> list[AReading]:
        '''
        Get the loudness strength value, maximum value is 100.0%

        Returns:
            (int): ratio, 0(0.0%) - 1000(100.0%)
        '''         
        noise = AReading(self.type, AReading.Unit.DECIBELS,self.sensor.read(self.channel))
        return [noise]
    
    
if __name__ == '__main__':
    noise = Noise(2)
    while True:
        print(noise.read_sensor())
        time.sleep(.5)