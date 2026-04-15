import type {
  Machine,
  MaintenanceAction,
  PagedResult,
  CreateMachineRequest,
  UpdateStatusRequest,
  CreateMaintenanceRequest
} from '@/types'

const BASE_URL = import.meta.env.VITE_API_BASE_URL || ''

// --- Token management ---

let authToken: string | null = localStorage.getItem('jwt_token')

export function setToken(token: string) {
  authToken = token
  localStorage.setItem('jwt_token', token)
}

export function clearToken() {
  authToken = null
  localStorage.removeItem('jwt_token')
}

export function getToken(): string | null {
  return authToken
}

// --- Base request ---

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json'
  }

  if (authToken) {
    headers['Authorization'] = `Bearer ${authToken}`
  }

  const { headers: customHeaders, ...restOptions } = options || {}
  const response = await fetch(`${BASE_URL}${url}`, {
    ...restOptions,
    headers: {
      ...headers,
      ...(customHeaders as Record<string, string>)
    }
  })

  if (response.status === 401) {
    clearToken()
    window.dispatchEvent(new CustomEvent('auth:expired'))
    throw new ApiError(401, 'Session expired. Please log in again.')
  }

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

// --- Auth ---

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  token: string
  expiresAt: string
}

export async function login(data: LoginRequest): Promise<LoginResponse> {
  const response = await fetch(`${BASE_URL}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  })

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: response.statusText }))
    throw new ApiError(response.status, error.message || 'Login failed')
  }

  const result: LoginResponse = await response.json()
  setToken(result.token)
  return result
}

export function logout() {
  clearToken()
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
