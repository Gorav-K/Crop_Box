import time
from ...interfaces.ISensor import ISensor,AReading
from gpiozero import Button

class MagneticDoorSensor (ISensor):
    def __init__(self, gpio: int = 24, model: str = "Magnetic door sensor reed switch" ,
                type: AReading.Type = AReading.Type.MAGNETIC_DOOR):
        self.sensor = Button(gpio)
        self._sensor_model = model
        self.type = type
        self._current_reading = None
    def read_sensor(self) -> list[AReading]:
        '''
        Get 
        Returns:
        '''  
        if self.sensor.is_pressed:
            doorState=AReading(self.type, AReading.Unit.MATNETIC_DOOR, AReading.State.CLOSED.value)
            self._current_reading = AReading.State.CLOSED.value
            return [doorState]
        else:
            doorState=AReading(self.type, AReading.Unit.MATNETIC_DOOR, AReading.State.OPEN.value)
            self._current_reading = AReading.State.OPEN.value
            return [doorState]
    
    
if __name__ == '__main__':
    magneticDoorSensor = MagneticDoorSensor(18)
    while True:
        print(magneticDoorSensor.read_sensor())
        time.sleep(.5)