import { useEffect } from 'react'
import { useAuth } from 'react-oidc-context'
import { useNavigate } from 'react-router-dom'
import { isActivated, needsActivation } from '../lib/auth.js'
import LoadingScreen from '../components/LoadingScreen.jsx'

export default function CallbackPage() {
  const auth = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    if (auth.isLoading) return
    if (auth.error) { navigate('/', { replace: true }); return }

    if (auth.isAuthenticated) {
      if (needsActivation(auth.user)) {
        navigate('/activate/social', { replace: true })
      } else if (isActivated(auth.user)) {
        navigate('/launches', { replace: true })
      } else {
        navigate('/', { replace: true })
      }
    }
  }, [auth.isLoading, auth.isAuthenticated, auth.error, auth.user, navigate])

  return <LoadingScreen />
}
