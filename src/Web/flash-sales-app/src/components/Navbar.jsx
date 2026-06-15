import { useAuth } from 'react-oidc-context'
import { Link, useNavigate, useLocation } from 'react-router-dom'
import { Zap, LogOut, Rocket, UserCircle2, Package, Radio } from 'lucide-react'
import { isActivated, isSeller } from '../lib/auth.js'
import Button from './Button.jsx'
import styles from './Navbar.module.css'

export default function Navbar() {
  const auth     = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const activated = isActivated(auth.user)
  const seller    = isSeller(auth.user)

  const handleLogout = () =>
    auth.signoutRedirect({ post_logout_redirect_uri: import.meta.env.VITE_POST_LOGOUT_URI })

  const isActive = (path) => location.pathname === path || location.pathname.startsWith(path + '/')

  return (
    <header className={styles.header}>
      <nav className={styles.nav}>
        <Link to={activated ? '/launches' : '/'} className={styles.brand}>
          <div className={styles.brandIcon}><Zap size={15} strokeWidth={2.5} /></div>
          <span className={styles.brandName}>Flash Sales</span>
        </Link>

        {activated && (
          <div className={styles.navLinks}>
            <Link to="/launches"  className={`${styles.navLink} ${isActive('/launches')  ? styles.navLinkActive : ''}`}>Launches</Link>
            <Link to="/products"  className={`${styles.navLink} ${isActive('/products') && !location.pathname.startsWith('/seller/products') ? styles.navLinkActive : ''}`}>Products</Link>
          </div>
        )}

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
              {seller ? (
                <>
                  <Button variant="ghost" size="sm" onClick={() => navigate('/seller/launches')}>
                    <Radio size={13} />
                    My launches
                  </Button>
                  <Button variant="ghost" size="sm" onClick={() => navigate('/seller/products')}>
                    <Package size={13} />
                    My products
                  </Button>
                  <Button variant="ghost" size="sm" onClick={() => navigate('/seller/profile')}>
                    <UserCircle2 size={13} />
                    My profile
                  </Button>
                </>
              ) : (
                <>
                  <Button variant="ghost" size="sm" onClick={() => navigate('/customer/profile')}>
                    <UserCircle2 size={13} />
                    My profile
                  </Button>
                  <Button variant="ghost" size="sm" onClick={() => navigate('/become-seller')}>
                    <Rocket size={13} />
                    Become a seller
                  </Button>
                </>
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
