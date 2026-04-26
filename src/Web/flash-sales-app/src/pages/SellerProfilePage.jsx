import { useState, useEffect, useRef } from 'react'
import { useAuth } from 'react-oidc-context'
import { Camera, Landmark, Hash, CreditCard, Mail, User, BadgeCheck, AlertCircle } from 'lucide-react'
import { useApiFetch } from '../hooks/useApiFetch.js'
import Navbar from '../components/Navbar.jsx'
import styles from './SellerProfilePage.module.css'

const MAX_FILE_SIZE = 5 * 1024 * 1024

function formatDoc(doc) {
  return doc?.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4') ?? '—'
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

          <div className={styles.infoCard}>
            <p className={styles.cardLabel}>Personal information</p>
            <div className={styles.fields}>
              <Field label="Full name"  value={`${profile.firstName} ${profile.lastName}`} icon={User} />
              <Field label="Email"      value={profile.email}               icon={Mail} />
              <Field label="CPF"        value={formatDoc(profile.document)}  icon={Hash} />
            </div>
          </div>

          <div className={styles.infoCard}>
            <p className={styles.cardLabel}>Bank details</p>
            <div className={styles.fields}>
              <Field label="Bank code"      value={profile.paymentAccount?.bankCode}      icon={Landmark} />
              <Field label="Agency"         value={profile.paymentAccount?.agency}        icon={Landmark} />
              <Field label="Account number" value={profile.paymentAccount?.accountNumber} icon={CreditCard} />
              <Field label="Account type"   value={profile.paymentAccount?.accountType}   icon={CreditCard} />
            </div>
          </div>

        </div>
      </div>
    </div>
  )
}
