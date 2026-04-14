<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getMaintenanceActions, createMaintenanceAction } from '@/api/machineApi'
import type { MaintenanceAction, PagedResult } from '@/types'

const props = defineProps<{ machineId: string }>()

const result = ref<PagedResult<MaintenanceAction> | null>(null)
const loading = ref(false)
const error = ref('')
const page = ref(1)
const pageSize = 10

// New maintenance form
const description = ref('')
const performedBy = ref('')
const submitting = ref(false)
const formError = ref('')

async function loadActions() {
  loading.value = true
  error.value = ''
  try {
    result.value = await getMaintenanceActions(props.machineId, page.value, pageSize)
  } catch (e: any) {
    error.value = e.message || 'Fout bij ophalen van onderhoudsacties'
  } finally {
    loading.value = false
  }
}

async function handleSubmit() {
  if (!description.value.trim() || !performedBy.value.trim()) {
    formError.value = 'Vul alle velden in.'
    return
  }

  submitting.value = true
  formError.value = ''
  try {
    await createMaintenanceAction(props.machineId, {
      description: description.value.trim(),
      performedBy: performedBy.value.trim()
    })
    description.value = ''
    performedBy.value = ''
    page.value = 1
    await loadActions()
  } catch (e: any) {
    formError.value = e.message || 'Fout bij toevoegen van onderhoudsactie'
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  loadActions()
})
</script>

<template>
  <div>
    <h3>Onderhoudsacties</h3>

    <!-- Add new maintenance action -->
    <div class="maintenance-form">
      <div v-if="formError" class="error-message">{{ formError }}</div>
      <form @submit.prevent="handleSubmit">
        <div class="form-row">
          <div class="form-group" style="flex: 2;">
            <label for="description">Beschrijving</label>
            <input
              id="description"
              v-model="description"
              type="text"
              placeholder="bijv. Waterfilter vervangen"
            />
          </div>
          <div class="form-group" style="flex: 1;">
            <label for="performedBy">Uitgevoerd door</label>
            <input
              id="performedBy"
              v-model="performedBy"
              type="text"
              placeholder="bijv. Technicus Jan"
            />
          </div>
          <div class="form-group form-submit">
            <button type="submit" class="btn btn-primary" :disabled="submitting">
              {{ submitting ? '...' : 'Toevoegen' }}
            </button>
          </div>
        </div>
      </form>
    </div>

    <!-- Actions list -->
    <div v-if="error" class="error-message">{{ error }}</div>
    <div v-if="loading" class="loading">Laden...</div>

    <div v-if="result && !loading">
      <div v-if="result.data.length === 0" class="no-data">
        Geen onderhoudsacties gevonden.
      </div>

      <div v-for="action in result.data" :key="action.id" class="action-item">
        <div class="action-description">{{ action.description }}</div>
        <div class="action-meta">
          {{ action.performedBy }} &mdash;
          {{ new Date(action.performedAt).toLocaleString('nl-NL') }}
        </div>
      </div>

      <div v-if="result.totalPages > 1" class="pagination">
        <button class="btn btn-secondary" :disabled="page <= 1" @click="page--; loadActions()">
          Vorige
        </button>
        <span>{{ result.currentPage }} / {{ result.totalPages }}</span>
        <button
          class="btn btn-secondary"
          :disabled="page >= result.totalPages"
          @click="page++; loadActions()"
        >
          Volgende
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
h3 {
  margin-bottom: 1rem;
}

.maintenance-form {
  background: #f8f9fa;
  border-radius: 6px;
  padding: 1rem;
  margin-bottom: 1.5rem;
}

.form-row {
  display: flex;
  gap: 0.75rem;
  align-items: flex-end;
}

.form-submit {
  margin-bottom: 1rem;
}

.action-item {
  padding: 0.75rem 0;
  border-bottom: 1px solid #eee;
}

.action-item:last-child {
  border-bottom: none;
}

.action-description {
  font-weight: 500;
}

.action-meta {
  font-size: 0.85rem;
  color: #666;
  margin-top: 0.25rem;
}

.no-data {
  color: #666;
  font-style: italic;
  padding: 1rem 0;
}

@media (max-width: 600px) {
  .form-row {
    flex-direction: column;
  }
}
</style>
