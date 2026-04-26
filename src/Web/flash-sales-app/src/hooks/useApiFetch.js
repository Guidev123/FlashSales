import { useNavigate } from 'react-router-dom'

export function useApiFetch() {
  const navigate = useNavigate()

  return async function apiFetch(url, options = {}) {
    const res = await fetch(url, options)

    if (res.status === 403) {
      const body = await res.clone().json().catch(() => null)
      if (body?.type === 'account_not_activated') {
        const target = body.redirectTo === '/activate'
          ? '/activate/social'
          : (body.redirectTo ?? '/activate/social')
        navigate(target, { replace: true })
        return null
      }
    }

    return res
  }
}
