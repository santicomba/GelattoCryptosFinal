<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const usuario = ref('')
const password = ref('')
const error = ref('')
const router = useRouter()

const USUARIOS = [
  { usuario: 'santicomba', password: 'gelatto', rol: 'admin' },
  { usuario: 'user', password: 'pwd', rol: 'usuario' }
]

function login() {
  const encontrado = USUARIOS.find(u => u.usuario === usuario.value && u.password === password.value)
  if (encontrado) {
    localStorage.setItem('login', JSON.stringify(encontrado))
    router.push('/')
  } else {
    error.value = 'Usuario o contraseña incorrectos.'
  }
}
</script>

<template>
  <div class="login-wrap">
    <div class="card login-card">
      <h2>Ingresar</h2>
      <label>Usuario</label>
      <input v-model="usuario" type="text" placeholder="Usuario">
      <label>Contraseña</label>
      <input v-model="password" type="password" placeholder="Contraseña">
      <div v-if="error" class="mensaje error">{{ error }}</div>
      <button class="btn" @click="login">Ingresar</button>
    </div>
  </div>
</template>