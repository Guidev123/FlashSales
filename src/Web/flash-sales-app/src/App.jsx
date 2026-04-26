import { BrowserRouter, Routes, Route, Navigate, useLocation } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'
import { isActivated, isSeller, needsActivation } from './lib/auth.js'

import LoadingScreen from './components/LoadingScreen.jsx'
import HomePage from './pages/HomePage.jsx'
import CallbackPage from './pages/CallbackPage.jsx'
import RegisterPage from './pages/RegisterPage.jsx'
import ActivateSocialPage from './pages/ActivateSocialPage.jsx'
import LaunchesPage from './pages/LaunchesPage.jsx'
import BecomeSellerPage from './pages/BecomeSellerPage.jsx'
import SellerProfilePage from './pages/SellerProfilePage.jsx'
import CustomerProfilePage from './pages/CustomerProfilePage.jsx'

function ActivationGuard({ children }) {
  const auth = useAuth()
  const location = useLocation()

  const exempt = location.pathname === '/activate/social' || location.pathname === '/callback'

  if (!auth.isLoading && auth.isAuthenticated && needsActivation(auth.user) && !exempt) {
    return <Navigate to="/activate/social" replace />
  }

  return children
}

function RequireActivated({ children }) {
  const auth = useAuth()
  if (auth.isLoading) return <LoadingScreen />
  if (!auth.isAuthenticated) return <Navigate to="/" replace />
  if (needsActivation(auth.user)) return <Navigate to="/activate/social" replace />
  if (!isActivated(auth.user)) return <Navigate to="/" replace />
  return children
}

function RequireSeller({ children }) {
  const auth = useAuth()
  if (auth.isLoading) return <LoadingScreen />
  if (!auth.isAuthenticated) return <Navigate to="/" replace />
  if (needsActivation(auth.user)) return <Navigate to="/activate/social" replace />
  if (!isSeller(auth.user)) return <Navigate to="/launches" replace />
  return children
}

function RequireCustomer({ children }) {
  const auth = useAuth()
  if (auth.isLoading) return <LoadingScreen />
  if (!auth.isAuthenticated) return <Navigate to="/" replace />
  if (needsActivation(auth.user)) return <Navigate to="/activate/social" replace />
  if (isSeller(auth.user)) return <Navigate to="/seller/profile" replace />
  return children
}

export default function App() {
  const auth = useAuth()

  if (auth.isLoading) return <LoadingScreen />

  return (
    <BrowserRouter>
      <ActivationGuard>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/callback" element={<CallbackPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/activate/social" element={<ActivateSocialPage />} />
          <Route
            path="/launches"
            element={
              <RequireActivated>
                <LaunchesPage />
              </RequireActivated>
            }
          />
          <Route
            path="/become-seller"
            element={
              <RequireActivated>
                <BecomeSellerPage />
              </RequireActivated>
            }
          />
          <Route
            path="/seller/profile"
            element={
              <RequireSeller>
                <SellerProfilePage />
              </RequireSeller>
            }
          />
          <Route
            path="/customer/profile"
            element={
              <RequireCustomer>
                <CustomerProfilePage />
              </RequireCustomer>
            }
          />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </ActivationGuard>
    </BrowserRouter>
  )
}
