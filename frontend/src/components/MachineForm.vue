<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { createMachine } from '@/api/machineApi'

const router = useRouter()

const name = ref('')
const location = ref('')
const status = ref('Actief')
const error = ref('')
const submitting = ref(false)

const statuses = ['Actief', 'Inactief', 'Defect', 'In onderhoud']

async function handleSubmit() {
  if (!name.value.trim() || !location.value.trim()) {
    error.value = 'Vul alle verplichte velden in.'
    return
  }

  submitting.value = true
  error.value = ''
  try {
    const machine = await createMachine({
      name: name.value.trim(),
      location: location.value.trim(),
      status: status.value
    })
    router.push(`/machines/${machine.id}`)
  } catch (e: any) {
    error.value = e.message || 'Fout bij aanmaken van machine'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div>
    <button class="btn btn-ghost" @click="router.push('/')">&larr; Terug naar overzicht</button>

    <div class="form-container">
      <div class="card form-card">
        <h2>Nieuwe Machine Registreren</h2>
        <p class="form-subtitle">Vul de gegevens in om een nieuwe koffiemachine te registreren.</p>

        <div v-if="error" class="error-message">{{ error }}</div>

        <form @submit.prevent="handleSubmit">
          <div class="form-group">
            <label for="name">Naam *</label>
            <input
              id="name"
              v-model="name"
              type="text"
              placeholder="bijv. Koffiemachine A"
              autofocus
            />
          </div>

          <div class="form-group">
            <label for="location">Locatie *</label>
            <input
              id="location"
              v-model="location"
              type="text"
              placeholder="bijv. Kantine 2e verdieping"
            />
          </div>

          <div class="form-group">
            <label for="status">Initiële status</label>
            <select id="status" v-model="status">
              <option v-for="s in statuses" :key="s" :value="s">{{ s }}</option>
            </select>
          </div>

          <div class="form-actions">
            <button type="button" class="btn btn-secondary" @click="router.push('/')">
              Annuleren
            </button>
            <button type="submit" class="btn btn-primary" :disabled="submitting">
              {{ submitting ? 'Bezig met registreren...' : 'Machine registreren' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.form-container {
  max-width: 520px;
  margin-top: 1.25rem;
}

.form-card {
  padding: 2rem;
}

.form-card h2 {
  margin-bottom: 0.25rem;
}

.form-subtitle {
  color: var(--color-text-muted);
  font-size: 0.875rem;
  margin-bottom: 1.75rem;
}

.form-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  margin-top: 0.5rem;
  padding-top: 1.25rem;
  border-top: 1px solid var(--color-border-light);
}

@media (max-width: 640px) {
  .form-actions {
    flex-direction: column-reverse;
  }

  .form-actions .btn {
    width: 100%;
  }
}
</style>
