import SaldosView from '../views/SaldosView.vue'
import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import InicioView from '../views/InicioView.vue'
import NuevaTransaccionView from '../views/NuevaTransaccionView.vue'
import HistorialView from '../views/HistorialView.vue'
import PortfolioView from '../views/PortfolioView.vue'
import MercadoView from '../views/MercadoView.vue'
import { isLogueado } from '../auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'login', component: LoginView },
    { path: '/', name: 'inicio', component: InicioView },
    { path: '/nueva-transaccion', name: 'transaccion', component: NuevaTransaccionView },
    { path: '/historial', name: 'historial', component: HistorialView },
    { path: '/portfolio', name: 'portfolio', component: PortfolioView },
    { path: '/mercado', name: 'mercado', component: MercadoView },
    { path: '/saldos', name: 'saldos', component: SaldosView },
  ]
})

router.beforeEach((to, from, next) => {
  const logueado = isLogueado()
  if (to.path !== '/login' && !logueado) {
    next('/login')
  } else if (to.path === '/login' && logueado) {
    next('/')
  } else {
    next()
  }
})

export default router