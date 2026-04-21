import { useState, useEffect } from 'react'
import { useAuth } from 'react-oidc-context'

export function useSellerStatus() {
  const auth = useAuth()
  const [isSeller, setIsSeller] = useState(false)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!auth.isAuthenticated) {
      setLoading(false)
      return
    }

    async function check() {
      try {
        const res = await fetch(`${import.meta.env.VITE_API_URL}/api/v1/users/me`, {
          headers: { Authorization: `Bearer ${auth.user.access_token}` },
        })
        if (res.ok) {
          const data = await res.json()
          setIsSeller(data.roles?.includes('seller') ?? false)
        }
      } catch {
        setIsSeller(false)
      } finally {
        setLoading(false)
      }
    }

    check()
  }, [auth.isAuthenticated, auth.user?.access_token])

  return { isSeller, loading }
}
