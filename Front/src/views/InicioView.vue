<script setup>
import { getUsuario } from '../auth.js'

const usuario = getUsuario()

const opciones = [
  { titulo: 'Nueva Transacción', desc: 'Comprá o vendé criptomonedas', ruta: '/nueva-transaccion', icono: '💱' },
  { titulo: 'Historial', desc: 'Revisá tus movimientos', ruta: '/historial', icono: '📜' },
  { titulo: 'Mi Portfolio', desc: 'Mirá tu cartera actual', ruta: '/portfolio', icono: '📊' },
  { titulo: 'Mercado', desc: 'Precios de las criptos', ruta: '/mercado', icono: '📈' },
  { titulo: 'Cargar Saldo', desc: 'Sumá fondos a tu cuenta', ruta: '/saldos', icono: '💰' },
]
</script>

<template>
  <div class="page">
    <h2>Hola, {{ usuario?.usuario }} 👋</h2>
    <div class="menu-grid">
      <RouterLink v-for="op in opciones" :key="op.ruta" :to="op.ruta" class="menu-card">
        <div class="menu-icono">{{ op.icono }}</div>
        <h3>{{ op.titulo }}</h3>
        <p>{{ op.desc }}</p>
      </RouterLink>
    </div>
  </div>
</template>

<style scoped>
.menu-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 20px;
}
.menu-card {
  background: white;
  border-radius: 15px;
  padding: 30px 25px;
  text-decoration: none;
  box-shadow: 0 4px 15px rgba(0,100,255,0.08);
  transition: transform 0.15s, box-shadow 0.15s;
  border: 2px solid transparent;
}
.menu-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0,100,255,0.15);
  border-color: #4da6ff;
}
.menu-icono { font-size: 2.2em; margin-bottom: 12px; }
.menu-card h3 { color: #1a6fc4; font-size: 1.15em; margin-bottom: 6px; }
.menu-card p { color: #4a7fb5; font-size: 0.9em; }
</style>