<script setup>
import { ref, watch } from 'vue'
import { getSaldo, setSaldo, apiFetch } from '../auth.js'


// misma lista de 6 criptos que usa MercadoView, para que todo el sitio sea consistente
const CRIPTOS = [
  { code: 'btc', nombre: 'Bitcoin', icono: '₿' },
  { code: 'eth', nombre: 'Ethereum', icono: 'Ξ' },
  { code: 'ada', nombre: 'Cardano', icono: '🔷' },
  { code: 'usdc', nombre: 'USDC', icono: '💵' },
  { code: 'sol', nombre: 'Solana', icono: '◎' },
  { code: 'bnb', nombre: 'BNB', icono: '🟡' },
]

const accion = ref('purchase')
const crypto = ref('btc')
const cantidad = ref('')
const fecha = ref('')
const mensaje = ref('')
const tipoMensaje = ref('')
const saldo = ref(0)
const cargandoSaldo = ref(true)
const precioUnitario = ref(null)
const cargandoPrecio = ref(false)

// portfolio del usuario, para el dropdown dinámico de venta
const portfolio = ref([])          // [{ cryptoCode, cantidad, valorARS }]
const cargandoPortfolio = ref(false)

async function cargarSaldo() {
  cargandoSaldo.value = true

  try {
    const res = await apiFetch('/balances')

    if (!res.ok) {
      saldo.value = 0
      return
    }

    const data = await res.json()
    saldo.value = Number(data.saldo || 0)
    setSaldo(saldo.value)
  } catch (e) {
    saldo.value = 0
  } finally {
    cargandoSaldo.value = false
  }
}

function mostrarMensaje(texto, tipo) {
  mensaje.value = texto
  tipoMensaje.value = tipo
  setTimeout(() => { mensaje.value = '' }, 4000)
}

async function cargarPortfolio() {
  cargandoPortfolio.value = true
  try {
    const res = await apiFetch('/transactions/portfolio')
    const data = await res.json()
    portfolio.value = data.tenencias || []
  } catch (e) {
    portfolio.value = []
  }
  cargandoPortfolio.value = false
}

async function consultarPrecio() {
  cargandoPrecio.value = true
  precioUnitario.value = null
  try {
    const res = await apiFetch(`/prices/${crypto.value}`)
    const data = await res.json()
    precioUnitario.value = data.price
  } catch (e) {
    precioUnitario.value = null
  }
  cargandoPrecio.value = false
}

// cuando cambia el tipo de operación a "venta", cargo el portfolio real
// y si la cripto seleccionada no está en la cartera, salto a la primera que sí tenga
watch(accion, async (nuevaAccion) => {
  if (nuevaAccion === 'sale') {
    await cargarPortfolio()
    const tengoLaSeleccionada = portfolio.value.some(t => t.cryptoCode === crypto.value)
    if (!tengoLaSeleccionada && portfolio.value.length > 0) {
      crypto.value = portfolio.value[0].cryptoCode
    }
  }
}, { immediate: true })

// cada vez que cambia la cripto elegida, vuelvo a consultar el precio
watch(crypto, consultarPrecio, { immediate: true })

cargarSaldo()

async function guardarTransaccion() {
  const cant = parseFloat(cantidad.value)
  if (!cant || cant <= 0) { mostrarMensaje('La cantidad debe ser mayor a 0', 'error'); return }
  if (!fecha.value) { mostrarMensaje('Ingresá la fecha y hora', 'error'); return }
  if (!precioUnitario.value) { mostrarMensaje('No se pudo obtener el precio, intentá de nuevo', 'error'); return }

  // valido en el frontend que no vendas más de lo que tenés
  if (accion.value === 'sale') {
    const tenencia = portfolio.value.find(t => t.cryptoCode === crypto.value)
    const disponible = tenencia ? tenencia.cantidad : 0
    if (cant > disponible) {
      mostrarMensaje(`❌ No tenés suficiente ${crypto.value.toUpperCase()}. Disponible: ${disponible}`, 'error')
      return
    }
  }

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
    const res = await apiFetch('/transactions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    })
        if (res.ok) {
      await cargarSaldo()

      mostrarMensaje('✅ ¡Transacción guardada con éxito!', 'ok')
      cantidad.value = ''
      fecha.value = ''

      // si vendí, refresco el portfolio para que el dropdown quede al día
      if (accion.value === 'sale') await cargarPortfolio()
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

      <!-- Compra: lista fija con las 6 criptos disponibles -->
      <select v-if="accion === 'purchase'" v-model="crypto">
        <option v-for="c in CRIPTOS" :key="c.code" :value="c.code">
          {{ c.icono }} {{ c.nombre }} ({{ c.code.toUpperCase() }})
        </option>
      </select>

      <!-- Venta: dropdown dinámico según lo que tenés en el portfolio -->
      <template v-else>
        <div v-if="cargandoPortfolio" class="mensaje">Cargando tu cartera...</div>
        <div v-else-if="portfolio.length === 0" class="mensaje error">
          No tenés criptomonedas para vender.
        </div>
        <select v-else v-model="crypto">
          <option v-for="t in portfolio" :key="t.cryptoCode" :value="t.cryptoCode">
            {{ t.cryptoCode.toUpperCase() }} — tenés {{ t.cantidad }}
          </option>
        </select>
      </template>

      <label>Cantidad</label>
      <input type="number" v-model="cantidad" step="0.00000001" placeholder="Ej: 0.00070">

      <label>Fecha y hora</label>
      <input type="datetime-local" v-model="fecha">

      <div v-if="cargandoPrecio" class="mensaje">Consultando precio...</div>
      <div v-else-if="precioUnitario && cantidad > 0" class="mensaje ok">
        Precio unitario: ${{ precioUnitario.toLocaleString('es-AR') }}<br>
        <strong>Total: ${{ (precioUnitario * cantidad).toLocaleString('es-AR') }}</strong>
      </div>

      <button class="btn" :disabled="accion === 'sale' && portfolio.length === 0" @click="guardarTransaccion">Guardar transacción</button>
      <div v-if="mensaje" class="mensaje" :class="tipoMensaje">{{ mensaje }}</div>
    </div>
  </div>
</template>