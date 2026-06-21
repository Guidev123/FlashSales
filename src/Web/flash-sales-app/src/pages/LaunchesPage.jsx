import { useState, useEffect } from 'react'
import { useAuth } from 'react-oidc-context'
import { Clock, ArrowRight, TrendingUp, Zap, AlertCircle } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { useApiFetch } from '../hooks/useApiFetch.js'
import Navbar from '../components/Navbar.jsx'
import styles from './LaunchesPage.module.css'
import clsx from 'clsx'

function formatPrice(value) {
  if (value == null) return '—'
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
}

function timeRemaining(endAt) {
  if (!endAt) return null
  const diff = new Date(endAt) - Date.now()
  if (diff <= 0) return null
  const h = Math.floor(diff / 3600000)
  const m = Math.floor((diff % 3600000) / 60000)
  return `${h}h ${m.toString().padStart(2, '0')}m`
}

const STATUS_MAP = {
  Active:        { label: 'Live',     cls: 'live',     section: 'live' },
  Scheduled:     { label: 'Upcoming', cls: 'upcoming', section: 'upcoming' },
  ClosedByTime:  { label: 'Ended',    cls: 'soldOut',  section: 'closed' },
  ClosedByStock: { label: 'Sold out', cls: 'soldOut',  section: 'closed' },
}

function StockBar({ total, available }) {
  if (total == null) return null
  const pct = Math.min(((total - (available ?? 0)) / total) * 100, 100)
  return (
    <div className={styles.stockBar}>
      <div className={styles.stockFill} style={{ width: `${pct}%` }} />
    </div>
  )
}

function LaunchCard({ launch: l, onClick }) {
  const s = STATUS_MAP[l.status]
  if (!s) return null
  const isDimmed = s.section === 'closed'
  const endsIn = l.status === 'Active' ? timeRemaining(l.endAt) : null

  function stockLabel() {
    if (l.totalQuantity == null) return '—'
    if (l.status === 'ClosedByStock') return 'All units sold'
    if (l.status === 'ClosedByTime')  return 'Launch ended'
    if (l.status === 'Scheduled')     return `${l.totalQuantity} units available`
    return `${l.availableQuantity} of ${l.totalQuantity} left`
  }

  return (
    <div className={clsx(styles.card, isDimmed && styles.cardDimmed)} onClick={onClick}>
      <div className={styles.cardTop}>
        <span className={styles.category}>Digital</span>
        <span className={clsx(styles.badge, styles[s.cls])}>{s.label}</span>
      </div>

      <div className={styles.cardBody}>
        <h3 className={styles.cardTitle}>{l.title}</h3>
        <p className={styles.cardDesc}>{l.description}</p>
      </div>

      <div className={styles.cardMid}>
        <StockBar total={l.totalQuantity} available={l.availableQuantity} />
        <div className={styles.stockRow}>
          <span className={styles.stockText}>{stockLabel()}</span>
          {endsIn && <span className={styles.timer}><Clock size={11} />{endsIn}</span>}
        </div>
      </div>

      <div className={styles.cardFooter}>
        <span className={styles.price}>{formatPrice(l.discountedPrice)}</span>
        <span className={clsx(styles.cardBtn, isDimmed && styles.cardBtnDisabled)}>
          {isDimmed ? 'View' : l.status === 'Scheduled' ? 'View details' : 'Buy now'}
          {!isDimmed && <ArrowRight size={13} />}
        </span>
      </div>
    </div>
  )
}

export default function LaunchesPage() {
  const auth     = useAuth()
  const navigate = useNavigate()
  const apiFetch = useApiFetch()
  const name = auth.user?.profile?.given_name || auth.user?.profile?.name || null

  const [launches, setLaunches] = useState([])
  const [loading,  setLoading]  = useState(true)
  const [error,    setError]    = useState('')

  useEffect(() => {
    let cancelled = false
    async function load() {
      setLoading(true)
      setError('')
      try {
        const res = await apiFetch(
          `${import.meta.env.VITE_API_URL}/api/v1/launches?page=1&size=50`,
          auth.user ? { headers: { Authorization: `Bearer ${auth.user.access_token}` } } : {}
        )
        if (!res || cancelled) return
        if (!res.ok) { setError('Could not load launches. Please try again.'); return }
        const data = await res.json()
        setLaunches(data.items ?? [])
      } catch {
        if (!cancelled) setError('Could not reach the server. Check your connection.')
      } finally {
        if (!cancelled) setLoading(false)
      }
    }
    load()
    return () => { cancelled = true }
  }, [])

  const live     = launches.filter(l => l.status === 'Active')
  const upcoming = launches.filter(l => l.status === 'Scheduled')
  const closed   = launches.filter(l => l.status === 'ClosedByTime' || l.status === 'ClosedByStock')

  return (
    <div className={styles.page}>
      <Navbar />
      <div className={styles.inner}>
        <div className={styles.pageHeader}>
          <div>
            <h1 className={styles.pageTitle}>{name ? `Hey, ${name.split(' ')[0]}` : 'Launches'}</h1>
            <p className={styles.pageSub}>Browse active and upcoming launches below.</p>
          </div>
          <div className={styles.summaryPills}>
            <span className={styles.summaryPill}><Zap size={12} />{live.length} live</span>
            <span className={styles.summaryPill}><TrendingUp size={12} />{upcoming.length} upcoming</span>
          </div>
        </div>

        {loading && (
          <div className={styles.grid}>
            {Array.from({ length: 6 }).map((_, i) => <div key={i} className={styles.skeletonCard} />)}
          </div>
        )}

        {!loading && error && (
          <div className={styles.errorBox}><AlertCircle size={18} /> {error}</div>
        )}

        {!loading && !error && live.length === 0 && upcoming.length === 0 && closed.length === 0 && (
          <div className={styles.emptyState}>
            <Zap size={40} strokeWidth={1.2} />
            <p className={styles.emptyTitle}>No launches yet</p>
            <p className={styles.emptySub}>Check back soon — new launches are coming.</p>
          </div>
        )}

        {!loading && !error && live.length > 0 && (
          <section className={styles.section}>
            <div className={styles.sectionHead}>
              <h2 className={styles.sectionTitle}>Live now</h2>
              <span className={styles.liveDot} />
            </div>
            <div className={styles.grid}>
              {live.map(l => <LaunchCard key={l.id} launch={l} onClick={() => navigate(`/launches/${l.id}`)} />)}
            </div>
          </section>
        )}

        {!loading && !error && upcoming.length > 0 && (
          <section className={styles.section}>
            <h2 className={styles.sectionTitle}>Upcoming</h2>
            <div className={styles.grid}>
              {upcoming.map(l => <LaunchCard key={l.id} launch={l} onClick={() => navigate(`/launches/${l.id}`)} />)}
            </div>
          </section>
        )}

        {!loading && !error && closed.length > 0 && (
          <section className={styles.section}>
            <h2 className={clsx(styles.sectionTitle, styles.sectionTitleMuted)}>Recently closed</h2>
            <div className={styles.grid}>
              {closed.map(l => <LaunchCard key={l.id} launch={l} onClick={() => navigate(`/launches/${l.id}`)} />)}
            </div>
          </section>
        )}
      </div>
    </div>
  )
}
