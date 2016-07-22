interface DeviceProps {
    deviceID: string;
    hubEnabledState: boolean;
    createdTime: string;
    deviceState: string;
    updatedTime: string;
    manufacturer?: string;
    modelNumber?: string;
    serialNumber?: string;
    firmwareVersion?: string;
    platform?: string;
    processor?: string;
    installedRAM?: string;
    latitude?: number;
    longitude?: number;
}

interface SystemProps {
    iccid: string;
}

interface CommandParameter {
    name: string;
    type: string;
}

interface Command {
    name: string;
    parameters: CommandParameter[];
}

interface Parameter {}

interface CommandRun {
    name: string;
    messageId: string;
    createdTime: string;
    parameters?: Parameter[];
    updatedTime?: string;
    result?: string;
    errorMessage?: string;
}

interface Sensor {
    name: string;
    displayName: string;
    type: string;
}

interface IoTHubInfo {
    messageId: string;
    correlationId: string;
    connectionDeviceId: string;
    connectionDeviceGenerationId: string;
    enqueuedTime: string;
    streamId: string;
}

interface DeviceInfo {
    deviceProperties: DeviceProps;
    systemProperties: SystemProps;
    commands: Command[];
    commandHistory: CommandRun[];
    isSimulatedDevice: boolean;
    id: string;
    _rid: string; 
    _self: string;
    _etag: string;
    _ts: number;
    _attachments: string;
    telemetry?: Sensor[];
    version?: string;
    objectType?: string;
    ioTHub?: IoTHubInfo;
}

interface SingleDevice {
    data: DeviceInfo;
}

interface Devices {
    data: DeviceInfo[];
}

interface HubKeys {
    primaryKey: string;
    secondaryKey: string;
}