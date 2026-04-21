import { Zap } from 'lucide-react'
import styles from './LoadingScreen.module.css'

export default function LoadingScreen() {
  return (
    <div className={styles.root}>
      <div className={styles.icon}>
        <Zap size={20} strokeWidth={2.5} />
      </div>
    </div>
  )
}
