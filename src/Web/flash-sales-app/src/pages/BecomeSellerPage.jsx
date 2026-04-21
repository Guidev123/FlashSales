import { useState } from 'react'
import { useAuth } from 'react-oidc-context'
import { useNavigate } from 'react-router-dom'
import { Landmark, Hash, CreditCard, ArrowRight, CheckCircle2, Zap, ChevronLeft } from 'lucide-react'
import Input from '../components/Input.jsx'
import Button from '../components/Button.jsx'
import styles from './AuthFormPage.module.css'
import sellerStyles from './BecomeSellerPage.module.css'

const ACCOUNT_TYPES = [
  { value: 'Checking', label: 'Checking account' },
  { value: 'Savings',  label: 'Savings account'  },
]

function validate(v) {
  const e = {}
  if (!v.document?.trim())       e.document      = 'CPF is required'
  else if (!/^\d{11}$/.test(v.document.replace(/\D/g, '')))
                                 e.document      = 'Enter a valid CPF (11 digits)'
  if (!v.bankCode?.trim())       e.bankCode      = 'Bank code is required'
  else if (!/^\d{3}$/.test(v.bankCode.trim()))
                                 e.bankCode      = 'Bank code must be 3 digits'
  if (!v.agency?.trim())         e.agency        = 'Agency is required'
  if (!v.accountNumber?.trim())  e.accountNumber = 'Account number is required'
  if (!v.accountType)            e.accountType   = 'Select an account type'
  return e
}

function formatCPF(raw) {
  const digits = raw.replace(/\D/g, '').slice(0, 11)
  return digits
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d{1,2})$/, '$1-$2')
}

export default function BecomeSellerPage() {
  const auth     = useAuth()
  const navigate = useNavigate()

  const [values, setValues] = useState({
    document: '', bankCode: '', agency: '', accountNumber: '', accountType: '',
  })
  const [errors,  setErrors]   = useState({})
  const [touched, setTouched]  = useState({})
  const [loading, setLoading]  = useState(false)
  const [done,    setDone]     = useState(false)
  const [apiError,setApiError] = useState('')

  const set = (field) => (e) => {
    let val = e.target.value
    if (field === 'document') val = formatCPF(val)
    if (field === 'bankCode') val = val.replace(/\D/g, '').slice(0, 3)
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
      const res = await fetch(`${import.meta.env.VITE_API_URL}/api/v1/users/seller/activate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${auth.user.access_token}`,
        },
        body: JSON.stringify({
          document:      values.document.replace(/\D/g, ''),
          bankCode:      values.bankCode,
          agency:        values.agency,
          agencyNumber:  values.accountNumber,
          accountType:   values.accountType,
        }),
      })
      if (!res.ok) {
        const body = await res.json().catch(() => ({}))
        setApiError(body.message || 'Activation failed. Please try again.')
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
            <h2 className={styles.title}>You're a seller now!</h2>
            <p className={styles.sub}>
              Your seller account is activated. You can now create launches and sell your digital products.
            </p>
            <Button variant="primary" size="md" onClick={() => navigate('/launches')}>
              Go to launches <ArrowRight size={14} />
            </Button>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className={styles.page}>
      <div className={styles.card}>
        <div className={styles.brandRow}>
          <div className={styles.brandIcon}><Zap size={15} strokeWidth={2.5} /></div>
          <span className={styles.brandName}>Flash Sales</span>
        </div>

        <div className={styles.header}>
          <h2 className={styles.title}>Become a seller</h2>
          <p className={styles.sub}>
            Fill in your fiscal document and bank details to start selling digital products.
          </p>
        </div>

        <form onSubmit={handleSubmit} noValidate className={styles.form}>
          {apiError && <div className={styles.apiError}>{apiError}</div>}

          <div className={sellerStyles.sectionLabel}>Fiscal information</div>

          <Input
            label="CPF"
            name="document"
            placeholder="000.000.000-00"
            icon={Hash}
            value={values.document}
            onChange={set('document')}
            onBlur={blur('document')}
            error={errors.document}
            hint="Your individual taxpayer identification number."
          />

          <div className={sellerStyles.sectionLabel}>Bank details</div>

          <div className={styles.row}>
            <Input
              label="Bank code"
              name="bankCode"
              placeholder="001"
              icon={Landmark}
              value={values.bankCode}
              onChange={set('bankCode')}
              onBlur={blur('bankCode')}
              error={errors.bankCode}
              hint="3-digit BACEN code"
            />
            <Input
              label="Agency"
              name="agency"
              placeholder="0001"
              icon={Landmark}
              value={values.agency}
              onChange={set('agency')}
              onBlur={blur('agency')}
              error={errors.agency}
            />
          </div>

          <Input
            label="Account number"
            name="accountNumber"
            placeholder="00000000-0"
            icon={CreditCard}
            value={values.accountNumber}
            onChange={set('accountNumber')}
            onBlur={blur('accountNumber')}
            error={errors.accountNumber}
          />

          <div>
            <label className={sellerStyles.radioLabel}>Account type</label>
            <div className={sellerStyles.radioGroup}>
              {ACCOUNT_TYPES.map(opt => (
                <label
                  key={opt.value}
                  className={`${sellerStyles.radioOption} ${values.accountType === opt.value ? sellerStyles.radioSelected : ''}`}
                >
                  <input
                    type="radio"
                    name="accountType"
                    value={opt.value}
                    checked={values.accountType === opt.value}
                    onChange={() => {
                      setValues(v => ({ ...v, accountType: opt.value }))
                      setErrors(prev => ({ ...prev, accountType: undefined }))
                    }}
                    className={sellerStyles.radioInput}
                  />
                  {opt.label}
                </label>
              ))}
            </div>
            {errors.accountType && <span className={sellerStyles.radioError}>{errors.accountType}</span>}
          </div>

          <Button type="submit" variant="primary" size="md" loading={loading}>
            Activate seller account <ArrowRight size={14} />
          </Button>
        </form>

        <button className={styles.logoutLink} onClick={() => navigate('/launches')}>
          <ChevronLeft size={13} /> Back to launches
        </button>
      </div>
    </div>
  )
}
