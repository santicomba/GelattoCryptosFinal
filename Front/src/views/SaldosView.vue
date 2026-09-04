<script setup>
import { ref } from 'vue'
import { getSaldo, setSaldo, apiFetch } from '../auth.js'

const saldo = ref(0)
const metodo = ref('')
const monto = ref('')
const mensaje = ref('')
const tipoMensaje = ref('')

function mostrarMensaje(texto, tipo) {
  mensaje.value = texto
  tipoMensaje.value = tipo
  setTimeout(() => { mensaje.value = '' }, 4000)
}

async function cargarSaldo() {
  if (!metodo.value) {
    mostrarMensaje('Seleccioná un método de pago.', 'error')
    return
  }
  const valor = parseFloat(monto.value)
  if (!valor || valor <= 0) {
    mostrarMensaje('Ingresá un monto válido.', 'error')
    return
  }

  try {
    const res = await apiFetch('/balances/cargar', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        monto: valor
      })
    })

    if (!res.ok) {
      mostrarMensaje('❌ No se pudo cargar el saldo.', 'error')
      return
    }

    const data = await res.json()

    saldo.value = Number(data.saldo || 0)
    setSaldo(saldo.value)

    mostrarMensaje('✅ Saldo cargado con éxito.', 'ok')
    metodo.value = ''
    monto.value = ''
  } catch (e) {
    mostrarMensaje('❌ No se pudo conectar con el servidor.', 'error')
  }
}

async function traerSaldoInicial() {
  try {
    const res = await apiFetch('/balances')
    if (res.ok) {
      const data = await res.json()
      saldo.value = Number(data.saldo || 0)
      setSaldo(saldo.value)
    }
  } catch (e) {
    saldo.value = 0
  }
}

traerSaldoInicial()
</script>

<template>
  <div class="page">
    <h2>Cargar Saldo</h2>
    <div class="card">
      <p class="portfolio-total" style="text-align:left; background:none; color:#1a6fc4; padding:0; margin-bottom:20px;">
        Saldo actual: ${{ saldo.toLocaleString('es-AR') }}
      </p>

      <label>Método de pago</label>
      <select v-model="metodo">
        <option value="">Selecciona un método de pago</option>
        <option value="mercadopago">MercadoPago</option>
        <option value="transferencia">Transferencia bancaria</option>
        <option value="paypal">PayPal</option>
      </select>

      <label>Monto a cargar (ARS)</label>
      <input type="number" v-model="monto" placeholder="Monto en ARS">

      <button class="btn" @click="cargarSaldo">Confirmar</button>
      <div v-if="mensaje" class="mensaje" :class="tipoMensaje">{{ mensaje }}</div>
    </div>
  </div>
</template>