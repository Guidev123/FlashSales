import { useState, useEffect, useRef } from 'react'
import { useAuth } from 'react-oidc-context'
import {
  Camera, Landmark, Hash, CreditCard, Mail, User,
  BadgeCheck, AlertCircle, Pencil, X, Check,
} from 'lucide-react'
import { useApiFetch } from '../hooks/useApiFetch.js'
import Navbar from '../components/Navbar.jsx'
import Input from '../components/Input.jsx'
import Button from '../components/Button.jsx'
import styles from './SellerProfilePage.module.css'

const MAX_FILE_SIZE = 5 * 1024 * 1024

const ACCOUNT_TYPES = [
  { value: 'Checking', label: 'Checking account' },
  { value: 'Savings',  label: 'Savings account'  },
]

function formatDoc(doc) {
  return doc?.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4') ?? '—'
}

function validatePayment(v) {
  const e = {}
  if (!v.bankCode?.trim())       e.bankCode      = 'Bank code is required'
  else if (!/^\d{3}$/.test(v.bankCode.trim()))
                                 e.bankCode      = 'Bank code must be 3 digits'
  if (!v.agency?.trim())         e.agency        = 'Agency is required'
  if (!v.accountNumber?.trim())  e.accountNumber = 'Account number is required'
  if (!v.accountType)            e.accountType   = 'Select an account type'
  return e
}

function Field({ label, value, icon: Icon }) {
  return (
    <div className={styles.fieldRow}>
      <span className={styles.fieldLabel}>
        {Icon && <Icon size={12} />}
        {label}
      </span>
      <span className={styles.fieldValue}>{value || '—'}</span>
    </div>
  )
}

