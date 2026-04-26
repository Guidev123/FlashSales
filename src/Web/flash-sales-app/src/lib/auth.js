export const oidcConfig = {
  authority: import.meta.env.VITE_KEYCLOAK_URL,
  client_id: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
  redirect_uri: import.meta.env.VITE_REDIRECT_URI,
  post_logout_redirect_uri: import.meta.env.VITE_POST_LOGOUT_URI,
  response_type: 'code',
  scope: 'openid profile email',
  code_challenge_method: 'S256',
  automaticSilentRenew: true,
  loadUserInfo: true,
}

function parseJwt(token) {
  try {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
    return JSON.parse(atob(base64))
  } catch {
    return {}
  }
}

export function getRoles(user) {
  const payload = parseJwt(user?.access_token ?? '')
  return payload?.realm_access?.roles ?? []
}

export function isActivated(user) {
  return getRoles(user).includes('activated')
}

export function isSeller(user) {
  return getRoles(user).includes('seller')
}

export function needsActivation(user) {
  if (!user) return false
  return !isActivated(user)
}
