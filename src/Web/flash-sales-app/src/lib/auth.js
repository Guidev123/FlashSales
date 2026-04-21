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

export function getRoles(user) {
  return user?.profile?.realm_access?.roles ?? []
}

export function isActivated(user) {
  return getRoles(user).includes('activated')
}

export function isSocialLogin(user) {
  const sub = user?.profile?.sub ?? ''
  return sub.includes(':')
}

export function needsActivation(user) {
  if (!user) return false
  return isSocialLogin(user) && !isActivated(user)
}