export default function SellerProfilePage() {
  const auth     = useAuth()
  const apiFetch = useApiFetch()
  const fileRef  = useRef(null)

  const [profile,      setProfile]      = useState(null)
  const [loading,      setLoading]      = useState(true)
  const [fetchError,   setFetchError]   = useState('')
  const [previewUrl,   setPreviewUrl]   = useState(null)
  const [uploading,    setUploading]    = useState(false)
  const [uploadError,  setUploadError]  = useState('')

  // payment account edit state
  const [editingPayment, setEditingPayment] = useState(false)
  const [payForm,        setPayForm]        = useState({ bankCode: '', agency: '', accountNumber: '', accountType: '' })
  const [payErrors,      setPayErrors]      = useState({})
  const [payTouched,     setPayTouched]     = useState({})
  const [savingPay,      setSavingPay]      = useState(false)
  const [payError,       setPayError]       = useState('')

  useEffect(() => {
    async function load() {
      setLoading(true)
      setFetchError('')
      try {
        const res = await apiFetch(`${import.meta.env.VITE_API_URL}/api/v1/users/seller`, {
          headers: { Authorization: `Bearer ${auth.user.access_token}` },
        })
        if (!res) return
        if (!res.ok) {
          setFetchError('Could not load your profile. Please try again later.')
          return
        }
        const data = await res.json()
        setProfile(data)
        setPreviewUrl(data.profilePictureUrl ?? null)
      } catch {
        setFetchError('Could not reach the server. Check your connection.')
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [auth.user?.access_token])

  // ── Profile picture upload ────────────────────────────────
  async function handleFileChange(e) {
    const file = e.target.files?.[0]
    e.target.value = ''
    if (!file) return

    if (!file.type.startsWith('image/')) {
      setUploadError('Only image files are allowed (JPEG, PNG, WebP…).')
      return
    }
    if (file.size > MAX_FILE_SIZE) {
      setUploadError('File is too large. Maximum size is 5 MB.')
      return
    }

    const objectUrl = URL.createObjectURL(file)
    setPreviewUrl(objectUrl)
    setUploadError('')
    setUploading(true)

    try {
      const formData = new FormData()
      formData.append('file', file)

      const res = await apiFetch(`${import.meta.env.VITE_API_URL}/api/v1/users/seller/picture`, {
        method: 'PATCH',
        headers: { Authorization: `Bearer ${auth.user.access_token}` },
        body: formData,
      })
      if (!res) return

      if (!res.ok) {
        const body = await res.json().catch(() => ({}))
        setUploadError(body.message || 'Upload failed. Please try again.')
        setPreviewUrl(profile?.profilePictureUrl ?? null)
        URL.revokeObjectURL(objectUrl)
      }
    } catch {
      setUploadError('Could not reach the server. Check your connection.')
      setPreviewUrl(profile?.profilePictureUrl ?? null)
      URL.revokeObjectURL(objectUrl)
    } finally {
      setUploading(false)
    }
  }

  // ── Payment account edit ──────────────────────────────────
  function openPaymentEdit() {
    setPayForm({
      bankCode:      profile.paymentAccount?.bankCode      ?? '',
      agency:        profile.paymentAccount?.agency        ?? '',
      accountNumber: profile.paymentAccount?.accountNumber ?? '',
      accountType:   profile.paymentAccount?.accountType   ?? '',
    })
    setPayErrors({})
    setPayTouched({})
    setPayError('')
    setEditingPayment(true)
  }

  function cancelPaymentEdit() {
    setEditingPayment(false)
    setPayError('')
  }

  const setPayField = (field) => (e) => {
    let val = e.target.value
    if (field === 'bankCode') val = val.replace(/\D/g, '').slice(0, 3)
    setPayForm(f => ({ ...f, [field]: val }))
    if (payTouched[field]) {
      const errs = validatePayment({ ...payForm, [field]: val })
      setPayErrors(prev => ({ ...prev, [field]: errs[field] }))
    }
  }

  const blurPay = (field) => () => {
    setPayTouched(t => ({ ...t, [field]: true }))
    const errs = validatePayment(payForm)
    setPayErrors(prev => ({ ...prev, [field]: errs[field] }))
  }

  async function handleSavePayment(e) {
    e.preventDefault()
    const errs = validatePayment(payForm)
    setPayErrors(errs)
    setPayTouched(Object.keys(payForm).reduce((a, k) => ({ ...a, [k]: true }), {}))
    if (Object.keys(errs).length) return

    setSavingPay(true)
    setPayError('')
    try {
      const res = await apiFetch(`${import.meta.env.VITE_API_URL}/api/v1/users/seller/payment-account`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${auth.user.access_token}`,
        },
        body: JSON.stringify({
          bankCode:      payForm.bankCode,
          agency:        payForm.agency,
          accountNumber: payForm.accountNumber,
          accountType:   payForm.accountType,
        }),
      })
      if (!res) return
      if (!res.ok) {
        const body = await res.json().catch(() => ({}))
        setPayError(body.message || 'Could not save changes. Please try again.')
        return
      }

      setProfile(p => ({ ...p, paymentAccount: { ...payForm } }))
      setEditingPayment(false)
    } catch {
      setPayError('Could not reach the server. Check your connection.')
    } finally {
      setSavingPay(false)
    }
  }

  // ── Loading / error states ────────────────────────────────
  if (loading) {
    return (
      <div className={styles.page}>
        <Navbar />
        <div className={styles.inner}>
          <div className={styles.skeletonHeader} />
          <div className={styles.grid}>
            <div className={styles.skeletonCard} />
            <div className={styles.skeletonCard} />
          </div>
        </div>
      </div>
    )
  }

  if (fetchError) {
    return (
      <div className={styles.page}>
        <Navbar />
        <div className={styles.inner}>
          <div className={styles.errorBox}>
            <AlertCircle size={20} />
            {fetchError}
          </div>
        </div>
      </div>
    )
  }

  const initials = `${profile.firstName?.[0] ?? ''}${profile.lastName?.[0] ?? ''}`.toUpperCase()

  return (
    <div className={styles.page}>
      <Navbar />

      <div className={styles.inner}>

        {/* ── Profile header ─────────────────────────────── */}
        <div className={styles.profileHeader}>
          <div className={styles.avatarSection}>
            <button
              className={styles.avatarWrap}
              onClick={() => !uploading && fileRef.current?.click()}
              aria-label="Change profile picture"
              title="Change profile picture"
            >
              {previewUrl ? (
                <img src={previewUrl} alt="Profile" className={styles.avatarImg} />
              ) : (
                <div className={styles.avatarInitials}>{initials}</div>
              )}

              {uploading ? (
                <div className={styles.avatarOverlay}>
                  <span className={styles.spinner} />
                </div>
              ) : (
                <div className={styles.avatarOverlay}>
                  <Camera size={18} />
                </div>
              )}
            </button>

            <input
              ref={fileRef}
              type="file"
              accept="image/*"
              className={styles.fileInput}
              onChange={handleFileChange}
            />
          </div>

          <div className={styles.headerInfo}>
            <div className={styles.nameRow}>
              <h1 className={styles.sellerName}>{profile.firstName} {profile.lastName}</h1>
              <span className={styles.sellerBadge}><BadgeCheck size={13} /> Seller</span>
            </div>
            <p className={styles.sellerEmail}>{profile.email}</p>
            {uploadError && (
              <p className={styles.uploadError}><AlertCircle size={13} /> {uploadError}</p>
            )}
            <p className={styles.photoHint}>Click the avatar to change your profile picture</p>
          </div>
        </div>

        <div className={styles.grid}>

          {/* ── Personal information (read-only) ─────────── */}
          <div className={styles.infoCard}>
            <p className={styles.cardLabel}>Personal information</p>
            <div className={styles.fields}>
              <Field label="Full name"  value={`${profile.firstName} ${profile.lastName}`} icon={User} />
              <Field label="Email"      value={profile.email}               icon={Mail} />
              <Field label="CPF"        value={formatDoc(profile.document)}  icon={Hash} />
            </div>
          </div>

          {/* ── Bank details (editable) ───────────────────── */}
          <div className={styles.infoCard}>
            <div className={styles.cardHeader}>
              <p className={styles.cardLabel}>Bank details</p>
              {!editingPayment && (
                <button className={styles.editBtn} onClick={openPaymentEdit} aria-label="Edit bank details">
                  <Pencil size={13} /> Edit
                </button>
              )}
            </div>

            {editingPayment ? (
              <form onSubmit={handleSavePayment} noValidate className={styles.editForm}>
                {payError && (
                  <div className={styles.errorBox} style={{ marginBottom: 4 }}>
                    <AlertCircle size={15} /> {payError}
                  </div>
                )}

                <div className={styles.twoCol}>
                  <Input
                    label="Bank code"
                    name="bankCode"
                    placeholder="001"
                    icon={Landmark}
                    value={payForm.bankCode}
                    onChange={setPayField('bankCode')}
                    onBlur={blurPay('bankCode')}
                    error={payErrors.bankCode}
                    hint="3-digit BACEN code"
                  />
                  <Input
                    label="Agency"
                    name="agency"
                    placeholder="0001"
                    icon={Landmark}
                    value={payForm.agency}
                    onChange={setPayField('agency')}
                    onBlur={blurPay('agency')}
                    error={payErrors.agency}
                  />
                </div>

                <Input
                  label="Account number"
                  name="accountNumber"
                  placeholder="00000000-0"
                  icon={CreditCard}
                  value={payForm.accountNumber}
                  onChange={setPayField('accountNumber')}
                  onBlur={blurPay('accountNumber')}
                  error={payErrors.accountNumber}
                />

                <div>
                  <label className={styles.radioLabel}>Account type</label>
                  <div className={styles.radioGroup}>
                    {ACCOUNT_TYPES.map(opt => (
                      <label
                        key={opt.value}
                        className={`${styles.radioOption} ${payForm.accountType === opt.value ? styles.radioSelected : ''}`}
                      >
                        <input
                          type="radio"
                          name="accountType"
                          value={opt.value}
                          checked={payForm.accountType === opt.value}
                          onChange={() => {
                            setPayForm(f => ({ ...f, accountType: opt.value }))
                            setPayErrors(prev => ({ ...prev, accountType: undefined }))
                          }}
                          className={styles.radioInput}
                        />
                        {opt.label}
                      </label>
                    ))}
                  </div>
                  {payErrors.accountType && (
                    <span className={styles.radioError}>{payErrors.accountType}</span>
                  )}
                </div>

                <div className={styles.editActions}>
                  <Button type="submit" variant="primary" size="sm" loading={savingPay}>
                    <Check size={13} /> Save changes
                  </Button>
                  <Button type="button" variant="ghost" size="sm" onClick={cancelPaymentEdit} disabled={savingPay}>
                    <X size={13} /> Cancel
                  </Button>
                </div>
              </form>
            ) : (
              <div className={styles.fields}>
                <Field label="Bank code"      value={profile.paymentAccount?.bankCode}      icon={Landmark} />
                <Field label="Agency"         value={profile.paymentAccount?.agency}        icon={Landmark} />
                <Field label="Account number" value={profile.paymentAccount?.accountNumber} icon={CreditCard} />
                <Field label="Account type"   value={profile.paymentAccount?.accountType}   icon={CreditCard} />
              </div>
            )}
          </div>

        </div>
      </div>
    </div>
  )
}
