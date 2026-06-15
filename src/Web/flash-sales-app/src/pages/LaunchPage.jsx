import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useApiFetch } from '../hooks/useApiFetch.js'
import { ChevronLeft, Clock, AlertCircle, TrendingUp, Calendar } from 'lucide-react'
import Navbar from '../components/Navbar.jsx'
import clsx from 'clsx'
import styles from './LaunchPage.module.css'

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

function timeRemaining(endAt) {
  if (!endAt) return null
  const diff = new Date(endAt) - Date.now()
  if (diff <= 0) return null
  const h = Math.floor(diff / 3600000)
  const m = Math.floor((diff % 3600000) / 60000)
  const s = Math.floor((diff % 60000) / 1000)
  if (h > 0) return `${h}h ${m.toString().padStart(2, '0')}m`
  return `${m}m ${s.toString().padStart(2, '0')}s`
}

function timeUntilStart(startAt) {
  if (!startAt) return null
  const diff = new Date(startAt) - Date.now()
  if (diff <= 0) return null
  const days = Math.floor(diff / 86400000)
  const h    = Math.floor((diff % 86400000) / 3600000)
  const m    = Math.floor((diff % 3600000) / 60000)
  if (days > 0) return `${days}d ${h}h`
  if (h > 0)    return `${h}h ${m.toString().padStart(2, '0')}m`
  return `${m}m`
}

const STATUS_MAP = {
  Draft:         { label: 'Draft',     cls: 'badgeDraft'    },
  Scheduled:     { label: 'Upcoming',  cls: 'badgeUpcoming' },
  Active:        { label: 'Live',      cls: 'badgeLive'     },
  ClosedByTime:  { label: 'Ended',     cls: 'badgeClosed'   },
  ClosedByStock: { label: 'Sold out',  cls: 'badgeSoldOut'  },
  Cancelled:     { label: 'Cancelled', cls: 'badgeClosed'   },
}

