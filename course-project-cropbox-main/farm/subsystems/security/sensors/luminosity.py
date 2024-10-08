from ...interfaces.ISensor import ISensor,AReading
import seeed_python_reterminal.core as rt
import time


class Luminosity (ISensor):
    def __init__(self, gpio: int = -1, model: str = "Luminosity" ,type: AReading.Type = AReading.Type.LUMINOSITY):
        self.sensor = rt
        self._sensor_model = model
        self.type = type
    
    
    def read_sensor(self) -> list[AReading]:
        illuminance = AReading(self.type, AReading.Unit.LUMINOSITY,self.sensor.illuminance)
        return [illuminance]
   
 
if __name__ == '__main__':
    luminosity = Luminosity()
    while True:
        luminosity.read_sensor()
        time.sleep(.5)