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
    error.value = 'Vul alle velden in.'
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
    <button class="btn btn-secondary" @click="router.push('/')">Terug naar overzicht</button>

    <div class="card" style="margin-top: 1.5rem; max-width: 500px;">
      <h2>Nieuwe Machine Registreren</h2>

      <div v-if="error" class="error-message" style="margin-top: 1rem;">{{ error }}</div>

      <form @submit.prevent="handleSubmit" style="margin-top: 1rem;">
        <div class="form-group">
          <label for="name">Naam</label>
          <input id="name" v-model="name" type="text" placeholder="bijv. Koffiemachine A" />
        </div>

        <div class="form-group">
          <label for="location">Locatie</label>
          <input id="location" v-model="location" type="text" placeholder="bijv. Kantine" />
        </div>

        <div class="form-group">
          <label for="status">Status</label>
          <select id="status" v-model="status">
            <option v-for="s in statuses" :key="s" :value="s">{{ s }}</option>
          </select>
        </div>

        <button type="submit" class="btn btn-primary" :disabled="submitting">
          {{ submitting ? 'Bezig...' : 'Registreren' }}
        </button>
      </form>
    </div>
  </div>
</template>
