import asyncio
from typing import Callable

from subsystems.interfaces.ISensor import AReading
from subsystems.interfaces.IActuator import ACommand

from azure.iot.device.aio import IoTHubDeviceClient
from azure.iot.device import MethodResponse, Message
from dotenv import load_dotenv
import os
import json
from datetime import datetime  

class ConnectionConfig:
    DEVICE_CONN_STR = "IOTHUB_DEVICE_CONNECTION_STRING"

    def __init__(self, device_str: str) -> None:
        self._device_connection_str = device_str


class ConnectionManager:
    def __init__(self) -> None:
        try:
            self._connected = False
            
            self._config: ConnectionConfig = self._load_connection_config()
            self._client = IoTHubDeviceClient.create_from_connection_string(
                self._config._device_connection_str)

            self.telemetry_interval = 1 #to demontrate that this value will be changed when the set_telemetry_interval method will be called
        except:
            print("Error in ConnectionManager")

    def _load_connection_config(self) -> ConnectionConfig:
        load_dotenv()
        device_connection_string = str(os.getenv(ConnectionConfig.DEVICE_CONN_STR))

        return ConnectionConfig(device_connection_string)

    async def get_thresholds_from_twin(self):
        twin = await self._client.get_twin()
        return twin['desired']

    def _on_message_received(self, message: Message) -> None:
        try:
            if "command-type" in message.custom_properties:
                command_type = message.custom_properties["command-type"]
                command = ACommand(ACommand.Type(command_type), message.data)
                self._command_callback(command)
        except:
            print("Error parsing command")
    
    async def set_telemetry_interval(self):
        twin = await self._client.get_twin()
        thresholds = twin['desired']
        self.telemetry_interval = thresholds['telemetryInterval']

    async def set_callback_methods(self):
        self._client.on_method_request_received = self.method_request_handler
        self._client.on_twin_desired_properties_patch_received = self.twin_patch_handler

    async def method_request_handler(self, method_request):
        try:

            if method_request.name == "actuate_fan":
                type = ACommand.Type.FAN
                status = 200  
            elif method_request.name == "actuate_rgbled":
                type = ACommand.Type.RGB_LED
                status = 200  
            elif method_request.name == "actuate_buzzer":
                type = ACommand.Type.BUZZER
                status = 200  
            elif method_request.name == "actuate_lock":
                type = ACommand.Type.DOOR_LOCK_ACTUATOR
                status = 200  
            else:
                status = 400 
            
            if(status == 200): 
                print("executed " + method_request.name)
                direct_method_payload = method_request.payload
                if isinstance(direct_method_payload, dict): direct_method_payload = str(direct_method_payload)
                
                reading: list[AReading] = self._command_callback(ACommand(type, direct_method_payload.replace("\"", "").replace("\'", "\"")))
                
                state = json.loads(reading[0].value.replace("\"", "").replace("\'", "\""))
                parsed_reading = json.loads(reading[0].export_json().replace("\"", "").replace("\'", "\""))

                print(f"Actuator {reading[0].reading_type.value.lower()} current state set to {state['value']}.")

                payload = { 
                    "details": f"method {method_request.name} finished", 
                    "reading": parsed_reading,
                    "currentState": state['value'],
                    "timeStamp": str(datetime.now())
                    }


            elif(status == 400): 
                print(f"unknown method: {method_request.name}")
                payload = { "details": "method name unknown" }

            method_response = MethodResponse.create_from_method_request(method_request, status, payload)
            await self._client.send_method_response(method_response)
        except Exception as e:
            payload = { "details": f"Error in method_request_handler: {e}" }
            status = 400
            method_response = MethodResponse.create_from_method_request(method_request, status, payload)
            await self._client.send_method_response(method_response)
            print(f"Error in method_request_handler: {e}")

    async def twin_patch_handler(self, patch):
        self.telemetry_interval = patch["telemetryInterval"]
        print("Current value of telemetryInterval: " + str(self.telemetry_interval))
        reported_patch = {
                "telemetryInterval": self.telemetry_interval
            }
        await self._client.patch_twin_reported_properties(reported_patch)

    async def connect(self) -> None:
        await self._client.connect()
        self._connected = True
        print("Connected")
        self._client.on_message_received = self._on_message_received

    def register_command_callback(self, command_callback: Callable[[ACommand], None]) -> None:
        self._command_callback = command_callback

    async def send_readings(self, readings: list[AReading]) -> None:
        for reading in readings:
            message = Message(reading.export_json())
            message.custom_properties["reading-type"] = reading.reading_type.name
            
            await self._client.send_message(message)


async def main_demo():

    def dummy_callback(command: ACommand):
         print(command)

    connection_manager = ConnectionManager()
    connection_manager.register_command_callback(dummy_callback)
    await connection_manager.connect()

    TEST_SLEEP_TIME = 3
    while True:

        # ===== Create a list of fake readings =====
        fake_temperature_reading = AReading(
            AReading.Type.TEMPERATURE, AReading.Unit.CELCIUS, 12.34)
        fake_humidity_reading = AReading(
            AReading.Type.HUMIDITY, AReading.Unit.HUMIDITY, 56.78)

        # ===== Send fake readings =====
        await connection_manager.send_readings([
            fake_temperature_reading,
            fake_humidity_reading
        ])

        await asyncio.sleep(TEST_SLEEP_TIME)

if __name__ == "__main__":
    asyncio.run(main_demo())
