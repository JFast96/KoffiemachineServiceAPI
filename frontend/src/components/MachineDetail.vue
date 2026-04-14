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
  try {
    machine.value = await updateMachineStatus(props.id, {
      status: newStatus.value,
      rowVersion: machine.value.rowVersion
    })
  } catch (e: any) {
    if (e instanceof ApiError && e.status === 409) {
      error.value = 'Conflict: deze machine is door een andere gebruiker gewijzigd. Pagina wordt herladen.'
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
    <button class="btn btn-secondary" @click="router.push('/')">Terug naar overzicht</button>

    <div v-if="error" class="error-message" style="margin-top: 1rem;">{{ error }}</div>
    <div v-if="loading" class="loading">Laden...</div>

    <div v-if="machine && !loading" class="detail-grid">
      <div class="card">
        <h2>{{ machine.name }}</h2>
        <div class="detail-fields">
          <div class="detail-field">
            <label>Locatie</label>
            <span>{{ machine.location }}</span>
          </div>
          <div class="detail-field">
            <label>Status</label>
            <span class="status-badge" :class="statusClass(machine.status)">
              {{ machine.status }}
            </span>
          </div>
          <div class="detail-field">
            <label>Aangemaakt</label>
            <span>{{ new Date(machine.createdAt).toLocaleString('nl-NL') }}</span>
          </div>
          <div class="detail-field" v-if="machine.updatedAt">
            <label>Laatst gewijzigd</label>
            <span>{{ new Date(machine.updatedAt).toLocaleString('nl-NL') }}</span>
          </div>
        </div>

        <div class="status-update">
          <h3>Status wijzigen</h3>
          <div class="status-form">
            <select v-model="newStatus">
              <option v-for="s in statuses" :key="s" :value="s">{{ s }}</option>
            </select>
            <button
              class="btn btn-primary"
              :disabled="updating || newStatus === machine.status"
              @click="handleUpdateStatus"
            >
              {{ updating ? 'Bezig...' : 'Opslaan' }}
            </button>
          </div>
        </div>
      </div>

      <div class="card">
        <MaintenanceLog :machine-id="id" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.detail-grid {
  display: grid;
  gap: 1.5rem;
  margin-top: 1.5rem;
}

.detail-fields {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1rem;
  margin: 1.5rem 0;
}

.detail-field label {
  font-size: 0.8rem;
  color: #666;
  text-transform: uppercase;
  margin-bottom: 0.25rem;
}

.detail-field span {
  font-size: 1rem;
}

.status-update {
  border-top: 1px solid #eee;
  padding-top: 1rem;
  margin-top: 1rem;
}

.status-update h3 {
  font-size: 1rem;
  margin-bottom: 0.75rem;
}

.status-form {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}

.status-form select {
  width: auto;
  min-width: 160px;
}
</style>
