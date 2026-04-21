import { useState } from 'react'
import { Eye, EyeOff } from 'lucide-react'
import clsx from 'clsx'
import styles from './Input.module.css'

export default function Input({ label, type = 'text', error, icon: Icon, hint, className, ...props }) {
  const [show, setShow] = useState(false)
  const isPassword = type === 'password'

  return (
    <div className={clsx(styles.wrapper, className)}>
      {label && <label className={styles.label}>{label}</label>}
      <div className={clsx(styles.wrap, error && styles.hasError)}>
        {Icon && <span className={styles.icon}><Icon size={15} /></span>}
        <input
          type={isPassword ? (show ? 'text' : 'password') : type}
          className={clsx(styles.input, Icon && styles.withIcon)}
          {...props}
        />
        {isPassword && (
          <button type="button" className={styles.toggle} onClick={() => setShow(v => !v)} tabIndex={-1}>
            {show ? <EyeOff size={15} /> : <Eye size={15} />}
          </button>
        )}
      </div>
      {error  && <span className={styles.error}>{error}</span>}
      {!error && hint && <span className={styles.hint}>{hint}</span>}
    </div>
  )
}
