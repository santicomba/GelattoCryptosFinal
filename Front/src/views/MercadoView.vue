<script setup>
import { ref, onMounted } from 'vue'

const API = 'https://localhost:7097'

const CRIPTOS = [
  { code: 'btc', nombre: 'Bitcoin', icono: '₿' },
  { code: 'eth', nombre: 'Ethereum', icono: 'Ξ' },
  { code: 'ada', nombre: 'Cardano', icono: '🔷' },
  { code: 'usdc', nombre: 'USDC', icono: '💵' },
  { code: 'sol', nombre: 'Solana', icono: '◎' },
  { code: 'bnb', nombre: 'BNB', icono: '🟡' },
]

const precios = ref([])
const cargando = ref(true)

async function consultarPrecio(cripto) {
  try {
    const res = await fetch(`${API}/prices/${cripto.code}`)
    if (!res.ok) throw new Error()
    const data = await res.json()
    return { ...cripto, price: data.price, error: false }
  } catch (e) {
    return { ...cripto, price: null, error: true }
  }
}

onMounted(async () => {
  cargando.value = true
  precios.value = await Promise.all(CRIPTOS.map(consultarPrecio))
  cargando.value = false
})
</script>

<template>
  <div class="page">
    <h2>Mercado</h2>
    <div v-if="cargando" class="card">
      <p>Consultando precios...</p>
    </div>
    <div v-else class="card" style="padding:0; overflow:hidden;">
      <table>
        <thead>
          <tr>
            <th>Criptomoneda</th>
            <th>Código</th>
            <th>Precio (ARS)</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in precios" :key="c.code">
            <td>{{ c.icono }} {{ c.nombre }}</td>
            <td><b>{{ c.code.toUpperCase() }}</b></td>
            <td v-if="c.error" style="color:#b71c1c;">No disponible</td>
            <td v-else>${{ c.price.toLocaleString('es-AR') }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>