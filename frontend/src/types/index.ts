export interface Machine {
  id: string
  name: string
  location: string
  status: string
  rowVersion: string
  createdAt: string
  updatedAt: string | null
}

export interface MaintenanceAction {
  id: string
  machineId: string
  description: string
  performedBy: string
  performedAt: string
}

export interface PagedResult<T> {
  totalRecords: number
  pageSize: number
  currentPage: number
  totalPages: number
  data: T[]
}

export interface CreateMachineRequest {
  name: string
  location: string
  status: string
}

export interface UpdateStatusRequest {
  status: string
  rowVersion: string
}

export interface CreateMaintenanceRequest {
  description: string
  performedBy: string
}
