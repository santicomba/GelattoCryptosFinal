<script setup>
import { ref } from 'vue'

const API = 'https://localhost:7097'
const codigo = ref('btc')
const precio = ref(null)
const cargando = ref(false)

async function consultarPrecio() {
  cargando.value = true
  precio.value = null
  try {
    const res = await fetch(`${API}/prices/${codigo.value}`)
    precio.value = await res.json()
  } catch (e) {
    precio.value = null
  }
  cargando.value = false
}
</script>

<template>
  <div class="page">
    <h2>Mercado</h2>
    <div class="card">
      <label>Criptomoneda</label>
      <select v-model="codigo">
        <option value="btc">₿ Bitcoin (BTC)</option>
        <option value="eth">Ξ Ethereum (ETH)</option>
        <option value="usdc">💵 USDC</option>
      </select>
      <button class="btn" @click="consultarPrecio">Consultar precio</button>

      <div v-if="cargando" class="mensaje">Consultando...</div>
      <div v-if="precio" class="portfolio-total" style="margin-top:20px;">
        {{ precio.cryptoCode.toUpperCase() }}: ${{ precio.price.toLocaleString('es-AR') }}
      </div>
    </div>
  </div>
</template>