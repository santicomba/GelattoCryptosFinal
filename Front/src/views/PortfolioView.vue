<script setup>
import { ref, onMounted } from 'vue'
import { apiFetch } from '../auth'

const API = 'https://localhost:7097'
const tenencias = ref([])
const total = ref(0)
const cargado = ref(false)

onMounted(async () => {
  try {
    const res = await apiFetch('/transactions/portfolio')
    const datos = await res.json()
    tenencias.value = datos.tenencias || []
    total.value = datos.totalEnARS || 0
  } catch (e) {
    tenencias.value = []
    total.value = 0
  } finally {
    cargado.value = true
  }
})
</script>

<template>
  <div class="page">
    <h2>Mi Portfolio</h2>
    <div v-if="cargado && tenencias.length === 0" class="card">
      <p>No tenés criptomonedas en tu cartera.</p>
    </div>
    <div v-else-if="cargado">
      <div class="portfolio-card" v-for="t in tenencias" :key="t.cryptoCode">
        <div>
          <h3>{{ t.cryptoCode.toUpperCase() }}</h3>
          <p>Cantidad: {{ t.cantidad }}</p>
        </div>
        <div class="valor">${{ t.valorARS.toLocaleString('es-AR') }}</div>
      </div>
      <div class="portfolio-total">💰 Total en ARS: ${{ total.toLocaleString('es-AR') }}</div>
    </div>
  </div>
</template>