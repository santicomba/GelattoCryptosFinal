<script setup>
import { ref, onMounted } from 'vue'

const API = 'https://localhost:7097'
const historial = ref([])

const modalVer = ref(false)
const modalEditar = ref(false)
const modalBorrar = ref(false)

const detalle = ref(null)
const edit = ref({ id: null, cryptoCode: '', action: '', cryptoAmount: 0, money: 0, dateTime: '' })
const borrarId = ref(null)

onMounted(cargarHistorial)

async function cargarHistorial() {
  const res = await fetch(`${API}/transactions`)
  historial.value = await res.json()
}

function formatearFecha(f) {
  return new Date(f).toLocaleString('es-AR')
}

async function verTransaccion(id) {
  const res = await fetch(`${API}/transactions/${id}`)
  detalle.value = await res.json()
  modalVer.value = true
}

async function abrirEdicion(id) {
  const res = await fetch(`${API}/transactions/${id}`)
  const t = await res.json()
  edit.value = { ...t, dateTime: t.dateTime.slice(0, 16) }
  modalEditar.value = true
}

async function guardarEdicion() {
  const body = {
    cryptoCode: edit.value.cryptoCode,
    action: edit.value.action,
    cryptoAmount: parseFloat(edit.value.cryptoAmount),
    money: parseFloat(edit.value.money),
    dateTime: new Date(edit.value.dateTime).toISOString()
  }
  const res = await fetch(`${API}/transactions/${edit.value.id}`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body)
  })
  if (res.ok) { modalEditar.value = false; cargarHistorial() }
}

function abrirBorrado(id) {
  borrarId.value = id
  modalBorrar.value = true
}

async function confirmarBorrado() {
  await fetch(`${API}/transactions/${borrarId.value}`, { method: 'DELETE' })
  modalBorrar.value = false
  cargarHistorial()
}
</script>

<template>
  <div class="page">
    <h2>Historial de Movimientos</h2>
    <div class="card" style="padding:0; overflow:hidden;">
      <table>
        <thead>
          <tr>
            <th>ID</th><th>Tipo</th><th>Cripto</th><th>Cantidad</th><th>Monto ARS</th><th>Fecha</th><th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="t in historial" :key="t.id">
            <td>{{ t.id }}</td>
            <td>{{ t.action === 'purchase' ? '🟢 Compra' : '🔴 Venta' }}</td>
            <td><b>{{ t.cryptoCode.toUpperCase() }}</b></td>
            <td>{{ t.cryptoAmount }}</td>
            <td>${{ t.money.toLocaleString('es-AR') }}</td>
            <td>{{ formatearFecha(t.dateTime) }}</td>
            <td>
              <button class="btn-small btn-ver" @click="verTransaccion(t.id)">Ver</button>
              <button class="btn-small btn-editar" @click="abrirEdicion(t.id)">Editar</button>
              <button class="btn-small btn-borrar" @click="abrirBorrado(t.id)">Borrar</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>

  <div class="modal" :class="{active: modalVer}">
    <div class="modal-box" v-if="detalle">
      <h3>📋 Detalle de Transacción</h3>
      <div class="detalle-item"><b>ID:</b> {{ detalle.id }}</div>
      <div class="detalle-item"><b>Tipo:</b> {{ detalle.action === 'purchase' ? '🟢 Compra' : '🔴 Venta' }}</div>
      <div class="detalle-item"><b>Cripto:</b> {{ detalle.cryptoCode.toUpperCase() }}</div>
      <div class="detalle-item"><b>Cantidad:</b> {{ detalle.cryptoAmount }}</div>
      <div class="detalle-item"><b>Monto ARS:</b> ${{ detalle.money.toLocaleString('es-AR') }}</div>
      <div class="detalle-item"><b>Fecha:</b> {{ formatearFecha(detalle.dateTime) }}</div>
      <div class="modal-btns">
        <button class="btn" @click="modalVer = false">Cerrar</button>
      </div>
    </div>
  </div>

  <div class="modal" :class="{active: modalEditar}">
    <div class="modal-box">
      <h3>✏️ Editar Transacción</h3>
      <label>Criptomoneda</label>
      <select v-model="edit.cryptoCode">
        <option value="btc">₿ Bitcoin (BTC)</option>
        <option value="eth">Ξ Ethereum (ETH)</option>
        <option value="usdc">💵 USDC</option>
      </select>
      <label>Tipo</label>
      <select v-model="edit.action">
        <option value="purchase">🟢 Compra</option>
        <option value="sale">🔴 Venta</option>
      </select>
      <label>Cantidad</label>
      <input type="number" v-model="edit.cryptoAmount" step="0.00000001">
      <label>Monto ARS</label>
      <input type="number" v-model="edit.money">
      <label>Fecha</label>
      <input type="datetime-local" v-model="edit.dateTime">
      <div class="modal-btns">
        <button class="btn" @click="guardarEdicion">Guardar</button>
        <button class="btn" style="background:#e3f2fd; color:#1a6fc4;" @click="modalEditar = false">Cancelar</button>
      </div>
    </div>
  </div>

  <div class="modal" :class="{active: modalBorrar}">
    <div class="modal-box">
      <h3>🗑️ ¿Confirmar borrado?</h3>
      <p style="color:#4a7fb5;">Esta acción no se puede deshacer.</p>
      <div class="modal-btns">
        <button class="btn" style="background:linear-gradient(135deg,#ef5350,#b71c1c);" @click="confirmarBorrado">Sí, borrar</button>
        <button class="btn" style="background:#e3f2fd; color:#1a6fc4;" @click="modalBorrar = false">Cancelar</button>
      </div>
    </div>
  </div>
</template>