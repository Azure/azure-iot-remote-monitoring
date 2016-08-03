interface AlertHistory {
    totalAlertCount: number;
    totalFilteredCount: number;
    data: Alert[];
    devices: DeviceAlert[];
    maxLatitude: number;
    maxLongitude: number;
    minLatitude: number;
    minLongitude: number;
}

interface DeviceAlert {
    deviceId: string;
    latitude: number;
    longitude: number;
    status: number;
}

interface Alert {
    deviceId: string;
    ruleOutput: string;
    timestamp: string;
    value: string;
}

interface DeviceLocationData {
    deviceLocationList: DeviceLocation[];
    minimumLatitude: number;
    maximumLatitude: number;
    minimumLongitude: number;
    maximumLongitude: number;
}

interface DeviceLocation {
    deviceId: string;
    latitude: number;
    longitude: number;
}

interface TelemetrySummary {
    averageHumidity: number;
    deviceId: string;
    maximumHumidity: number;
    minimumHumidity: number;
    timeFrameMinutes: number;
    timestamp: string;
}

interface DeviceTelemetry {
    deviceId: string;
    values: TelemetryValues;
    timestamp: string;
}

interface TelemetryValues {
    temperature: number;
    humidity: number;
}

interface DevicePaneData {
    deviceId: string;
    deviceTelemetryModels: DeviceTelemetry[];
    deviceTelemetrySummaryModel: TelemetrySummary;
    deviceTelemetryFields: TelemetryField[];
}

interface TelemetryField {
    displayName: string;
    name: string;
    type: string;
}