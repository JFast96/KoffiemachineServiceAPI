<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { getMachines } from '@/api/machineApi'
import type { Machine, PagedResult } from '@/types'

const result = ref<PagedResult<Machine> | null>(null)
const loading = ref(false)
const error = ref('')
const page = ref(1)
const pageSize = 20
const statusFilter = ref('')

const statuses = ['', 'Actief', 'Inactief', 'Defect', 'In onderhoud']

async function loadMachines() {
  loading.value = true
  error.value = ''
  try {
    result.value = await getMachines(page.value, pageSize, statusFilter.value || undefined)
  } catch (e: any) {
    error.value = e.message || 'Fout bij ophalen van machines'
  } finally {
    loading.value = false
  }
}

function statusClass(status: string): string {
  const s = status.toLowerCase().replace(' ', '')
  if (s === 'actief') return 'status-actief'
  if (s === 'inactief') return 'status-inactief'
  if (s === 'defect') return 'status-defect'
  if (s === 'inonderhoud') return 'status-onderhoud'
  return ''
}

watch([page, statusFilter], () => {
  loadMachines()
})

onMounted(() => {
  loadMachines()
})
</script>

<template>
  <div>
    <div class="list-header">
      <h1>Koffiemachines</h1>
      <div class="list-controls">
        <select v-model="statusFilter">
          <option value="">Alle statussen</option>
          <option v-for="s in statuses.slice(1)" :key="s" :value="s">{{ s }}</option>
        </select>
        <RouterLink to="/machines/new" class="btn btn-primary">+ Nieuwe Machine</RouterLink>
      </div>
    </div>

    <div v-if="error" class="error-message">{{ error }}</div>
    <div v-if="loading" class="loading">Laden...</div>

    <div v-if="result && !loading">
      <p class="record-count">{{ result.totalRecords }} machines gevonden</p>

      <table class="machine-table">
        <thead>
          <tr>
            <th>Naam</th>
            <th>Locatie</th>
            <th>Status</th>
            <th>Aangemaakt</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="machine in result.data" :key="machine.id">
            <td>{{ machine.name }}</td>
            <td>{{ machine.location }}</td>
            <td>
              <span class="status-badge" :class="statusClass(machine.status)">
                {{ machine.status }}
              </span>
            </td>
            <td>{{ new Date(machine.createdAt).toLocaleDateString('nl-NL') }}</td>
            <td>
              <RouterLink :to="`/machines/${machine.id}`" class="btn btn-secondary">
                Details
              </RouterLink>
            </td>
          </tr>
        </tbody>
      </table>

      <div class="pagination">
        <button class="btn btn-secondary" :disabled="page <= 1" @click="page--">Vorige</button>
        <span>Pagina {{ result.currentPage }} van {{ result.totalPages }}</span>
        <button
          class="btn btn-secondary"
          :disabled="page >= result.totalPages"
          @click="page++"
        >
          Volgende
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.list-controls {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.list-controls select {
  width: auto;
  min-width: 160px;
}

.record-count {
  color: #666;
  font-size: 0.9rem;
  margin-bottom: 0.75rem;
}

.machine-table {
  width: 100%;
  border-collapse: collapse;
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.machine-table th,
.machine-table td {
  padding: 0.75rem 1rem;
  text-align: left;
}

.machine-table th {
  background: #f8f9fa;
  font-weight: 600;
  font-size: 0.85rem;
  text-transform: uppercase;
  color: #666;
}

.machine-table tr:not(:last-child) td {
  border-bottom: 1px solid #eee;
}

.machine-table tr:hover td {
  background: #f8f9fa;
}
</style>
