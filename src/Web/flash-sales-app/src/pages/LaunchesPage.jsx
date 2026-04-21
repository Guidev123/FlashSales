import { useAuth } from 'react-oidc-context'
import { Clock, ArrowRight, TrendingUp, Users, Zap } from 'lucide-react'
import { Link } from 'react-router-dom'
import Navbar from '../components/Navbar.jsx'
import styles from './LaunchesPage.module.css'
import clsx from 'clsx'

const MOCK_LAUNCHES = [
  {
    id: 1,
    title: 'Full-Stack Architecture Masterclass',
    seller: 'Rafael Torres',
    category: 'Course',
    price: 'R$ 497',
    stock: 200,
    sold: 187,
    status: 'live',
    endsIn: '2h 14m',
    desc: 'Deep dive into distributed systems, event-driven architecture and scalable API design.',
  },
  {
    id: 2,
    title: 'Personal Finance Mentorship — Q3 Cohort',
    seller: 'Ana Beatriz',
    category: 'Mentorship',
    price: 'R$ 1.200',
    stock: 30,
    sold: 30,
    status: 'sold_out',
    endsIn: null,
    desc: 'Monthly group sessions with hands-on financial planning for tech professionals.',
  },
  {
    id: 3,
    title: 'Design Systems in Practice',
    seller: 'Mateus Leal',
    category: 'E-book',
    price: 'R$ 97',
    stock: 500,
    sold: 312,
    status: 'live',
    endsIn: '5h 02m',
    desc: 'A practical guide to building and maintaining scalable design systems from scratch.',
  },
  {
    id: 4,
    title: 'Cloud Native with Kubernetes',
    seller: 'Julia Moraes',
    category: 'Course',
    price: 'R$ 790',
    stock: 100,
    sold: 0,
    status: 'upcoming',
    endsIn: null,
    desc: 'From zero to production-ready: containers, orchestration, observability, and GitOps.',
  },
  {
    id: 5,
    title: '1:1 Consultancy — Backend Engineering',
    seller: 'Pedro Alves',
    category: 'Consultancy',
    price: 'R$ 350',
    stock: 10,
    sold: 7,
    status: 'live',
    endsIn: '0h 48m',
    desc: 'One hour deep-dive on your backend challenges with a senior engineer.',
  },
  {
    id: 6,
    title: 'Data Engineering Bootcamp',
    seller: 'Camila Nunes',
    category: 'Course',
    price: 'R$ 990',
    stock: 150,
    sold: 0,
    status: 'upcoming',
    endsIn: null,
    desc: 'Spark, dbt, Airflow, and modern lakehouse architecture — 8 weeks live.',
  },
]

const STATUS_MAP = {
  live:     { label: 'Live',     cls: 'live' },
  sold_out: { label: 'Sold out', cls: 'soldOut' },
  upcoming: { label: 'Upcoming', cls: 'upcoming' },
}

function StockBar({ stock, sold }) {
  const pct = Math.min((sold / stock) * 100, 100)
  return (
    <div className={styles.stockBar}>
      <div className={styles.stockFill} style={{ width: `${pct}%` }} />
    </div>
  )
}

function LaunchCard({ launch: l }) {
  const s = STATUS_MAP[l.status]
  const remaining = l.stock - l.sold

  return (
    <div className={clsx(styles.card, l.status === 'sold_out' && styles.cardDimmed)}>
      <div className={styles.cardTop}>
        <span className={styles.category}>{l.category}</span>
        <span className={clsx(styles.badge, styles[s.cls])}>{s.label}</span>
      </div>

      <div className={styles.cardBody}>
        <h3 className={styles.cardTitle}>{l.title}</h3>
        <p className={styles.cardDesc}>{l.desc}</p>
        <p className={styles.cardSeller}>by {l.seller}</p>
      </div>

      <div className={styles.cardMid}>
        <StockBar stock={l.stock} sold={l.sold} />
        <div className={styles.stockRow}>
          <span className={styles.stockText}>
            {l.status === 'sold_out'
              ? 'All units sold'
              : l.status === 'upcoming'
              ? `${l.stock} units available`
              : `${remaining} of ${l.stock} left`}
          </span>
          {l.endsIn && (
            <span className={styles.timer}><Clock size={11} />{l.endsIn}</span>
          )}
        </div>
      </div>

      <div className={styles.cardFooter}>
        <span className={styles.price}>{l.price}</span>
        <button
          className={clsx(styles.cardBtn, l.status === 'sold_out' && styles.cardBtnDisabled)}
          disabled={l.status === 'sold_out'}
        >
          {l.status === 'upcoming'
            ? 'Notify me'
            : l.status === 'sold_out'
            ? 'Sold out'
            : 'Buy now'}
          {l.status !== 'sold_out' && <ArrowRight size={13} />}
        </button>
      </div>
    </div>
  )
}

export default function LaunchesPage() {
  const auth = useAuth()
  const name = auth.user?.profile?.given_name || auth.user?.profile?.name || 'there'

  const live     = MOCK_LAUNCHES.filter(l => l.status === 'live')
  const upcoming = MOCK_LAUNCHES.filter(l => l.status === 'upcoming')
  const soldOut  = MOCK_LAUNCHES.filter(l => l.status === 'sold_out')

  return (
    <div className={styles.page}>
      <Navbar />

      <div className={styles.inner}>
        <div className={styles.pageHeader}>
          <div>
            <h1 className={styles.pageTitle}>Hey, {name.split(' ')[0]} 👋</h1>
            <p className={styles.pageSub}>Browse active and upcoming launches below.</p>
          </div>
          <div className={styles.summaryPills}>
            <span className={styles.summaryPill}>
              <Zap size={12} />
              {live.length} live
            </span>
            <span className={styles.summaryPill}>
              <TrendingUp size={12} />
              {upcoming.length} upcoming
            </span>
          </div>
        </div>

        {live.length > 0 && (
          <section className={styles.section}>
            <div className={styles.sectionHead}>
              <h2 className={styles.sectionTitle}>Live now</h2>
              <span className={styles.liveDot} />
            </div>
            <div className={styles.grid}>
              {live.map(l => <LaunchCard key={l.id} launch={l} />)}
            </div>
          </section>
        )}

        {upcoming.length > 0 && (
          <section className={styles.section}>
            <h2 className={styles.sectionTitle}>Upcoming</h2>
            <div className={styles.grid}>
              {upcoming.map(l => <LaunchCard key={l.id} launch={l} />)}
            </div>
          </section>
        )}

        {soldOut.length > 0 && (
          <section className={styles.section}>
            <h2 className={clsx(styles.sectionTitle, styles.sectionTitleMuted)}>Recently sold out</h2>
            <div className={styles.grid}>
              {soldOut.map(l => <LaunchCard key={l.id} launch={l} />)}
            </div>
          </section>
        )}
      </div>
    </div>
  )
}
