<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { RouterLink, RouterView } from 'vue-router'
import { getToken, logout } from '@/api/machineApi'
import LoginForm from '@/components/LoginForm.vue'

const isAuthenticated = ref(!!getToken())

function handleLoggedIn() {
  isAuthenticated.value = true
}

function handleLogout() {
  logout()
  isAuthenticated.value = false
}

function handleAuthExpired() {
  isAuthenticated.value = false
}

onMounted(() => {
  window.addEventListener('auth:expired', handleAuthExpired)
})

onUnmounted(() => {
  window.removeEventListener('auth:expired', handleAuthExpired)
})
</script>

<template>
  <div class="app">
    <!-- Login screen -->
    <template v-if="!isAuthenticated">
      <main>
        <LoginForm @logged-in="handleLoggedIn" />
      </main>
    </template>

    <!-- Authenticated app -->
    <template v-else>
      <header>
        <nav>
          <RouterLink to="/" class="nav-brand">
            <span class="nav-icon">&#9749;</span>
            Koffiemachine Service
          </RouterLink>
          <div class="nav-links">
            <RouterLink to="/" class="nav-link">
              <span class="nav-link-icon">&#9776;</span>
              Overzicht
            </RouterLink>
            <a href="#" class="nav-link" @click.prevent="handleLogout">
              Uitloggen
            </a>
          </div>
        </nav>
      </header>

      <main>
        <RouterView />
      </main>

      <footer>
        <p>Koffiemachine Service API &mdash; Interview Opdracht</p>
      </footer>
    </template>
  </div>
</template>

<style>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap');

*,
*::before,
*::after {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

:root {
  --color-bg: #f0f2f5;
  --color-surface: #ffffff;
  --color-primary: #2563eb;
  --color-primary-hover: #1d4ed8;
  --color-primary-light: #eff6ff;
  --color-text: #1e293b;
  --color-text-muted: #64748b;
  --color-text-light: #94a3b8;
  --color-border: #e2e8f0;
  --color-border-light: #f1f5f9;
  --color-danger: #ef4444;
  --color-danger-hover: #dc2626;
  --color-success: #10b981;
  --color-warning: #f59e0b;
  --color-info: #3b82f6;
  --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.05);
  --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.07), 0 2px 4px -2px rgba(0, 0, 0, 0.05);
  --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.08), 0 4px 6px -4px rgba(0, 0, 0, 0.04);
  --radius-sm: 6px;
  --radius-md: 10px;
  --radius-lg: 14px;
  --radius-full: 9999px;
  --transition: 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

