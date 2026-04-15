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
const showForm = ref(false)

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
    showForm.value = false
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
    <div class="log-header">
      <h3>Onderhoudshistorie</h3>
      <button class="btn btn-primary btn-sm" @click="showForm = !showForm">
        {{ showForm ? 'Annuleren' : '+ Actie toevoegen' }}
      </button>
    </div>

    <!-- Add form (collapsible) -->
    <div v-if="showForm" class="add-form">
      <div v-if="formError" class="error-message">{{ formError }}</div>
      <form @submit.prevent="handleSubmit">
        <div class="form-row">
          <div class="form-group" style="flex: 2;">
            <label for="m-description">Beschrijving</label>
            <input
              id="m-description"
              v-model="description"
              type="text"
              placeholder="bijv. Waterfilter vervangen"
            />
          </div>
          <div class="form-group" style="flex: 1;">
            <label for="m-performedBy">Uitgevoerd door</label>
            <input
              id="m-performedBy"
              v-model="performedBy"
              type="text"
              placeholder="bijv. Technicus Jan"
            />
          </div>
        </div>
        <div class="form-submit-row">
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            {{ submitting ? 'Opslaan...' : 'Onderhoudsactie opslaan' }}
          </button>
        </div>
      </form>
    </div>

    <!-- Error & loading -->
    <div v-if="error" class="error-message">{{ error }}</div>
    <div v-if="loading" class="loading">Onderhoudsacties laden...</div>

    <!-- Actions list -->
    <div v-if="result && !loading">
      <div v-if="result.data.length === 0" class="empty-state-sm">
        Geen onderhoudsacties gevonden voor deze machine.
      </div>

      <div class="action-list">
        <div v-for="action in result.data" :key="action.id" class="action-item">
          <div class="action-dot"></div>
          <div class="action-content">
            <div class="action-description">{{ action.description }}</div>
            <div class="action-meta">
              <span class="action-person">{{ action.performedBy }}</span>
              <span class="action-separator">&middot;</span>
              <span>{{ new Date(action.performedAt).toLocaleString('nl-NL') }}</span>
            </div>
          </div>
        </div>
      </div>

      <div v-if="result.totalPages > 1" class="pagination">
        <button class="btn btn-secondary" :disabled="page <= 1" @click="page--; loadActions()">
          &larr; Vorige
        </button>
        <span>{{ result.currentPage }} / {{ result.totalPages }}</span>
        <button
          class="btn btn-secondary"
          :disabled="page >= result.totalPages"
          @click="page++; loadActions()"
        >
          Volgende &rarr;
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.log-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.25rem;
}

.btn-sm {
  padding: 0.4rem 0.875rem;
  font-size: 0.8rem;
}

.add-form {
  background: var(--color-bg);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: 1.25rem;
  margin-bottom: 1.5rem;
}

.form-row {
  display: flex;
  gap: 0.75rem;
}

.form-submit-row {
  display: flex;
  justify-content: flex-end;
  margin-top: 0.25rem;
}

.action-list {
  position: relative;
}

.action-item {
  display: flex;
  gap: 1rem;
  padding: 1rem 0;
  position: relative;
}

.action-item:not(:last-child) {
  border-bottom: 1px solid var(--color-border-light);
}

.action-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--color-primary);
  margin-top: 0.5rem;
  flex-shrink: 0;
}

.action-content {
  flex: 1;
}

.action-description {
  font-weight: 500;
  font-size: 0.9rem;
  margin-bottom: 0.2rem;
}

.action-meta {
  font-size: 0.8rem;
  color: var(--color-text-muted);
  display: flex;
  align-items: center;
  gap: 0.4rem;
}

.action-person {
  font-weight: 500;
  color: var(--color-text);
}

.action-separator {
  color: var(--color-text-light);
}

.empty-state-sm {
  text-align: center;
  padding: 2rem 1rem;
  color: var(--color-text-light);
  font-size: 0.9rem;
  font-style: italic;
}

@media (max-width: 640px) {
  .form-row {
    flex-direction: column;
  }
}
</style>
