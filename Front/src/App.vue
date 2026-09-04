<script setup>
import { ref, watch } from 'vue'
import { RouterLink, RouterView, useRouter, useRoute } from 'vue-router'
import { isLogueado, logout } from './auth'

const router = useRouter()
const route = useRoute()
const logueado = ref(isLogueado())

watch(route, () => {
  logueado.value = isLogueado()
})

function salir() {
  logout()
  logueado.value = false
  router.push('/login')
}
</script>

<template>
  <nav v-if="logueado">
    <img src="/logo.png" alt="Gelatto Logo">
    <h1>GelattoCryptos</h1>
    <RouterLink to="/">Nueva Transacción</RouterLink>
    <RouterLink to="/historial">Historial</RouterLink>
    <RouterLink to="/portfolio">Mi Portfolio</RouterLink>
    <RouterLink to="/mercado">Mercado</RouterLink>
    <RouterLink to="/saldos">Cargar Saldo</RouterLink>
    <button class="salir" @click="salir">Salir</button>
  </nav>
  <RouterView />
</template>