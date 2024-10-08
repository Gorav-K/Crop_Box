import time
from ...interfaces.ISensor import ISensor,AReading
from gpiozero import MotionSensor 

class Motion (ISensor):
    def __init__(self, gpio: int = 16, model: str = "PIR Motion Sensor",
        type: AReading.Type = AReading.Type.MOTION):
        self.sensor = MotionSensor(gpio)
        self._sensor_model = model
        self.type = type
        self._current_reading = None

    def read_sensor(self) -> list[AReading]:
        if self.sensor.motion_detected:
            motion = AReading(self.type, AReading.Unit.MOTION, AReading.State.MOTION.value)
            self._current_reading = AReading.State.MOTION.value
            return [motion]
        else:
            motion = AReading(self.type, AReading.Unit.MOTION, AReading.State.NO_MOTION.value)
            self._current_reading = AReading.State.NO_MOTION.value
            return [motion]
                
if __name__ == '__main__':
    motion = Motion(16)
    print("Waiting for PIR to settle ...")
    motion.sensor.wait_for_no_motion()
    while True:
        print(motion.read_sensor())
        time.sleep(1)
            
        