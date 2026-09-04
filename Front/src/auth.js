export function getUsuario() {
  const data = localStorage.getItem('login')
  return data ? JSON.parse(data) : null
}

export function isLogueado() {
  return getUsuario() !== null
}

export function isAdmin() {
  const u = getUsuario()
  return u?.rol === 'admin'
}

export function logout() {
  localStorage.removeItem('login')
}

// cada usuario tiene su propia clave de saldo en localStorage
export function getClaveSaldo() {
  const u = getUsuario()
  if (u === null) return 'saldo_invitado'
  return 'saldo_' + u.usuario
}

export function getSaldo() {
  const guardado = localStorage.getItem(getClaveSaldo())
  return guardado ? parseFloat(guardado) : 0
}

export function setSaldo(nuevoSaldo) {
  localStorage.setItem(getClaveSaldo(), nuevoSaldo)
}