body {
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  background-color: var(--color-bg);
  color: var(--color-text);
  line-height: 1.6;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

.app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

/* --- Header / Nav --- */
header {
  background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
  color: white;
  padding: 0 2rem;
  position: sticky;
  top: 0;
  z-index: 100;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
}

nav {
  max-width: 1280px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 64px;
}

.nav-brand {
  font-size: 1.15rem;
  font-weight: 600;
  color: white;
  text-decoration: none;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  letter-spacing: -0.02em;
}

.nav-icon {
  font-size: 1.4rem;
}

.nav-links {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.nav-link {
  color: rgba(255, 255, 255, 0.7);
  text-decoration: none;
  font-size: 0.875rem;
  font-weight: 500;
  padding: 0.5rem 0.875rem;
  border-radius: var(--radius-sm);
  transition: all var(--transition);
  display: flex;
  align-items: center;
  gap: 0.4rem;
}

.nav-link:hover {
  color: white;
  background: rgba(255, 255, 255, 0.1);
}

.nav-link.router-link-exact-active {
  color: white;
  background: rgba(255, 255, 255, 0.12);
}

.nav-link-icon {
  font-size: 0.9rem;
}

.nav-link-cta {
  background: var(--color-primary);
  color: white !important;
}

.nav-link-cta:hover {
  background: var(--color-primary-hover) !important;
}

/* --- Main --- */
main {
  max-width: 1280px;
  width: 100%;
  margin: 0 auto;
  padding: 2rem;
  flex: 1;
}

/* --- Footer --- */
footer {
  text-align: center;
  padding: 1.5rem 2rem;
  color: var(--color-text-light);
  font-size: 0.8rem;
  border-top: 1px solid var(--color-border);
  background: var(--color-surface);
}

/* --- Cards --- */
.card {
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  border: 1px solid var(--color-border);
  padding: 1.75rem;
  margin-bottom: 1.25rem;
  transition: box-shadow var(--transition);
}

.card:hover {
  box-shadow: var(--shadow-md);
}

/* --- Buttons --- */
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.4rem;
  padding: 0.55rem 1.1rem;
  border: none;
  border-radius: var(--radius-sm);
  font-family: inherit;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  text-decoration: none;
  transition: all var(--transition);
  white-space: nowrap;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background: var(--color-primary);
  color: white;
  box-shadow: 0 1px 2px rgba(37, 99, 235, 0.3);
}

.btn-primary:hover:not(:disabled) {
  background: var(--color-primary-hover);
  box-shadow: 0 2px 4px rgba(37, 99, 235, 0.4);
  transform: translateY(-1px);
}

.btn-danger {
  background: var(--color-danger);
  color: white;
}

.btn-danger:hover:not(:disabled) {
  background: var(--color-danger-hover);
}

.btn-secondary {
  background: var(--color-surface);
  color: var(--color-text);
  border: 1px solid var(--color-border);
}

.btn-secondary:hover:not(:disabled) {
  background: var(--color-bg);
  border-color: var(--color-text-light);
}

.btn-ghost {
  background: transparent;
  color: var(--color-text-muted);
  padding: 0.4rem 0.75rem;
}

.btn-ghost:hover:not(:disabled) {
  background: var(--color-bg);
  color: var(--color-text);
}

/* --- Form elements --- */
input,
select,
textarea {
  width: 100%;
  padding: 0.6rem 0.875rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  font-family: inherit;
  font-size: 0.875rem;
  color: var(--color-text);
  background: var(--color-surface);
  transition: all var(--transition);
}

input:focus,
select:focus,
textarea:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

input::placeholder {
  color: var(--color-text-light);
}

label {
  display: block;
  margin-bottom: 0.35rem;
  font-weight: 500;
  font-size: 0.8rem;
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.form-group {
  margin-bottom: 1.25rem;
}

/* --- Status badges --- */
.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-full);
  font-size: 0.75rem;
  font-weight: 600;
  letter-spacing: 0.02em;
}

.status-badge::before {
  content: '';
  width: 6px;
  height: 6px;
  border-radius: 50%;
  display: inline-block;
}

.status-actief {
  background: #ecfdf5;
  color: #059669;
}
.status-actief::before {
  background: #10b981;
}

.status-inactief {
  background: #fefce8;
  color: #ca8a04;
}
.status-inactief::before {
  background: #f59e0b;
}

.status-defect {
  background: #fef2f2;
  color: #dc2626;
}
.status-defect::before {
  background: #ef4444;
}

.status-onderhoud {
  background: #eff6ff;
  color: #2563eb;
}
.status-onderhoud::before {
  background: #3b82f6;
}

/* --- Pagination --- */
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  margin-top: 1.75rem;
  padding-top: 1.25rem;
  border-top: 1px solid var(--color-border-light);
}

.pagination span {
  font-size: 0.85rem;
  color: var(--color-text-muted);
  min-width: 120px;
  text-align: center;
}

/* --- Error / Loading --- */
.error-message {
  background: #fef2f2;
  color: #dc2626;
  padding: 0.875rem 1.125rem;
  border-radius: var(--radius-sm);
  margin-bottom: 1rem;
  font-size: 0.875rem;
  border: 1px solid #fecaca;
}

.loading {
  text-align: center;
  padding: 3rem 2rem;
  color: var(--color-text-light);
  font-size: 0.9rem;
}

.loading::before {
  content: '';
  display: block;
  width: 32px;
  height: 32px;
  margin: 0 auto 1rem;
  border: 3px solid var(--color-border);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* --- Page titles --- */
h1 {
  font-size: 1.5rem;
  font-weight: 700;
  letter-spacing: -0.03em;
  color: var(--color-text);
}

h2 {
  font-size: 1.25rem;
  font-weight: 600;
  letter-spacing: -0.02em;
  color: var(--color-text);
}

h3 {
  font-size: 1rem;
  font-weight: 600;
  color: var(--color-text);
}

/* --- Responsive --- */
@media (max-width: 640px) {
  nav {
    flex-direction: column;
    height: auto;
    padding: 0.75rem 0;
    gap: 0.5rem;
  }

  main {
    padding: 1.25rem;
  }
}
</style>
