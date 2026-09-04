const API = 'https://localhost:7097'

export function getToken() {
  return localStorage.getItem('token')
}

export function getUsuario() {
  return localStorage.getItem('usuario')
}

export function getRol() {
  return localStorage.getItem('rol')
}

export function isAdmin() {
  return getRol() === 'admin'
}

export function estaLogueado() {
  return !!getToken()
}

export function getSaldo() {
  const saldo = localStorage.getItem('saldo')
  return saldo ? Number(saldo) : 0
}

export function setSaldo(saldo) {
  localStorage.setItem('saldo', saldo)
}

export async function apiFetch(endpoint, options = {}) {
  const token = getToken()

  const headers = {
    ...(options.headers || {})
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  return fetch(`${API}${endpoint}`, {
    ...options,
    headers
  })
}

export function login(usuario, rol, token, saldo = 0) {
  localStorage.setItem('usuario', usuario)
  localStorage.setItem('rol', rol)
  localStorage.setItem('token', token)
  localStorage.setItem('saldo', saldo)
}

export function logout() {
  localStorage.removeItem('usuario')
  localStorage.removeItem('rol')
  localStorage.removeItem('token')
  localStorage.removeItem('saldo')
}