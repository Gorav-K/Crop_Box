from abc import ABC, abstractmethod
from .IActuator import ACommand, IActuator
from .ISensor import AReading, ISensor

class ISubsystem(ABC):
    @abstractmethod
    def __init__(self, name: str, sensors: list[ISensor], actuators: list[IActuator]) -> None:
        pass
    
    @abstractmethod
    def read_sensors(self) -> list[AReading]:
        pass

    @abstractmethod
    def control_actuator(self, command: ACommand) -> list[AReading]:
        pass

    @abstractmethod
    def set_sensor_thresholds(self, thresholds: dict[str, int]):
        pass

