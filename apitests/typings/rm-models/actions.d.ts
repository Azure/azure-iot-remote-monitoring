interface DeviceActionsWithCount {
    draw: number;
    recordsTotal: number;
    recordsFiltered: number;
    data: Action[];
}

interface DeviceActions {
    data: Action[];
}

interface Action {
    numberOfDevices: number;
    ruleOutput: string;
    actionId: string
}

interface Rules {
    data: string[]
}

interface ActionId {
    data: string
}
