import { useAuth } from 'react-oidc-context'
import { useNavigate } from 'react-router-dom'
import { Zap, Clock, Users, ShieldCheck, ArrowRight, TrendingUp } from 'lucide-react'
import { isActivated } from '../lib/auth.js'
import Navbar from '../components/Navbar.jsx'
import Button from '../components/Button.jsx'
import styles from './HomePage.module.css'
import clsx from 'clsx'

const FEATURES = [
  { icon: Zap,         title: 'Instant reservations',  desc: 'Stock reserved in under 2 seconds, even under thousands of concurrent buyers.' },
  { icon: ShieldCheck, title: 'Zero overselling',       desc: 'Atomic stock control guarantees the inventory cap is never exceeded — ever.' },
  { icon: Clock,       title: '10-min checkout window', desc: 'Reserved units are held for 10 minutes. Expired reservations automatically free stock.' },
  { icon: Users,       title: 'Smart waitlist',         desc: 'Pre-registered buyers get priority. Everyone else queues chronologically, fairly.' },
]

export default function HomePage() {
  const auth = useAuth()
  const navigate = useNavigate()
  const activated = isActivated(auth.user)

  const handleCTA = () => {
    if (auth.isAuthenticated && activated) {
      navigate('/launches')
    } else {
      auth.signinRedirect()
    }
  }

  return (
    <div className={styles.page}>
      <Navbar />

      <section className={styles.hero}>
        <div className={styles.heroBg} />
        <div className={styles.heroGlow} />
        <div className={styles.heroInner}>
          <div className={styles.pill}>
            <TrendingUp size={12} />
            High-demand digital launches
          </div>

          <h1 className={styles.heroTitle}>
            Buy exclusive digital<br />
            <span className={styles.accent}>products at flash speed.</span>
          </h1>

          <p className={styles.heroDesc}>
            Courses, mentorships, and consultancies with intentional scarcity.
            Fair waitlists, atomic stock control, zero overselling.
          </p>

          <Button variant="primary" size="lg" onClick={handleCTA}>
            {activated ? 'Browse launches' : 'Create free account'}
            <ArrowRight size={16} />
          </Button>

          <div className={styles.stats}>
            {[
              { val: '10k+', label: 'concurrent buyers' },
              { val: '<2s',  label: 'reservation latency' },
              { val: '0',    label: 'oversells ever' },
            ].map((s, i, arr) => (
              <div key={s.label} className={styles.statGroup}>
                <div className={styles.stat}>
                  <span className={styles.statVal}>{s.val}</span>
                  <span className={styles.statLabel}>{s.label}</span>
                </div>
                {i < arr.length - 1 && <div className={styles.statDivider} />}
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className={styles.features}>
        <div className={styles.container}>
          <h2 className={styles.sectionTitle}>How it works</h2>
          <p className={styles.sectionSub}>The infrastructure behind every launch</p>
          <div className={styles.featGrid}>
            {FEATURES.map(f => (
              <div key={f.title} className={styles.feat}>
                <div className={styles.featIcon}><f.icon size={17} strokeWidth={2} /></div>
                <div>
                  <h3 className={styles.featTitle}>{f.title}</h3>
                  <p className={styles.featDesc}>{f.desc}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      <footer className={styles.footer}>
        <div className={styles.container}>
          <span className={styles.footerBrand}><Zap size={13} /> Flash Sales</span>
          <span className={styles.footerCopy}>© 2025</span>
        </div>
      </footer>
    </div>
  )
}
