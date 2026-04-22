import { useAuth } from 'react-oidc-context'
import { Link, useNavigate } from 'react-router-dom'
import { Zap, LogOut, Rocket } from 'lucide-react'
import { isActivated, isSeller } from '../lib/auth.js'
import Button from './Button.jsx'
import styles from './Navbar.module.css'

export default function Navbar() {
  const auth = useAuth()
  const navigate = useNavigate()
  const activated = isActivated(auth.user)
  const seller    = isSeller(auth.user)

  const handleLogout = () =>
    auth.signoutRedirect({ post_logout_redirect_uri: import.meta.env.VITE_POST_LOGOUT_URI })

  return (
    <header className={styles.header}>
      <nav className={styles.nav}>
        <Link to={activated ? '/launches' : '/'} className={styles.brand}>
          <div className={styles.brandIcon}><Zap size={15} strokeWidth={2.5} /></div>
          <span className={styles.brandName}>Flash Sales</span>
        </Link>

        <div className={styles.actions}>
          {!auth.isAuthenticated && (
            <>
              <Button variant="ghost" size="sm" onClick={() => navigate('/register')}>
                Register
              </Button>
              <Button variant="primary" size="sm" onClick={() => auth.signinRedirect()}>
                Sign in
              </Button>
            </>
          )}

          {auth.isAuthenticated && activated && (
            <>
              {!seller && (
                <Button variant="ghost" size="sm" onClick={() => navigate('/become-seller')}>
                  <Rocket size={13} />
                  Become a seller
                </Button>
              )}
              <Button variant="outline" size="sm" onClick={handleLogout}>
                <LogOut size={13} />
                Sign out
              </Button>
            </>
          )}

          {auth.isAuthenticated && !activated && (
            <Button variant="outline" size="sm" onClick={handleLogout}>
              <LogOut size={13} />
              Sign out
            </Button>
          )}
        </div>
      </nav>
    </header>
  )
}
