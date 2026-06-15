import { useState, useEffect } from 'react'
import { useAuth } from 'react-oidc-context'
import { useNavigate } from 'react-router-dom'
import { Plus, Rocket, AlertCircle, Clock, TrendingUp } from 'lucide-react'
import { useApiFetch } from '../hooks/useApiFetch.js'
import Navbar from '../components/Navbar.jsx'
import Button from '../components/Button.jsx'
import clsx from 'clsx'
import styles from './SellerLaunchesPage.module.css'

function formatPrice(value) {
  if (value == null) return '—'
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
}

function formatDate(iso) {
  if (!iso) return '—'
  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit', month: 'short', year: 'numeric',
  }).format(new Date(iso))
}

const STATUS_MAP = {
  Draft:         { label: 'Draft',     cls: 'statusDraft'    },
  Scheduled:     { label: 'Scheduled', cls: 'statusScheduled' },
  Active:        { label: 'Live',      cls: 'statusLive'     },
  ClosedByTime:  { label: 'Ended',     cls: 'statusClosed'   },
  ClosedByStock: { label: 'Sold out',  cls: 'statusClosed'   },
  Cancelled:     { label: 'Cancelled', cls: 'statusCancelled' },
}

function LaunchCard({ launch: l, onClick }) {
  const s = STATUS_MAP[l.status] ?? STATUS_MAP.Draft

  return (
    <button className={styles.card} onClick={onClick}>
      <div className={styles.cardTop}>
        <span className={`${styles.statusBadge} ${styles[s.cls]}`}>{s.label}</span>
      </div>

      <div className={styles.cardBody}>
        <p className={styles.launchTitle}>{l.title}</p>
        <p className={styles.launchDesc}>{l.description}</p>
      </div>

      <div className={styles.cardMeta}>
        {l.discountedPrice != null && (
          <span className={styles.metaItem}>
            <TrendingUp size={11} />
            {formatPrice(l.discountedPrice)}
          </span>
        )}
        {(l.totalQuantity != null) && (
          <span className={styles.metaItem}>
            {l.availableQuantity ?? l.totalQuantity} / {l.totalQuantity} left
          </span>
        )}
        {l.startAt && (
          <span className={styles.metaItem}>
            <Clock size={11} />
            {formatDate(l.startAt)}
          </span>
        )}
      </div>
    </button>
  )
}

export default function SellerLaunchesPage() {
  const auth     = useAuth()
  const navigate = useNavigate()
  const apiFetch = useApiFetch()

  const sellerId = auth.user?.profile?.sub

  const [launches, setLaunches] = useState([])
  const [loading,  setLoading]  = useState(true)
  const [error,    setError]    = useState('')
  const [page,     setPage]     = useState(1)
  const [meta,     setMeta]     = useState(null)

  useEffect(() => {
    if (!sellerId) return
    let cancelled = false

    async function load() {
      setLoading(true)
      setError('')
      try {
        const res = await apiFetch(
          `${import.meta.env.VITE_API_URL}/api/v1/launches/seller/${sellerId}?page=${page}&size=12`,
          { headers: { Authorization: `Bearer ${auth.user.access_token}` } }
        )
        if (!res || cancelled) return
        if (!res.ok) { setError('Could not load your launches. Please try again.'); return }
        const data = await res.json()
        setLaunches(data.items ?? [])
        setMeta(data)
      } catch {
        if (!cancelled) setError('Could not reach the server. Check your connection.')
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    load()
    return () => { cancelled = true }
  }, [auth.user?.access_token, sellerId, page])

  return (
    <div className={styles.page}>
      <Navbar />

      <div className={styles.inner}>
        <div className={styles.pageHeader}>
          <div>
            <h1 className={styles.pageTitle}>My launches</h1>
            <p className={styles.pageSub}>Manage your digital product launch events</p>
          </div>
          <Button variant="primary" size="sm" onClick={() => navigate('/seller/launches/new')}>
            <Plus size={14} />
            New launch
          </Button>
        </div>

        {loading && (
          <div className={styles.grid}>
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className={styles.skeletonCard} />
            ))}
          </div>
        )}

        {!loading && error && (
          <div className={styles.errorBox}>
            <AlertCircle size={18} /> {error}
          </div>
        )}

        {!loading && !error && launches.length === 0 && (
          <div className={styles.emptyState}>
            <Rocket size={40} strokeWidth={1.2} />
            <p className={styles.emptyTitle}>No launches yet</p>
            <p className={styles.emptySub}>Create your first launch to start selling your digital products.</p>
            <Button variant="primary" size="sm" onClick={() => navigate('/seller/launches/new')}>
              <Plus size={14} />
              Create launch
            </Button>
          </div>
        )}

        {!loading && !error && launches.length > 0 && (
          <>
            <div className={styles.grid}>
              {launches.map(l => (
                <LaunchCard
                  key={l.id}
                  launch={l}
                  onClick={() => navigate(`/seller/launches/${l.id}`)}
                />
              ))}
            </div>

            {meta && (meta.hasNextPage || meta.hasPreviousPage) && (
              <div className={styles.pagination}>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!meta.hasPreviousPage}
                  onClick={() => setPage(p => p - 1)}
                >
                  Previous
                </Button>
                <span className={styles.pageInfo}>Page {meta.page} of {meta.totalPages}</span>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!meta.hasNextPage}
                  onClick={() => setPage(p => p + 1)}
                >
                  Next
                </Button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  )
}
