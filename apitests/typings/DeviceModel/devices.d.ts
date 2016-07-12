interface DeviceProps {
    DeviceID: string;
    HubEnabledState: boolean;
    CreatedTime: string;
    DeviceState: string;
    UpdatedTime: string;
    Manufacturer?: string;
    ModelNumber?: string;
    SerialNumber?: string;
    FirmwareVersion?: string;
    Platform?: string;
    Processor?: string;
    InstalledRAM?: string;
    Latitude?: number;
    Longitude?: number;
}

interface SystemProps {
    ICCID: string;
}

interface CommandParameter {
    Name: string;
    Type: string;
}

interface Command {
    Name: string;
    Parameters: CommandParameter[];
}

interface Parameter {}

interface CommandRun {
    Name: string;
    MessageId: string;
    CreatedTime: string;
    Parameters: Parameter[];
    UpdatedTime: string;
    Result: string;
    ErrorMessage: string;
}

interface Sensor {
    Name: string;
    DisplayName: string;
    Type: string;
}

interface IoTHubInfo {
    MessageId: string;
    CorrelationId: string;
    ConnectionDeviceId: string;
    ConnectionDeviceGenerationId: string;
    EnqueuedTime: string;
    StreamId: string;
}

interface DeviceInfo {
    DeviceProperties: DeviceProps;
    SystemProperties: SystemProps;
    Commands: Command[];
    CommandHistory: CommandRun[];
    IsSimulatedDevice: boolean;
    id: string;
    _rid: string; 
    _self: string;
    _etag: string;
    _ts: number;
    _attachments: string;
    Telemetry?: Sensor[];
    Version?: string;
    ObjectType?: string;
    IoTHub?: IoTHubInfo;
}

interface Devices {
    data: DeviceInfo[];
}