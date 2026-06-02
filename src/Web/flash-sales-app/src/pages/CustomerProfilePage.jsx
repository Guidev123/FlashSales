import { useState, useEffect } from 'react'
import { useAuth } from 'react-oidc-context'
import { Mail, User, Calendar, BadgeCheck, AlertCircle, Pencil, X, Check } from 'lucide-react'
import { useApiFetch } from '../hooks/useApiFetch.js'
import Navbar from '../components/Navbar.jsx'
import Input from '../components/Input.jsx'
import Button from '../components/Button.jsx'
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

function toDateInputValue(iso) {
  if (!iso) return ''
  return new Date(iso).toISOString().slice(0, 10)
}

function validateProfile(v) {
  const e = {}
  if (!v.name?.trim())           e.name      = 'Full name is required'
  else if (v.name.trim().split(/\s+/).length < 2)
                                 e.name      = 'Please enter first and last name'
  if (!v.birthDate)              e.birthDate = 'Date of birth is required'
  else {
    const birth = new Date(v.birthDate)
    const today = new Date()
    let age = today.getFullYear() - birth.getFullYear()
    const m = today.getMonth() - birth.getMonth()
    if (m < 0 || (m === 0 && today.getDate() < birth.getDate())) age--
    if (age < 16) e.birthDate = 'You must be at least 16 years old'
  }
  return e
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

  // edit state
  const [editing,   setEditing]   = useState(false)
  const [form,      setForm]      = useState({ name: '', birthDate: '' })
  const [errors,    setErrors]    = useState({})
  const [touched,   setTouched]   = useState({})
  const [saving,    setSaving]    = useState(false)
  const [saveError, setSaveError] = useState('')

  useEffect(() => {
    async function load() {
      setLoading(true)
      setFetchError('')
      try {
        const res = await apiFetch(`${import.meta.env.VITE_API_URL}/api/v1/users/me/profile`, {
          headers: { Authorization: `Bearer ${auth.user.access_token}` },
        })
        if (!res) return
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

  function openEdit() {
    setForm({
      name:      `${profile.firstName} ${profile.lastName}`.trim(),
      birthDate: toDateInputValue(profile.birthDate),
    })
    setErrors({})
    setTouched({})
    setSaveError('')
    setEditing(true)
  }

  function cancelEdit() {
    setEditing(false)
    setSaveError('')
  }

  const set = (field) => (e) => {
    const val = e.target.value
    setForm(f => ({ ...f, [field]: val }))
    if (touched[field]) {
      const errs = validateProfile({ ...form, [field]: val })
      setErrors(prev => ({ ...prev, [field]: errs[field] }))
    }
  }

  const blur = (field) => () => {
    setTouched(t => ({ ...t, [field]: true }))
    const errs = validateProfile(form)
    setErrors(prev => ({ ...prev, [field]: errs[field] }))
  }

  async function handleSave(e) {
    e.preventDefault()
    const errs = validateProfile(form)
    setErrors(errs)
    setTouched({ name: true, birthDate: true })
    if (Object.keys(errs).length) return

    setSaving(true)
    setSaveError('')
    try {
      const res = await apiFetch(`${import.meta.env.VITE_API_URL}/api/v1/users/profile`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${auth.user.access_token}`,
        },
        body: JSON.stringify({
          name:      form.name.trim(),
          birthDate: new Date(form.birthDate).toISOString(),
        }),
      })
      if (!res) return
      if (!res.ok) {
        const body = await res.json().catch(() => ({}))
        setSaveError(body.message || 'Could not save changes. Please try again.')
        return
      }

      // update local state optimistically
      const [firstName, ...rest] = form.name.trim().split(/\s+/)
      const lastName = rest.join(' ')
      setProfile(p => ({ ...p, firstName, lastName, birthDate: new Date(form.birthDate).toISOString() }))
      setEditing(false)
    } catch {
      setSaveError('Could not reach the server. Check your connection.')
    } finally {
      setSaving(false)
    }
  }

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
          <div className={styles.cardHeader}>
            <p className={styles.cardLabel}>Personal information</p>
            {!editing && (
              <button className={styles.editBtn} onClick={openEdit} aria-label="Edit profile">
                <Pencil size={13} /> Edit
              </button>
            )}
          </div>

          {editing ? (
            <form onSubmit={handleSave} noValidate className={styles.editForm}>
              {saveError && (
                <div className={styles.errorBox} style={{ marginBottom: 4 }}>
                  <AlertCircle size={15} /> {saveError}
                </div>
              )}

              <Input
                label="Full name"
                name="name"
                placeholder="First Last"
                icon={User}
                value={form.name}
                onChange={set('name')}
                onBlur={blur('name')}
                error={errors.name}
              />

              <Input
                label="Date of birth"
                name="birthDate"
                type="date"
                icon={Calendar}
                value={form.birthDate}
                onChange={set('birthDate')}
                onBlur={blur('birthDate')}
                error={errors.birthDate}
              />

              <div className={styles.editActions}>
                <Button type="submit" variant="primary" size="sm" loading={saving}>
                  <Check size={13} /> Save changes
                </Button>
                <Button type="button" variant="ghost" size="sm" onClick={cancelEdit} disabled={saving}>
                  <X size={13} /> Cancel
                </Button>
              </div>
            </form>
          ) : (
            <div className={styles.fields}>
              <Field label="Full name"     value={`${profile.firstName} ${profile.lastName}`} icon={User} />
              <Field label="Email"         value={profile.email}                              icon={Mail} />
              <Field label="Date of birth" value={formatDate(profile.birthDate)}              icon={Calendar} />
              <Field label="Age"           value={age !== null ? `${age} years old` : '—'}    icon={Calendar} />
            </div>
          )}
        </div>

      </div>
    </div>
  )
}
