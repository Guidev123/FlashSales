import { useState, useEffect } from 'react'
import { useAuth } from 'react-oidc-context'
import { useNavigate } from 'react-router-dom'
import { Zap, Calendar, User, Mail, ArrowRight, CheckCircle2, LogOut } from 'lucide-react'
import { isActivated, needsActivation } from '../lib/auth.js'
import Input from '../components/Input.jsx'
import Button from '../components/Button.jsx'
import styles from './AuthFormPage.module.css'

function validate(v) {
  const e = {}
  if (!v.birthDate) {
    e.birthDate = 'Required'
  } else {
    const age = (Date.now() - new Date(v.birthDate)) / (1000 * 60 * 60 * 24 * 365.25)
    if (age < 16) e.birthDate = 'You must be at least 16 years old'
  }
  return e
}

export default function ActivateSocialPage() {
  const auth = useAuth()
  const navigate = useNavigate()

  const [birthDate, setBirthDate] = useState('')
  const [error, setError] = useState('')
  const [apiError, setApiError] = useState('')
  const [loading, setLoading] = useState(false)
  const [done, setDone] = useState(false)

  useEffect(() => {
    if (auth.isLoading) return
    if (!auth.isAuthenticated) { navigate('/', { replace: true }); return }
    if (isActivated(auth.user)) { navigate('/launches', { replace: true }); return }
    if (!needsActivation(auth.user)) { navigate('/', { replace: true }); return }

    if (auth.user.profile.birth_date) {
      setBirthDate(auth.user.profile.birth_date)
    }
  }, [auth.isLoading, auth.isAuthenticated, auth.user, navigate])

  if (auth.isLoading || !auth.user) return null

  const profile = auth.user.profile

  const handleSubmit = async (e) => {
    e.preventDefault()
    const errs = validate({ birthDate })
    if (errs.birthDate) { setError(errs.birthDate); return }

    setLoading(true)
    setApiError('')
    try {
      const params = new URLSearchParams({ birthDate: new Date(birthDate).toISOString() })
      const res = await fetch(
        `${import.meta.env.VITE_API_URL}/api/v1/users/customer/activate?${params}`,
        {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${auth.user.access_token}`,
          },
        },
      )
      if (!res.ok) {
        const body = await res.json().catch(() => ({}))
        setApiError(body.message || 'Activation failed. Please try again.')
        return
      }
      setDone(true)
    } catch {
      setApiError('Could not reach the server. Check your connection.')
    } finally {
      setLoading(false)
    }
  }

  const handleLogout = () =>
    auth.signoutRedirect({ post_logout_redirect_uri: import.meta.env.VITE_POST_LOGOUT_URI })

  if (done) {
    return (
      <div className={styles.page}>
        <div className={styles.card}>
          <div className={styles.success}>
            <div className={styles.successIcon}><CheckCircle2 size={28} /></div>
            <h2 className={styles.title}>You're in!</h2>
            <p className={styles.sub}>
              Your customer account is activated. Now you can participate in future launches.
            </p>
            <Button variant="primary" size="md" onClick={() => auth.signinRedirect()}>
              Go to launches <ArrowRight size={14} />
            </Button>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className={styles.page}>
      <div className={styles.card}>
        <div className={styles.brandRow}>
          <div className={styles.brandIcon}><Zap size={15} strokeWidth={2.5} /></div>
          <span className={styles.brandName}>Flash Sales</span>
        </div>

        <div className={styles.header}>
          <h2 className={styles.title}>Almost there</h2>
          <p className={styles.sub}>
            Confirm your details to activate your customer account.
          </p>
        </div>

        <div className={styles.readonlyBox}>
          <div className={styles.readonlyField}>
            <User size={13} className={styles.roIcon} />
            <span className={styles.roLabel}>Name</span>
            <span className={styles.roValue}>{profile.given_name} {profile.family_name}</span>
          </div>
          <div className={styles.readonlyField}>
            <Mail size={13} className={styles.roIcon} />
            <span className={styles.roLabel}>Email</span>
            <span className={styles.roValue}>{profile.email}</span>
          </div>
        </div>

        <form onSubmit={handleSubmit} noValidate className={styles.form}>
          {apiError && <div className={styles.apiError}>{apiError}</div>}

          <Input
            label="Date of birth"
            type="date"
            icon={Calendar}
            value={birthDate}
            onChange={e => { setBirthDate(e.target.value); setError('') }}
            error={error}
            hint="Required to comply with platform age policy."
          />

          <Button type="submit" variant="primary" size="md" loading={loading}>
            Activate account <ArrowRight size={14} />
          </Button>
        </form>

        <button className={styles.logoutLink} onClick={handleLogout}>
          <LogOut size={13} /> Sign in with a different account
        </button>
      </div>
    </div>
  )
}
