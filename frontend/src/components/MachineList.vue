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

const statuses = ['Actief', 'Inactief', 'Defect', 'In onderhoud']

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

function resetFilter() {
  statusFilter.value = ''
  page.value = 1
}

watch(statusFilter, () => {
  page.value = 1
  loadMachines()
})

watch(page, () => {
  loadMachines()
})

onMounted(() => {
  loadMachines()
})
</script>

<template>
  <div>
    <!-- Page header -->
    <div class="page-header">
      <div>
        <h1>Koffiemachines</h1>
        <p class="page-subtitle" v-if="result">
          {{ result.totalRecords.toLocaleString('nl-NL') }} machines geregistreerd
        </p>
      </div>
      <RouterLink to="/machines/new" class="btn btn-primary">
        + Nieuwe Machine
      </RouterLink>
    </div>

    <!-- Filters -->
    <div class="filters card">
      <div class="filter-row">
        <div class="filter-group">
          <label>Filter op status</label>
          <select v-model="statusFilter">
            <option value="">Alle statussen</option>
            <option v-for="s in statuses" :key="s" :value="s">{{ s }}</option>
          </select>
        </div>
        <button v-if="statusFilter" class="btn btn-ghost" @click="resetFilter">
          Filter wissen
        </button>
      </div>
    </div>

    <!-- Error -->
    <div v-if="error" class="error-message">{{ error }}</div>

    <!-- Loading -->
    <div v-if="loading" class="loading">Machines laden...</div>

    <!-- Table -->
    <div v-if="result && !loading" class="table-wrapper card">
      <table class="machine-table">
        <thead>
          <tr>
            <th>Naam</th>
            <th>Locatie</th>
            <th>Status</th>
            <th>Aangemaakt</th>
            <th class="col-action"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="machine in result.data" :key="machine.id">
            <td class="col-name">
              <RouterLink :to="`/machines/${machine.id}`">{{ machine.name }}</RouterLink>
            </td>
            <td class="col-location">{{ machine.location }}</td>
            <td>
              <span class="status-badge" :class="statusClass(machine.status)">
                {{ machine.status }}
              </span>
            </td>
            <td class="col-date">{{ new Date(machine.createdAt).toLocaleDateString('nl-NL') }}</td>
            <td class="col-action">
              <RouterLink :to="`/machines/${machine.id}`" class="btn btn-ghost">
                Bekijken &rarr;
              </RouterLink>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Empty state -->
      <div v-if="result.data.length === 0" class="empty-state">
        <p>Geen machines gevonden</p>
        <p class="empty-hint">Pas de filters aan of registreer een nieuwe machine.</p>
      </div>

      <!-- Pagination -->
      <div v-if="result.totalPages > 1" class="pagination">
        <button class="btn btn-secondary" :disabled="page <= 1" @click="page--">
          &larr; Vorige
        </button>
        <span>Pagina {{ result.currentPage }} van {{ result.totalPages.toLocaleString('nl-NL') }}</span>
        <button class="btn btn-secondary" :disabled="page >= result.totalPages" @click="page++">
          Volgende &rarr;
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1.5rem;
}

.page-subtitle {
  color: var(--color-text-muted);
  font-size: 0.875rem;
  margin-top: 0.25rem;
}

.filters {
  padding: 1rem 1.5rem;
  margin-bottom: 1.25rem;
}

.filter-row {
  display: flex;
  gap: 1rem;
  align-items: flex-end;
}

.filter-group {
  flex: 0 0 220px;
}

.filter-group select {
  margin-top: 0.25rem;
}

.table-wrapper {
  padding: 0;
  overflow: hidden;
}

.machine-table {
  width: 100%;
  border-collapse: collapse;
}

.machine-table th {
  padding: 0.875rem 1.25rem;
  text-align: left;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
  background: var(--color-bg);
  border-bottom: 1px solid var(--color-border);
}

.machine-table td {
  padding: 0.875rem 1.25rem;
  font-size: 0.9rem;
  border-bottom: 1px solid var(--color-border-light);
}

.machine-table tbody tr {
  transition: background var(--transition);
}

.machine-table tbody tr:hover {
  background: var(--color-primary-light);
}

.machine-table tbody tr:last-child td {
  border-bottom: none;
}

.col-name a {
  color: var(--color-text);
  text-decoration: none;
  font-weight: 500;
}

.col-name a:hover {
  color: var(--color-primary);
}

.col-location {
  color: var(--color-text-muted);
}

.col-date {
  color: var(--color-text-muted);
  font-size: 0.85rem;
}

.col-action {
  text-align: right;
  width: 120px;
}

.empty-state {
  text-align: center;
  padding: 3rem 2rem;
}

.empty-state p {
  color: var(--color-text-muted);
  font-size: 0.95rem;
}

.empty-hint {
  font-size: 0.85rem !important;
  color: var(--color-text-light) !important;
  margin-top: 0.25rem;
}

.pagination {
  padding: 1rem 1.25rem;
  border-top: 1px solid var(--color-border-light);
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    gap: 1rem;
  }

  .filter-row {
    flex-direction: column;
  }

  .filter-group {
    flex: 1;
  }

  .col-date,
  .col-action {
    display: none;
  }
}
</style>
