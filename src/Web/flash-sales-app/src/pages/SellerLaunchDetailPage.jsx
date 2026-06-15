import { useState, useEffect } from 'react'
import { useAuth } from 'react-oidc-context'
import { useNavigate, useParams } from 'react-router-dom'
import {
  ChevronLeft, CalendarClock, AlertCircle, CheckCircle2,
  XCircle, TrendingUp, Clock,
} from 'lucide-react'
import { useApiFetch } from '../hooks/useApiFetch.js'
import Navbar from '../components/Navbar.jsx'
import Button from '../components/Button.jsx'
import styles from './SellerLaunchDetailPage.module.css'

function formatPrice(value) {
  if (value == null) return '—'
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
}

function formatDate(iso) {
  if (!iso) return '—'
  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  }).format(new Date(iso))
}

function toLocalDatetimeValue(iso) {
  if (!iso) return ''
  const d = new Date(iso)
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function nowDatetimeValue() {
  return toLocalDatetimeValue(new Date().toISOString())
}

const STATUS_MAP = {
  Draft:         { label: 'Draft',     cls: 'statusDraft'     },
  Scheduled:     { label: 'Scheduled', cls: 'statusScheduled' },
  Active:        { label: 'Live',      cls: 'statusLive'      },
  ClosedByTime:  { label: 'Ended',     cls: 'statusClosed'    },
  ClosedByStock: { label: 'Sold out',  cls: 'statusClosed'    },
  Cancelled:     { label: 'Cancelled', cls: 'statusCancelled' },
}

function validateSchedule(v) {
  const e = {}
  const disc = parseFloat(v.discountedPrice)
  const orig = parseFloat(v.originalPrice)
  const qty  = parseInt(v.totalQuantity, 10)

  if (!v.discountedPrice || isNaN(disc) || disc <= 0)
    e.discountedPrice = 'Enter a valid promotional price'
  if (!v.originalPrice || isNaN(orig) || orig <= 0)
    e.originalPrice = 'Enter a valid original price'
  if (!isNaN(disc) && !isNaN(orig) && orig < disc)
    e.originalPrice = 'Original price must be ≥ promotional price'
  if (!v.totalQuantity || isNaN(qty) || qty < 1)
    e.totalQuantity = 'Enter a valid quantity (min 1)'
  if (!v.startAt)
    e.startAt = 'Select a start date'
  else if (new Date(v.startAt) <= new Date())
    e.startAt = 'Start date must be in the future'
  if (!v.endAt)
    e.endAt = 'Select an end date'
  else if (v.startAt && new Date(v.endAt) <= new Date(new Date(v.startAt).getTime() + 3600000))
    e.endAt = 'End date must be at least 1 hour after start'
  return e
}

function ScheduleForm({ launchId, onScheduled, authHeader, apiFetch }) {
  const [values, setValues] = useState({
    discountedPrice: '',
    originalPrice:   '',
    totalQuantity:   '',
    startAt:         '',
    endAt:           '',
  })
  const [errors,   setErrors]   = useState({})
  const [touched,  setTouched]  = useState({})
  const [loading,  setLoading]  = useState(false)
  const [apiError, setApiError] = useState('')

  const set = (field) => (e) => {
    const val = e.target.value
    setValues(v => ({ ...v, [field]: val }))
    if (touched[field]) {
      const errs = validateSchedule({ ...values, [field]: val })
      setErrors(prev => ({ ...prev, [field]: errs[field] }))
    }
  }

  const blur = (field) => () => {
    setTouched(v => ({ ...v, [field]: true }))
    setErrors(prev => ({ ...prev, [field]: validateSchedule(values)[field] }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    const errs = validateSchedule(values)
    setErrors(errs)
    setTouched(Object.keys(values).reduce((a, k) => ({ ...a, [k]: true }), {}))
    if (Object.keys(errs).length) return

    setLoading(true)
    setApiError('')
    try {
      const res = await apiFetch(
        `${import.meta.env.VITE_API_URL}/api/v1/launches/schedule`,
        {
          method: 'PATCH',
          headers: { 'Content-Type': 'application/json', ...authHeader },
          body: JSON.stringify({
            launchId:         launchId,
            discountedPrice:  parseFloat(values.discountedPrice),
            originalPrice:    parseFloat(values.originalPrice),
            totalQuantity:    parseInt(values.totalQuantity, 10),
            reservedQuantity: 0,
            startAt:          new Date(values.startAt).toISOString(),
            endAt:            new Date(values.endAt).toISOString(),
          }),
        }
      )
      if (!res) return
      if (!res.ok) {
        const body = await res.json().catch(() => ({}))
        setApiError(body.detail || body.message || 'Could not schedule launch. Please try again.')
        return
      }
      onScheduled()
    } catch {
      setApiError('Could not reach the server. Check your connection.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className={styles.card}>
      <div className={styles.cardHeader}>
        <div className={styles.iconWrap}><CalendarClock size={18} /></div>
        <div>
          <p className={styles.cardLabel}>Schedule this launch</p>
          <p className={styles.cardSub}>Set pricing, stock, and dates to publish your launch</p>
        </div>
      </div>

      <form onSubmit={handleSubmit} noValidate className={styles.form}>
        {apiError && (
          <div className={styles.apiError}><AlertCircle size={14} /> {apiError}</div>
        )}

        {/* Pricing */}
        <div className={styles.formSection}>
          <p className={styles.sectionLabel}>Pricing</p>
          <div className={styles.row2}>
            <div className={styles.fieldGroup}>
              <label className={styles.label}>
                Promotional price (R$)
                {errors.discountedPrice && touched.discountedPrice && (
                  <span className={styles.fieldError}>{errors.discountedPrice}</span>
                )}
              </label>
              <input
                type="number"
                min="0.01"
                step="0.01"
                className={`${styles.input} ${errors.discountedPrice && touched.discountedPrice ? styles.inputError : ''}`}
                placeholder="e.g. 297.00"
                value={values.discountedPrice}
                onChange={set('discountedPrice')}
                onBlur={blur('discountedPrice')}
              />
            </div>

            <div className={styles.fieldGroup}>
              <label className={styles.label}>
                Original price (R$)
                {errors.originalPrice && touched.originalPrice && (
                  <span className={styles.fieldError}>{errors.originalPrice}</span>
                )}
              </label>
              <input
                type="number"
                min="0.01"
                step="0.01"
                className={`${styles.input} ${errors.originalPrice && touched.originalPrice ? styles.inputError : ''}`}
                placeholder="e.g. 497.00"
                value={values.originalPrice}
                onChange={set('originalPrice')}
                onBlur={blur('originalPrice')}
              />
            </div>
          </div>
        </div>

        {/* Stock */}
        <div className={styles.formSection}>
          <p className={styles.sectionLabel}>Stock</p>
          <div className={styles.fieldGroup}>
            <label className={styles.label}>
              Total units available
              {errors.totalQuantity && touched.totalQuantity && (
                <span className={styles.fieldError}>{errors.totalQuantity}</span>
              )}
            </label>
            <input
              type="number"
              min="1"
              step="1"
              className={`${styles.input} ${errors.totalQuantity && touched.totalQuantity ? styles.inputError : ''}`}
              placeholder="e.g. 100"
              value={values.totalQuantity}
              onChange={set('totalQuantity')}
              onBlur={blur('totalQuantity')}
            />
            <p className={styles.inputHint}>Max 5 units per buyer. Stock cannot be increased after launch starts.</p>
          </div>
        </div>

        {/* Dates */}
        <div className={styles.formSection}>
          <p className={styles.sectionLabel}>Schedule</p>
          <div className={styles.row2}>
            <div className={styles.fieldGroup}>
              <label className={styles.label}>
                Start date & time
                {errors.startAt && touched.startAt && (
                  <span className={styles.fieldError}>{errors.startAt}</span>
                )}
              </label>
              <input
                type="datetime-local"
                min={nowDatetimeValue()}
                className={`${styles.input} ${errors.startAt && touched.startAt ? styles.inputError : ''}`}
                value={values.startAt}
                onChange={set('startAt')}
                onBlur={blur('startAt')}
              />
            </div>

            <div className={styles.fieldGroup}>
              <label className={styles.label}>
                End date & time
                {errors.endAt && touched.endAt && (
                  <span className={styles.fieldError}>{errors.endAt}</span>
                )}
              </label>
              <input
                type="datetime-local"
                min={values.startAt || nowDatetimeValue()}
                className={`${styles.input} ${errors.endAt && touched.endAt ? styles.inputError : ''}`}
                value={values.endAt}
                onChange={set('endAt')}
                onBlur={blur('endAt')}
              />
            </div>
          </div>
          <p className={styles.inputHint}>Minimum duration is 1 hour. The launch closes automatically at the end time.</p>
        </div>

        <Button type="submit" variant="primary" size="md" loading={loading}>
          <CheckCircle2 size={15} />
          Schedule launch
        </Button>
      </form>
    </div>
  )
}

export default function SellerLaunchDetailPage() {
  const { id }   = useParams()
  const auth     = useAuth()
  const navigate = useNavigate()
  const apiFetch = useApiFetch()

  const [launch,     setLaunch]     = useState(null)
  const [loading,    setLoading]    = useState(true)
  const [fetchError, setFetchError] = useState('')
  const [cancelling, setCancelling] = useState(false)
  const [actionError, setActionError] = useState('')

  const authHeader = { Authorization: `Bearer ${auth.user.access_token}` }

  async function load() {
    setLoading(true)
    setFetchError('')
    try {
      const res = await apiFetch(
        `${import.meta.env.VITE_API_URL}/api/v1/launches/${id}`,
        { headers: authHeader }
      )
      if (!res) return
      if (!res.ok) { setFetchError('Could not load launch.'); return }
      setLaunch(await res.json())
    } catch {
      setFetchError('Could not reach the server. Check your connection.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [id, auth.user?.access_token])

  async function handleCancel() {
    if (!window.confirm('Are you sure you want to cancel this launch? This cannot be undone.')) return
    setCancelling(true)
    setActionError('')
    try {
      const res = await apiFetch(
        `${import.meta.env.VITE_API_URL}/api/v1/launches/${id}`,
        { method: 'PATCH', headers: authHeader }
      )
      if (!res) return
      if (!res.ok) {
        const body = await res.json().catch(() => ({}))
        setActionError(body.detail || body.message || 'Could not cancel launch.')
        return
      }
      await load()
    } catch {
      setActionError('Could not reach the server.')
    } finally {
      setCancelling(false)
    }
  }

  if (loading) {
    return (
      <div className={styles.page}>
        <Navbar />
        <div className={styles.inner}>
          <div className={styles.skeletonBack} />
          <div className={styles.skeletonHeader} />
          <div className={styles.skeletonBody} />
        </div>
      </div>
    )
  }

  if (fetchError) {
    return (
      <div className={styles.page}>
        <Navbar />
        <div className={styles.inner}>
          <button className={styles.backLink} onClick={() => navigate('/seller/launches')}>
            <ChevronLeft size={14} /> Back to launches
          </button>
          <div className={styles.errorBox}><AlertCircle size={18} /> {fetchError}</div>
        </div>
      </div>
    )
  }

  const s        = STATUS_MAP[launch.status] ?? STATUS_MAP.Draft
  const isDraft  = launch.status === 'Draft'
  const canClose = launch.status === 'Scheduled' || launch.status === 'Active'
  const isLive   = launch.status === 'Active'
  const total    = launch.totalQuantity
  const available = launch.availableQuantity
  const sold     = total != null && available != null ? total - available : null
  const pct      = total ? Math.min(((total - (available ?? 0)) / total) * 100, 100) : 0

  return (
    <div className={styles.page}>
      <Navbar />

      <div className={styles.inner}>
        <button className={styles.backLink} onClick={() => navigate('/seller/launches')}>
          <ChevronLeft size={14} /> Back to launches
        </button>

        {/* Header */}
        <div className={styles.pageHeader}>
          <div className={styles.headerLeft}>
            <h1 className={styles.launchName}>{launch.title}</h1>
            <span className={`${styles.statusBadge} ${styles[s.cls]}`}>{s.label}</span>
          </div>

          <div className={styles.headerActions}>
            {actionError && (
              <span className={styles.actionError}><AlertCircle size={13} /> {actionError}</span>
            )}
            {canClose && (
              <Button variant="danger" size="sm" loading={cancelling} onClick={handleCancel}>
                <XCircle size={13} />
                {launch.status === 'Active' ? 'Close launch' : 'Cancel launch'}
              </Button>
            )}
          </div>
        </div>

        {isDraft && (
          <ScheduleForm
            launchId={id}
            onScheduled={load}
            authHeader={authHeader}
            apiFetch={apiFetch}
          />
        )}

        <div className={styles.grid}>
          {/* Info card */}
          <div className={styles.card}>
            <p className={styles.cardLabel}>Launch information</p>
            <div className={styles.fields}>
              <div className={styles.fieldRow}>
                <span className={styles.fieldLbl}>Status</span>
                <span className={`${styles.statusBadge} ${styles[s.cls]}`}>{s.label}</span>
              </div>
              <div className={styles.fieldRow}>
                <span className={styles.fieldLbl}>Description</span>
                <span className={styles.fieldVal}>{launch.description}</span>
              </div>
              {launch.discountedPrice != null && (
                <div className={styles.fieldRow}>
                  <span className={styles.fieldLbl}>Promotional price</span>
                  <span className={styles.fieldVal}>{formatPrice(launch.discountedPrice)}</span>
                </div>
              )}
              {launch.originalPrice != null && (
                <div className={styles.fieldRow}>
                  <span className={styles.fieldLbl}>Original price</span>
                  <span className={styles.fieldVal}>{formatPrice(launch.originalPrice)}</span>
                </div>
              )}
              {launch.startAt && (
                <div className={styles.fieldRow}>
                  <span className={styles.fieldLbl}>Starts</span>
                  <span className={styles.fieldVal}><Clock size={12} /> {formatDate(launch.startAt)}</span>
                </div>
              )}
              {launch.endAt && (
                <div className={styles.fieldRow}>
                  <span className={styles.fieldLbl}>Ends</span>
                  <span className={styles.fieldVal}><Clock size={12} /> {formatDate(launch.endAt)}</span>
                </div>
              )}
            </div>
          </div>

          {/* Stock card */}
          {total != null && (
            <div className={styles.card}>
              <p className={styles.cardLabel}>Stock overview</p>

              <div className={styles.statsGrid}>
                <div className={styles.statItem}>
                  <span className={styles.statValue}>{total}</span>
                  <span className={styles.statLabel}>Total units</span>
                </div>
                <div className={styles.statItem}>
                  <span className={styles.statValue}>{sold ?? '—'}</span>
                  <span className={styles.statLabel}>Sold</span>
                </div>
                <div className={styles.statItem}>
                  <span className={styles.statValue}>{available ?? '—'}</span>
                  <span className={styles.statLabel}>Available</span>
                </div>
              </div>

              <div className={styles.stockBarWrap}>
                <div className={styles.stockBar}>
                  <div className={styles.stockFill} style={{ width: `${pct}%` }} />
                </div>
                <p className={styles.stockPct}>{Math.round(pct)}% sold</p>
              </div>

              {isLive && sold != null && launch.discountedPrice != null && (
                <div className={styles.revenueRow}>
                  <TrendingUp size={13} />
                  <span>Estimated revenue: {formatPrice(sold * launch.discountedPrice)}</span>
                </div>
              )}
            </div>
          )}
        </div>

        {isDraft && (
          <div className={styles.hintBox}>
            <CalendarClock size={14} />
            <span>
              This launch is in draft. Fill in the schedule form above to publish it.
            </span>
          </div>
        )}
      </div>
    </div>
  )
}
