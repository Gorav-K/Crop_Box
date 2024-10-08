from ...interfaces.ISensor import ISensor, AReading
import seeed_python_reterminal.core as rt
import seeed_python_reterminal.acceleration as rt_accel
from enum import Enum
import math
from time import sleep

class Accelerometer(ISensor):
    def __init__(self, gpio: int = -1, model: str = "reterminal built-in accelerometer", type: AReading.Type = AReading.Type.ANGLE):
        self.device = rt.get_acceleration_device()
        self.model= model
        self.type = type
        self.gpio = gpio
        self.pitch_threshold = 0
        self.roll_threshold = 0
        self.vibration_threshold = 0
        self.warning = 0

    def get_xyz_readings(self) -> list[float]:
        x_reading = None
        y_reading = None
        z_reading = None

        for event in self.device.read_loop():
            accelEvent = rt_accel.AccelerationEvent(event)
            if(x_reading is not None and y_reading is not None and z_reading is not None):
               return [x_reading, y_reading, z_reading] 
            elif(accelEvent.name is not None):
                if(accelEvent.name.value == AccelerationName.X.value and x_reading is None):
                    x_reading = accelEvent.value
                elif(accelEvent.name.value == AccelerationName.Y.value and y_reading is None):
                    y_reading = accelEvent.value
                elif(accelEvent.name.value == AccelerationName.Z.value and z_reading is None):
                    z_reading = accelEvent.value

    def read_sensor(self) -> list[AReading]:
        readings_1 = self.get_xyz_readings()
        pitch = 180 * math.atan2(readings_1[AccelerationName.X.value], math.sqrt(readings_1[AccelerationName.Y.value]*readings_1[AccelerationName.Y.value] + readings_1[AccelerationName.Z.value]*readings_1[AccelerationName.Z.value]))/math.pi
        roll = 180 * math.atan2(readings_1[AccelerationName.Y.value], math.sqrt(readings_1[AccelerationName.X.value]*readings_1[AccelerationName.X.value] + readings_1[AccelerationName.Z.value]*readings_1[AccelerationName.Z.value]))/math.pi
        
        commands = [AReading(type=AReading.Type.PITCH, unit=AReading.Unit.DEGREES, 
                                    value= str(pitch)),
                AReading(type=AReading.Type.ROLL, unit=AReading.Unit.DEGREES, 
                         value=str(roll))] 
        
        sleep(1)

        readings_2 = self.get_xyz_readings()
        vibration = math.sqrt((readings_2[AccelerationName.X.value] - readings_1[AccelerationName.X.value])**2 + (readings_2[AccelerationName.Y.value] - readings_1[AccelerationName.Y.value])**2 + (readings_2[AccelerationName.Z.value] - readings_1[AccelerationName.Z.value])**2)

        commands.append(AReading(type=AReading.Type.VIBRATION, unit=AReading.Unit.UNIT, 
                                        value=str(vibration)))

        self.evaluate_warnings(pitch, roll, vibration)

        return commands
    
    def evaluate_warnings(self, pitch, roll, vibration):
        if pitch >= self.pitch_threshold or roll >= self.roll_threshold or vibration >= self.vibration_threshold:
            self.warning = 2
        elif pitch >= self.pitch_threshold * 0.8 or roll >= self.roll_threshold * 0.8 or vibration >= self.vibration_threshold * 0.8:
            self.warning = 1
        else:
            self.warning = 0
        
    def set_threshold(self, thresholds: dict[str, int]):
        self.pitch_threshold = thresholds["pitchThreshold"]
        self.roll_threshold = thresholds["rollThreshold"]
        self.vibration_threshold = thresholds["vibrationThreshold"]

class AccelerationName(Enum):
    X = 0
    Y = 1
    Z = 2

if __name__ == "__main__":
    a = Accelerometer()

    while True:
        readings = a.read_sensor()
        for reading in readings:
            print(reading)
        sleep(1)