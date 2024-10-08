from time import sleep

from subsystems.interfaces.ISensor import AReading
from subsystems.interfaces.IActuator import ACommand
from subsystems.interfaces.ISubsystem import ISubsystem
from subsystems.geo_location.geoLocationSubsystem import GeoLocationSubsystem
from subsystems.plant.plantSubsystem import PlantSubsystem
from subsystems.security.securitySubsystem import SecuritySubsystem
from subsystems.security.actuators.buzzer import Buzzer

class DeviceController:

    def __init__(self, subsystems: list[ISubsystem] = [ PlantSubsystem() ]) -> None:
        self.subsystems : list[ISubsystem] = subsystems
        self.shared_components = [Buzzer()]

        for subSystem in self.subsystems:
            if hasattr(subSystem, 'has_shared_components'):
                subSystem.sensors.extend(self.shared_components)
                subSystem.actuators.extend(self.shared_components)

    def read_sensors(self) -> list[AReading]:
        readings: list[AReading] = []

        for subSystem in self.subsystems:
            readings.extend(subSystem.read_sensors())

        for reading in readings:
            print(reading)

        return readings

    def set_thresholds(self, thresholds: dict[str, int]):
        for subsystem in self.subsystems:
            subsystem.set_sensor_thresholds(thresholds)

    def control_actuator(self, command: ACommand) -> list[AReading]:
        readings: list[AReading] = []

        for subSystem in self.subsystems:
            readings.extend(subSystem.control_actuator(command))
        return readings
"""This script is intented to be used as a module, however, code below can be used for testing.
"""

if __name__ == "__main__":

    print("Demo available in farm.py")
