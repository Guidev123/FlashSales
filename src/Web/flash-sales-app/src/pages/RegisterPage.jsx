import { useState } from 'react'
import { useAuth } from 'react-oidc-context'
import { useNavigate, Link } from 'react-router-dom'
import { Mail, Lock, User, Calendar, Zap, ArrowRight, CheckCircle2 } from 'lucide-react'
import Input from '../components/Input.jsx'
import Button from '../components/Button.jsx'
import styles from './AuthFormPage.module.css'

function validate(v) {
  const e = {}
  if (!v.firstName?.trim())        e.firstName = 'Required'
  if (!v.lastName?.trim())         e.lastName  = 'Required'
  if (!v.email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v.email))
                                   e.email     = 'Enter a valid email'
  if (!v.birthDate)                e.birthDate = 'Required'
  else {
    const age = (Date.now() - new Date(v.birthDate)) / (1000 * 60 * 60 * 24 * 365.25)
    if (age < 16)                  e.birthDate = 'You must be at least 16 years old'
  }
  if (!v.password || v.password.length < 8) e.password = 'Min. 8 characters'
  if (v.confirmPassword !== v.password)     e.confirmPassword = 'Passwords do not match'
  return e
}

function strength(pw) {
  if (!pw) return 0
  let s = 0
  if (pw.length >= 8)          s++
  if (pw.length >= 12)         s++
  if (/[A-Z]/.test(pw))        s++
  if (/[0-9]/.test(pw))        s++
  if (/[^A-Za-z0-9]/.test(pw)) s++
  return Math.min(s, 4)
}
const STRENGTH_LABEL = ['', 'Weak', 'Fair', 'Good', 'Strong']
const STRENGTH_COLOR = ['', '#ef4444', '#f97316', '#eab308', '#22c55e']

