<script setup>
import { ref } from 'vue'

const API = 'https://localhost:7097'

const accion = ref('purchase')
const crypto = ref('btc')
const cantidad = ref('')
const fecha = ref('')
const mensaje = ref('')
const tipoMensaje = ref('')

function mostrarMensaje(texto, tipo) {
  mensaje.value = texto
  tipoMensaje.value = tipo
  setTimeout(() => { mensaje.value = '' }, 4000)
}

async function guardarTransaccion() {
  const cant = parseFloat(cantidad.value)
  if (!cant || cant <= 0) { mostrarMensaje('La cantidad debe ser mayor a 0', 'error'); return }
  if (!fecha.value) { mostrarMensaje('Ingresá la fecha y hora', 'error'); return }

  const body = {
    cryptoCode: crypto.value,
    action: accion.value,
    cryptoAmount: cant,
    dateTime: new Date(fecha.value).toISOString()
  }

  try {
    const res = await fetch(`${API}/transactions`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    })
    if (res.ok) {
      mostrarMensaje('✅ ¡Transacción guardada con éxito!', 'ok')
      cantidad.value = ''
      fecha.value = ''
    } else {
      const err = await res.text()
      mostrarMensaje('❌ Error: ' + err, 'error')
    }
  } catch (e) {
    mostrarMensaje('❌ No se pudo conectar con el servidor', 'error')
  }
}
</script>

<template>
  <div class="page">
    <h2>Nueva Transacción</h2>
    <div class="card">
      <label>Tipo de operación</label>
      <select v-model="accion">
        <option value="purchase">🟢 Compra</option>
        <option value="sale">🔴 Venta</option>
      </select>

      <label>Criptomoneda</label>
      <select v-model="crypto">
        <option value="btc">₿ Bitcoin (BTC)</option>
        <option value="eth">Ξ Ethereum (ETH)</option>
        <option value="usdc">💵 USDC</option>
      </select>

      <label>Cantidad</label>
      <input type="number" v-model="cantidad" step="0.00000001" placeholder="Ej: 0.00070">

      <label>Fecha y hora</label>
      <input type="datetime-local" v-model="fecha">

      <button class="btn" @click="guardarTransaccion">Guardar transacción</button>
      <div v-if="mensaje" class="mensaje" :class="tipoMensaje">{{ mensaje }}</div>
    </div>
  </div>
</template>