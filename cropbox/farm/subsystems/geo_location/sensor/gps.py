from ...interfaces.ISensor import ISensor, AReading
import serial
import pynmea2
from time import sleep
import json

class GPS(ISensor):
    def __init__(self, gpio: int = -1, model: str = "GPS (Air530)", type: AReading.Type = AReading.Type.COORDINATES):
        self.device = serial.Serial('/dev/ttyAMA0', 9600, timeout=1)
        self.model= model
        self.type = type
        self.gpio = gpio

    def read_sensor(self) -> list[AReading]:
        while True:
            try:
                line = self.device.readline().decode('latin-1')
                if (line not in "" and len(line) > 0 and "GNGGA" in line):
                    line = line.split("GNGGA")
                    line = str(line[1].rstrip())
                    if(",N" not in line and ",W" not in line):
                        raise GPSModuleNotConnectedException("The GPS could not read satellite data. An empty dataset has been returned.")  
                    parsed_data = pynmea2.parse("$GNGGA" + line)
                    return [AReading(type=AReading.Type.COORDINATES, unit=AReading.Unit.UNIT, value=json.dumps({"Latitude": parsed_data.latitude, "Longitude": parsed_data.longitude}))]
            except UnicodeDecodeError as e:
                line = ""
            except GPSModuleNotConnectedException as e:
                print(e)
                return [AReading(type=AReading.Type.COORDINATES, unit=AReading.Unit.UNIT, value=json.dumps(""))]
            except serial.serialutil.SerialException as e:
                print(e)
                self.device = serial.Serial('/dev/ttyAMA0', 9600, timeout=1)

class GPSModuleNotConnectedException(Exception):
    pass

if __name__ == "__main__":

    g = GPS()

    while True:
        readings = g.read_sensor()
        for reading in readings:
            print(reading)
        sleep(1)