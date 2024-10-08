import asyncio
from time import sleep
from threading import Thread
from connection_manager import ConnectionManager
from device_controller import DeviceController

class Farm:
    def __init__(self):
        self.thresholds = {}

    async def send_readings(self, connection_manager, device_controller):
        while True:
            thresholds = await connection_manager.get_thresholds_from_twin()
            self.thresholds = thresholds
            device_controller.set_thresholds(thresholds)
            readings = device_controller.read_sensors()
            await connection_manager.send_readings(readings)
            print('==========================================')
            print("Sent readings")
            print('==========================================')
            await asyncio.sleep(connection_manager.telemetry_interval + 1)

    async def main(self):
        connection_manager = ConnectionManager()
        device_controller = DeviceController()

        await connection_manager.connect()
        await connection_manager.set_telemetry_interval()
        await connection_manager.set_callback_methods()
        connection_manager.register_command_callback(device_controller.control_actuator)

        thresholds = await connection_manager.get_thresholds_from_twin()
        self.thresholds = thresholds
        device_controller.set_thresholds(thresholds)

        asyncio.create_task(self.send_readings(connection_manager, device_controller))

        while True:
            device_controller.set_thresholds(self.thresholds)
            readings = device_controller.read_sensors()
            print('local readings')
            await asyncio.sleep(0.5)


if __name__ == "__main__":
    farm = Farm()
    asyncio.run(farm.main())