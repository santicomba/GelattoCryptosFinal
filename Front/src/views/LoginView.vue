<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { login } from '../auth'

const usuario = ref('')
const password = ref('')
const error = ref('')
const router = useRouter()

async function iniciarSesion() {
  error.value = ''

  try {
    const res = await fetch('https://localhost:7097/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ usuario: usuario.value, password: password.value })
    })

    if (!res.ok) {
      error.value = 'Usuario o contraseña incorrectos.'
      return
    }

    const data = await res.json()
    login(data.usuario, data.rol, data.token, 0)
    router.push('/')
  } catch (e) {
    error.value = 'No se pudo conectar con el servidor.'
  }
}
</script>

<template>
  <div class="login-wrap">
    <div class="card login-card">
      <h2>Ingresar</h2>

      <label>Usuario</label>
      <input
        v-model="usuario"
        type="text"
        placeholder="Usuario"
      >

      <label>Contraseña</label>
      <input
        v-model="password"
        type="password"
        placeholder="Contraseña"
      >

      <div v-if="error" class="mensaje error">
        {{ error }}
      </div>

      <button class="btn" @click="iniciarSesion">
        Ingresar
      </button>
    </div>
  </div>
</template>