<script setup>
import { ref, watch } from 'vue'
import { getSaldo, setSaldo } from '../auth.js'

const API = 'https://localhost:7097'

const accion = ref('purchase')
const crypto = ref('btc')
const cantidad = ref('')
const fecha = ref('')
const mensaje = ref('')
const tipoMensaje = ref('')
const saldo = ref(getSaldo())
const precioUnitario = ref(null)
const cargandoPrecio = ref(false)

function mostrarMensaje(texto, tipo) {
  mensaje.value = texto
  tipoMensaje.value = tipo
  setTimeout(() => { mensaje.value = '' }, 4000)
}

async function consultarPrecio() {
  cargandoPrecio.value = true
  precioUnitario.value = null
  try {
    const res = await fetch(`${API}/prices/${crypto.value}`)
    const data = await res.json()
    precioUnitario.value = data.price
  } catch (e) {
    precioUnitario.value = null
  }
  cargandoPrecio.value = false
}

// cada vez que cambia la cripto elegida, vuelvo a consultar el precio
watch(crypto, consultarPrecio, { immediate: true })

async function guardarTransaccion() {
  const cant = parseFloat(cantidad.value)
  if (!cant || cant <= 0) { mostrarMensaje('La cantidad debe ser mayor a 0', 'error'); return }
  if (!fecha.value) { mostrarMensaje('Ingresá la fecha y hora', 'error'); return }
  if (!precioUnitario.value) { mostrarMensaje('No se pudo obtener el precio, intentá de nuevo', 'error'); return }

  const total = precioUnitario.value * cant

  // valido saldo solo si es una compra
  if (accion.value === 'purchase' && total > saldo.value) {
    mostrarMensaje('❌ No tenés saldo suficiente para esta compra.', 'error')
    return
  }

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
      // actualizo el saldo local segun la operacion
      const nuevoSaldo = accion.value === 'purchase' ? saldo.value - total : saldo.value + total
      setSaldo(nuevoSaldo)
      saldo.value = nuevoSaldo

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
      <p class="portfolio-total" style="text-align:left; background:none; color:#1a6fc4; padding:0; margin-bottom:10px;">
        Saldo disponible: ${{ saldo.toLocaleString('es-AR') }}
      </p>

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

      <div v-if="cargandoPrecio" class="mensaje">Consultando precio...</div>
      <div v-else-if="precioUnitario && cantidad > 0" class="mensaje ok">
        Precio unitario: ${{ precioUnitario.toLocaleString('es-AR') }}<br>
        <strong>Total: ${{ (precioUnitario * cantidad).toLocaleString('es-AR') }}</strong>
      </div>

      <button class="btn" @click="guardarTransaccion">Guardar transacción</button>
      <div v-if="mensaje" class="mensaje" :class="tipoMensaje">{{ mensaje }}</div>
    </div>
  </div>
</template>