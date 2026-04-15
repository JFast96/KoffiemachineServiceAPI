<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getMachine, updateMachineStatus, ApiError } from '@/api/machineApi'
import MaintenanceLog from '@/components/MaintenanceLog.vue'
import type { Machine } from '@/types'

const props = defineProps<{ id: string }>()
const router = useRouter()

const machine = ref<Machine | null>(null)
const loading = ref(false)
const error = ref('')
const successMsg = ref('')
const newStatus = ref('')
const updating = ref(false)

const statuses = ['Actief', 'Inactief', 'Defect', 'In onderhoud']

function statusClass(status: string): string {
  const s = status.toLowerCase().replace(' ', '')
  if (s === 'actief') return 'status-actief'
  if (s === 'inactief') return 'status-inactief'
  if (s === 'defect') return 'status-defect'
  if (s === 'inonderhoud') return 'status-onderhoud'
  return ''
}

async function loadMachine() {
  loading.value = true
  error.value = ''
  try {
    machine.value = await getMachine(props.id)
    newStatus.value = machine.value.status
  } catch (e: any) {
    error.value = e.message || 'Fout bij ophalen van machine'
  } finally {
    loading.value = false
  }
}

async function handleUpdateStatus() {
  if (!machine.value || newStatus.value === machine.value.status) return

  updating.value = true
  error.value = ''
  successMsg.value = ''
  try {
    machine.value = await updateMachineStatus(props.id, {
      status: newStatus.value,
      rowVersion: machine.value.rowVersion
    })
    successMsg.value = `Status gewijzigd naar "${machine.value.status}"`
    setTimeout(() => { successMsg.value = '' }, 3000)
  } catch (e: any) {
    if (e instanceof ApiError && e.status === 409) {
      error.value = 'Conflict: deze machine is inmiddels door een andere gebruiker gewijzigd. Data wordt herladen.'
      await loadMachine()
    } else {
      error.value = e.message || 'Fout bij wijzigen van status'
    }
  } finally {
    updating.value = false
  }
}

onMounted(() => {
  loadMachine()
})
</script>

<template>
  <div>
    <button class="btn btn-ghost" @click="router.push('/')">&larr; Terug naar overzicht</button>

    <div v-if="error" class="error-message" style="margin-top: 1rem;">{{ error }}</div>
    <div v-if="successMsg" class="success-message" style="margin-top: 1rem;">{{ successMsg }}</div>
    <div v-if="loading" class="loading">Machine laden...</div>

    <div v-if="machine && !loading" class="detail-layout">
      <!-- Machine info card -->
      <div class="card machine-card">
        <div class="machine-header">
          <div>
            <h2>{{ machine.name }}</h2>
            <span class="machine-id">{{ machine.id }}</span>
          </div>
          <span class="status-badge status-badge-lg" :class="statusClass(machine.status)">
            {{ machine.status }}
          </span>
        </div>

        <div class="info-grid">
          <div class="info-item">
            <span class="info-label">Locatie</span>
            <span class="info-value">{{ machine.location }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">Aangemaakt</span>
            <span class="info-value">{{ new Date(machine.createdAt).toLocaleString('nl-NL') }}</span>
          </div>
          <div class="info-item" v-if="machine.updatedAt">
            <span class="info-label">Laatst gewijzigd</span>
            <span class="info-value">{{ new Date(machine.updatedAt).toLocaleString('nl-NL') }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">Versie</span>
            <span class="info-value info-mono">{{ machine.rowVersion.substring(0, 8) }}...</span>
          </div>
        </div>

        <!-- Status update -->
        <div class="status-section">
          <h3>Status wijzigen</h3>
          <p class="section-hint">Bij het opslaan wordt de RowVersion gecontroleerd (optimistic concurrency).</p>
          <div class="status-form">
            <select v-model="newStatus">
              <option v-for="s in statuses" :key="s" :value="s">{{ s }}</option>
            </select>
            <button
              class="btn btn-primary"
              :disabled="updating || newStatus === machine.status"
              @click="handleUpdateStatus"
            >
              {{ updating ? 'Opslaan...' : 'Status opslaan' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Maintenance log card -->
      <div class="card">
        <MaintenanceLog :machine-id="id" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.detail-layout {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  margin-top: 1.25rem;
}

.machine-card {
  padding: 2rem;
}

.machine-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1.75rem;
}

.machine-id {
  font-size: 0.75rem;
  color: var(--color-text-light);
  font-family: monospace;
  margin-top: 0.25rem;
  display: block;
}

.status-badge-lg {
  font-size: 0.85rem;
  padding: 0.35rem 1rem;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1.5rem;
  padding-bottom: 1.75rem;
  border-bottom: 1px solid var(--color-border-light);
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}

.info-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.info-value {
  font-size: 0.95rem;
  color: var(--color-text);
}

.info-mono {
  font-family: monospace;
  font-size: 0.85rem;
  color: var(--color-text-muted);
}

.status-section {
  padding-top: 1.5rem;
}

.status-section h3 {
  margin-bottom: 0.25rem;
}

.section-hint {
  font-size: 0.8rem;
  color: var(--color-text-light);
  margin-bottom: 1rem;
}

.status-form {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}

.status-form select {
  width: auto;
  min-width: 180px;
}

.success-message {
  background: #ecfdf5;
  color: #059669;
  padding: 0.875rem 1.125rem;
  border-radius: var(--radius-sm);
  margin-bottom: 1rem;
  font-size: 0.875rem;
  border: 1px solid #a7f3d0;
}

@media (max-width: 640px) {
  .machine-header {
    flex-direction: column;
    gap: 0.75rem;
  }

  .info-grid {
    grid-template-columns: 1fr;
  }

  .status-form {
    flex-direction: column;
    align-items: stretch;
  }

  .status-form select {
    min-width: 100%;
  }
}
</style>