export default function RegisterPage() {
  const auth     = useAuth()
  const navigate = useNavigate()

  const [values, setValues] = useState({
    firstName: '', lastName: '', email: '', birthDate: '', password: '', confirmPassword: '',
  })
  const [errors,  setErrors]  = useState({})
  const [touched, setTouched] = useState({})
  const [loading, setLoading] = useState(false)
  const [done,    setDone]    = useState(false)
  const [apiError, setApiError] = useState('')

  const set = (field) => (e) => {
    const val = e.target.value
    setValues(v => ({ ...v, [field]: val }))
    if (touched[field]) {
      const errs = validate({ ...values, [field]: val })
      setErrors(prev => ({ ...prev, [field]: errs[field] }))
    }
  }

  const blur = (field) => () => {
    setTouched(v => ({ ...v, [field]: true }))
    const errs = validate(values)
    setErrors(prev => ({ ...prev, [field]: errs[field] }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    const errs = validate(values)
    setErrors(errs)
    setTouched(Object.keys(values).reduce((a, k) => ({ ...a, [k]: true }), {}))
    if (Object.keys(errs).length) return

    setLoading(true)
    setApiError('')
    try {
      const res = await fetch(`${import.meta.env.VITE_API_URL}/api/v1/users`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name:            `${values.firstName.trim()} ${values.lastName.trim()}`,
          email:           values.email,
          password:        values.password,
          confirmPassword: values.confirmPassword,
          birthDate:       `${values.birthDate}T00:00:00.000Z`,
        }),
      })
      if (!res.ok) {
        const body = await res.json().catch(() => ({}))
        setApiError(body.message || 'Something went wrong. Please try again.')
        return
      }
      setDone(true)
    } catch {
      setApiError('Could not reach the server. Check your connection.')
    } finally {
      setLoading(false)
    }
  }

  if (done) {
    return (
      <div className={styles.page}>
        <div className={styles.card}>
          <div className={styles.success}>
            <div className={styles.successIcon}><CheckCircle2 size={28} /></div>
            <h2 className={styles.title}>Account created!</h2>
            <p className={styles.sub}>
              Sign in with the email and password you just registered to get started.
            </p>
            <Button variant="primary" size="md" onClick={() => auth.signinRedirect()}>
              Sign in now <ArrowRight size={14} />
            </Button>
          </div>
        </div>
      </div>
    )
  }

  const pw = values.password
  const str = strength(pw)

  return (
    <div className={styles.page}>
      <div className={styles.card}>
        <div className={styles.brandRow}>
          <div className={styles.brandIcon}><Zap size={15} strokeWidth={2.5} /></div>
          <span className={styles.brandName}>Flash Sales</span>
        </div>

        <div className={styles.header}>
          <h2 className={styles.title}>Create your account</h2>
          <p className={styles.sub}>Join as a buyer. Upgrade to seller anytime.</p>
        </div>

        <Button variant="outline" size="md" onClick={() => auth.signinRedirect({ extraQueryParams: { kc_idp_hint: 'github' } })}>
          <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
            <path d="M12 2C6.477 2 2 6.477 2 12c0 4.418 2.865 8.166 6.839 9.489.5.092.682-.217.682-.482 0-.237-.009-.868-.013-1.703-2.782.604-3.369-1.342-3.369-1.342-.454-1.155-1.11-1.462-1.11-1.462-.908-.62.069-.608.069-.608 1.003.07 1.531 1.03 1.531 1.03.892 1.529 2.341 1.087 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.11-4.555-4.943 0-1.091.39-1.984 1.029-2.683-.103-.253-.446-1.27.098-2.647 0 0 .84-.269 2.75 1.025A9.578 9.578 0 0 1 12 6.836a9.59 9.59 0 0 1 2.504.337c1.909-1.294 2.747-1.025 2.747-1.025.546 1.377.202 2.394.1 2.647.64.699 1.028 1.592 1.028 2.683 0 3.842-2.339 4.687-4.566 4.935.359.309.678.919.678 1.852 0 1.336-.012 2.415-.012 2.743 0 .267.18.578.688.48C19.138 20.163 22 16.418 22 12c0-5.523-4.477-10-10-10z"/>
          </svg>
          Continue with GitHub
        </Button>

        <div className={styles.divider}><span className={styles.divLine}/><span className={styles.divLabel}>or register with email</span><span className={styles.divLine}/></div>

        <form onSubmit={handleSubmit} noValidate className={styles.form}>
          {apiError && <div className={styles.apiError}>{apiError}</div>}

          <div className={styles.row}>
            <Input label="First name" name="firstName" placeholder="Jane"
              icon={User} value={values.firstName}
              onChange={set('firstName')} onBlur={blur('firstName')} error={errors.firstName} />
            <Input label="Last name" name="lastName" placeholder="Doe"
              icon={User} value={values.lastName}
              onChange={set('lastName')} onBlur={blur('lastName')} error={errors.lastName} />
          </div>

          <Input label="Email" type="email" name="email" placeholder="you@example.com"
            icon={Mail} value={values.email}
            onChange={set('email')} onBlur={blur('email')} error={errors.email} />

          <Input label="Date of birth" type="date" name="birthDate"
            icon={Calendar} value={values.birthDate}
            onChange={set('birthDate')} onBlur={blur('birthDate')} error={errors.birthDate} />

          <div>
            <Input label="Password" type="password" name="password" placeholder="Min. 8 characters"
              icon={Lock} value={values.password}
              onChange={set('password')} onBlur={blur('password')} error={errors.password} />
            {pw && (
              <div className={styles.strengthBar}>
                <div className={styles.strengthSegs}>
                  {[1,2,3,4].map(i => (
                    <div key={i} className={styles.seg}
                      style={{ background: i <= str ? STRENGTH_COLOR[str] : 'var(--border)', transition: `background 0.3s ease ${i*0.04}s` }} />
                  ))}
                </div>
                <span style={{ color: STRENGTH_COLOR[str] }} className={styles.strengthLabel}>
                  {STRENGTH_LABEL[str]}
                </span>
              </div>
            )}
          </div>

          <Input label="Confirm password" type="password" name="confirmPassword" placeholder="Repeat your password"
            icon={Lock} value={values.confirmPassword}
            onChange={set('confirmPassword')} onBlur={blur('confirmPassword')} error={errors.confirmPassword} />

          <p className={styles.terms}>
            By registering you agree to our <a href="#">Terms of Service</a> and <a href="#">Privacy Policy</a>.
          </p>

          <Button type="submit" variant="primary" size="md" loading={loading}>
            Create account <ArrowRight size={14} />
          </Button>
        </form>

        <p className={styles.switchText}>
          Already have an account?{' '}
          <button className={styles.switchLink} onClick={() => auth.signinRedirect()}>Sign in</button>
        </p>
      </div>
    </div>
  )
}