export default function LaunchPage() {
  const { id }   = useParams()
  const navigate = useNavigate()
  const apiFetch = useApiFetch()

  const [launch,  setLaunch]  = useState(null)
  const [loading, setLoading] = useState(true)
  const [error,   setError]   = useState('')
  const [, setTick] = useState(0)

  useEffect(() => {
    let cancelled = false
    async function load() {
      setLoading(true)
      setError('')
      try {
        const res = await apiFetch(`${import.meta.env.VITE_API_URL}/api/v1/launches/${id}`)
        if (!res || cancelled) return
        if (!res.ok) { setError('Launch not found.'); return }
        setLaunch(await res.json())
      } catch {
        if (!cancelled) setError('Could not reach the server. Check your connection.')
      } finally {
        if (!cancelled) setLoading(false)
      }
    }
    load()
    return () => { cancelled = true }
  }, [id])

  // Re-render every second for live countdown
  useEffect(() => {
    if (!launch || launch.status !== 'Active') return
    const interval = setInterval(() => setTick(t => t + 1), 1000)
    return () => clearInterval(interval)
  }, [launch?.status])

  if (loading) {
    return (
      <div className={styles.page}>
        <Navbar />
        <div className={styles.inner}>
          <div className={styles.skeletonBack} />
          <div className={styles.skeletonLayout} />
        </div>
      </div>
    )
  }

  if (error || !launch) {
    return (
      <div className={styles.page}>
        <Navbar />
        <div className={styles.inner}>
          <button className={styles.backLink} onClick={() => navigate('/launches')}>
            <ChevronLeft size={14} /> Back to launches
          </button>
          <div className={styles.errorBox}><AlertCircle size={18} /> {error || 'Launch not found.'}</div>
        </div>
      </div>
    )
  }

  const s         = STATUS_MAP[launch.status] ?? STATUS_MAP.Draft
  const total     = launch.totalQuantity
  const available = launch.availableQuantity
  const sold      = total != null && available != null ? total - available : null
  const pct       = total ? Math.min(((total - (available ?? 0)) / total) * 100, 100) : 0
  const endsIn    = launch.status === 'Active' ? timeRemaining(launch.endAt) : null
  const startsIn  = launch.status === 'Scheduled' ? timeUntilStart(launch.startAt) : null
  const isLive    = launch.status === 'Active'
  const isClosed  = launch.status === 'ClosedByTime' || launch.status === 'ClosedByStock' || launch.status === 'Cancelled'
  const isUpcoming = launch.status === 'Scheduled'

  const hasDiscount = launch.originalPrice != null && launch.discountedPrice != null
    && launch.originalPrice > launch.discountedPrice

  return (
    <div className={styles.page}>
      <Navbar />

      <div className={styles.inner}>
        <button className={styles.backLink} onClick={() => navigate('/launches')}>
          <ChevronLeft size={14} /> Back to launches
        </button>

        <div className={styles.layout}>
          {/* Left: details */}
          <div className={styles.details}>
            <div className={styles.titleRow}>
              <h1 className={styles.title}>{launch.title}</h1>
              <span className={clsx(styles.badge, styles[s.cls])}>{s.label}</span>
            </div>

            {isLive && endsIn && (
              <div className={styles.liveBar}>
                <span className={styles.liveDot} />
                <span className={styles.liveText}>Live now</span>
                <span className={styles.liveTimer}><Clock size={12} />{endsIn} left</span>
              </div>
            )}

            {isUpcoming && startsIn && (
              <div className={styles.upcomingBar}>
                <Clock size={13} />
                <span>Starts in <strong>{startsIn}</strong></span>
              </div>
            )}

            <p className={styles.description}>{launch.description}</p>

            <div className={styles.metaRow}>
              <div className={styles.metaItem}>
                <Calendar size={13} />
                <span>Starts: {formatDate(launch.startAt)}</span>
              </div>
              <div className={styles.metaItem}>
                <Calendar size={13} />
                <span>Ends: {formatDate(launch.endAt)}</span>
              </div>
            </div>
          </div>

          {/* Right: purchase panel */}
          <div className={clsx(styles.panel, isClosed && styles.panelDimmed)}>
            {/* Pricing */}
            <div className={styles.priceBlock}>
              <span className={styles.priceMain}>{formatPrice(launch.discountedPrice)}</span>
              {hasDiscount && (
                <div className={styles.priceRow}>
                  <span className={styles.priceOriginal}>{formatPrice(launch.originalPrice)}</span>
                  <span className={styles.discount}>
                    -{Math.round(launch.discountPercentage ?? 0)}% off
                  </span>
                </div>
              )}
            </div>

            {/* Stock */}
            {total != null && (
              <div className={styles.stockBlock}>
                <div className={styles.stockBar}>
                  <div className={styles.stockFill} style={{ width: `${pct}%` }} />
                </div>
                <div className={styles.stockRow}>
                  <span className={styles.stockText}>
                    {launch.status === 'ClosedByStock' ? 'All units sold'
                      : launch.status === 'ClosedByTime' ? 'Launch ended'
                      : launch.status === 'Scheduled' ? `${total} units available`
                      : `${available} of ${total} left`}
                  </span>
                  {sold != null && isLive && (
                    <span className={styles.soldCount}>
                      <TrendingUp size={11} />{sold} sold
                    </span>
                  )}
                </div>
              </div>
            )}

            {/* CTA */}
            {isLive && (
              <div className={styles.ctaBlock}>
                <p className={styles.ctaHint}>
                  Reserve your spot now — you'll have 10 minutes to complete checkout.
                </p>
                <button className={styles.buyBtn} disabled>
                  Buy now — purchase coming soon
                </button>
              </div>
            )}

            {isUpcoming && (
              <div className={styles.ctaBlock}>
                <p className={styles.ctaHint}>
                  This launch hasn't started yet. Come back on {formatDate(launch.startAt)}.
                </p>
                <button className={styles.notifyBtn} disabled>
                  Pre-registration coming soon
                </button>
              </div>
            )}

            {isClosed && (
              <div className={styles.closedNotice}>
                {launch.status === 'ClosedByStock'
                  ? 'All units were sold. This launch is now closed.'
                  : launch.status === 'Cancelled'
                  ? 'This launch has been cancelled.'
                  : 'This launch has ended.'}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
