<script setup lang="ts">
import { ref } from 'vue'
import { login } from '@/api/machineApi'

const emit = defineEmits<{ (e: 'loggedIn'): void }>()

const username = ref('')
const password = ref('')
const error = ref('')
const submitting = ref(false)

async function handleSubmit() {
  if (!username.value.trim() || !password.value.trim()) {
    error.value = 'Vul beide velden in.'
    return
  }

  submitting.value = true
  error.value = ''
  try {
    await login({ username: username.value.trim(), password: password.value.trim() })
    emit('loggedIn')
  } catch (e: any) {
    error.value = e.status === 401
      ? 'Ongeldige gebruikersnaam of wachtwoord.'
      : e.message || 'Inloggen mislukt.'
  } finally {
    submitting.value = false
  }
}


</script>

<template>
  <div class="login-wrapper">
    <div class="login-card card">
      <div class="login-header">
        <span class="login-icon">&#9749;</span>
        <h2>Koffiemachine Service</h2>
        <p class="login-subtitle">Log in om toegang te krijgen tot het platform.</p>
      </div>

      <div v-if="error" class="error-message">{{ error }}</div>

      <form @submit.prevent="handleSubmit">
        <div class="form-group">
          <label for="username">Gebruikersnaam</label>
          <input
            id="username"
            v-model="username"
            type="text"
            placeholder="Voer gebruikersnaam in"
            autofocus
          />
        </div>

        <div class="form-group">
          <label for="password">Wachtwoord</label>
          <input
            id="password"
            v-model="password"
            type="password"
            placeholder="Voer wachtwoord in"
          />
        </div>

        <button type="submit" class="btn btn-primary login-btn" :disabled="submitting">
          {{ submitting ? 'Bezig...' : 'Inloggen' }}
        </button>
      </form>


    </div>
  </div>
</template>

<style scoped>
.login-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 80vh;
}

.login-card {
  width: 100%;
  max-width: 400px;
  padding: 2.5rem;
}

.login-header {
  text-align: center;
  margin-bottom: 2rem;
}

.login-icon {
  font-size: 2.5rem;
  display: block;
  margin-bottom: 0.5rem;
}

.login-header h2 {
  margin-bottom: 0.25rem;
}

.login-subtitle {
  color: var(--color-text-muted);
  font-size: 0.875rem;
}

.login-btn {
  width: 100%;
  padding: 0.7rem;
  font-size: 0.95rem;
  margin-top: 0.5rem;
}


</style>
