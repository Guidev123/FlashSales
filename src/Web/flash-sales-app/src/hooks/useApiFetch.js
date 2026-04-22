import { useNavigate } from 'react-router-dom'

/**
 * Wraps fetch with global handling for the 403 account_not_activated edge case.
 * When the API returns { type: "account_not_activated", redirectTo: "/activate" }
 * the user is automatically sent to /activate/social and the call returns null.
 *
 * All callers should guard: `if (!res) return`
 */
export function useApiFetch() {
  const navigate = useNavigate()

  return async function apiFetch(url, options = {}) {
    const res = await fetch(url, options)

    if (res.status === 403) {
      const body = await res.clone().json().catch(() => null)
      if (body?.type === 'account_not_activated') {
        // Backend sends /activate; our route is /activate/social
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
