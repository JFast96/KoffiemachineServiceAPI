import type {
  Machine,
  MaintenanceAction,
  PagedResult,
  CreateMachineRequest,
  UpdateStatusRequest,
  CreateMaintenanceRequest
} from '@/types'

const BASE_URL = import.meta.env.VITE_API_BASE_URL || ''

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${BASE_URL}${url}`, {
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers
    },
    ...options
  })

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: response.statusText }))
    throw new ApiError(response.status, error.message || 'An error occurred')
  }

  return response.json()
}

export class ApiError extends Error {
  constructor(
    public status: number,
    message: string
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

// --- Machines ---

export function getMachines(
  page = 1,
  pageSize = 20,
  status?: string
): Promise<PagedResult<Machine>> {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  if (status) params.set('status', status)
  return request<PagedResult<Machine>>(`/api/machines?${params}`)
}

export function getMachine(id: string): Promise<Machine> {
  return request<Machine>(`/api/machines/${id}`)
}

export function createMachine(data: CreateMachineRequest): Promise<Machine> {
  return request<Machine>('/api/machines', {
    method: 'POST',
    body: JSON.stringify(data)
  })
}

export function updateMachineStatus(id: string, data: UpdateStatusRequest): Promise<Machine> {
  return request<Machine>(`/api/machines/${id}/status`, {
    method: 'PUT',
    body: JSON.stringify(data)
  })
}

// --- Maintenance ---

export function getMaintenanceActions(
  machineId: string,
  page = 1,
  pageSize = 50,
  performedBy?: string
): Promise<PagedResult<MaintenanceAction>> {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  if (performedBy) params.set('performedBy', performedBy)
  return request<PagedResult<MaintenanceAction>>(
    `/api/machines/${machineId}/maintenance?${params}`
  )
}

export function createMaintenanceAction(
  machineId: string,
  data: CreateMaintenanceRequest
): Promise<MaintenanceAction> {
  return request<MaintenanceAction>(`/api/machines/${machineId}/maintenance`, {
    method: 'POST',
    body: JSON.stringify(data)
  })
}
