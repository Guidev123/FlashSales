import { useState, useEffect } from 'react'
import { useAuth } from 'react-oidc-context'
import { Mail, User, Calendar, BadgeCheck, AlertCircle } from 'lucide-react'
import { useApiFetch } from '../hooks/useApiFetch.js'
import Navbar from '../components/Navbar.jsx'
import styles from './CustomerProfilePage.module.css'

function formatDate(iso) {
  if (!iso) return '—'
  return new Intl.DateTimeFormat('en-US', {
    year: 'numeric', month: 'long', day: 'numeric',
  }).format(new Date(iso))
}

function calcAge(iso) {
  if (!iso) return null
  const birth = new Date(iso)
  const today = new Date()
  let age = today.getFullYear() - birth.getFullYear()
  const m = today.getMonth() - birth.getMonth()
  if (m < 0 || (m === 0 && today.getDate() < birth.getDate())) age--
  return age
}

function Field({ label, value, icon: Icon }) {
  return (
    <div className={styles.fieldRow}>
      <span className={styles.fieldLabel}>
        {Icon && <Icon size={12} />}
        {label}
      </span>
      <span className={styles.fieldValue}>{value || '—'}</span>
    </div>
  )
}

export default function CustomerProfilePage() {
  const auth     = useAuth()
  const apiFetch = useApiFetch()

  const [profile,    setProfile]    = useState(null)
  const [loading,    setLoading]    = useState(true)
  const [fetchError, setFetchError] = useState('')

  useEffect(() => {
    async function load() {
      setLoading(true)
      setFetchError('')
      try {
        const res = await apiFetch(`${import.meta.env.VITE_API_URL}/api/v1/users/me/profile`, {
          headers: { Authorization: `Bearer ${auth.user.access_token}` },
        })
        if (!res) return // redirected by useApiFetch
        if (!res.ok) {
          setFetchError('Could not load your profile. Please try again later.')
          return
        }
        setProfile(await res.json())
      } catch {
        setFetchError('Could not reach the server. Check your connection.')
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [auth.user?.access_token])

  // ── Skeleton ─────────────────────────────────────────────
  if (loading) {
    return (
      <div className={styles.page}>
        <Navbar />
        <div className={styles.inner}>
          <div className={styles.skeletonHeader} />
          <div className={styles.skeletonCard} />
        </div>
      </div>
    )
  }

  // ── Fetch error ───────────────────────────────────────────
  if (fetchError) {
    return (
      <div className={styles.page}>
        <Navbar />
        <div className={styles.inner}>
          <div className={styles.errorBox}>
            <AlertCircle size={20} />
            {fetchError}
          </div>
        </div>
      </div>
    )
  }

  const initials = `${profile.firstName?.[0] ?? ''}${profile.lastName?.[0] ?? ''}`.toUpperCase()
  const age      = calcAge(profile.birthDate)

  return (
    <div className={styles.page}>
      <Navbar />

      <div className={styles.inner}>

        {/* ── Profile header ─────────────────────────────── */}
        <div className={styles.profileHeader}>
          <div className={styles.avatarInitials}>{initials}</div>

          <div className={styles.headerInfo}>
            <div className={styles.nameRow}>
              <h1 className={styles.name}>{profile.firstName} {profile.lastName}</h1>
              <span className={styles.badge}><BadgeCheck size={13} /> Customer</span>
            </div>
            <p className={styles.email}>{profile.email}</p>
          </div>
        </div>

        {/* ── Info card ──────────────────────────────────── */}
        <div className={styles.infoCard}>
          <p className={styles.cardLabel}>Personal information</p>
          <div className={styles.fields}>
            <Field label="Full name"   value={`${profile.firstName} ${profile.lastName}`} icon={User} />
            <Field label="Email"       value={profile.email}             icon={Mail} />
            <Field label="Date of birth" value={formatDate(profile.birthDate)} icon={Calendar} />
            <Field label="Age"         value={age !== null ? `${age} years old` : '—'} icon={Calendar} />
          </div>
        </div>

      </div>
    </div>
  )
}
