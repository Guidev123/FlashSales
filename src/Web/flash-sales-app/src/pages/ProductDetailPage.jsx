import { useState, useEffect, useRef } from 'react'
import { useAuth } from 'react-oidc-context'
import { useNavigate, useParams } from 'react-router-dom'
import {
  ChevronLeft, ImagePlus, Trash2, CheckCircle2,
  Archive, AlertCircle, Tag, Info,
} from 'lucide-react'
import { useApiFetch } from '../hooks/useApiFetch.js'
import Navbar from '../components/Navbar.jsx'
import Button from '../components/Button.jsx'
import styles from './ProductDetailPage.module.css'

const MAX_FILE_SIZE = 5 * 1024 * 1024
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']

const STATUS_LABEL = {
  None:    { label: 'Draft',    cls: 'statusDraft'    },
  Draft:   { label: 'Draft',    cls: 'statusDraft'    },
  Active:  { label: 'Active',   cls: 'statusActive'   },
  Archive: { label: 'Archived', cls: 'statusArchived' },
}

export default function ProductDetailPage() {
  const { id }   = useParams()
  const auth     = useAuth()
  const navigate = useNavigate()
  const apiFetch = useApiFetch()

  const coverRef  = useRef(null)
  const extraRef  = useRef(null)

  const [product,      setProduct]      = useState(null)
  const [loading,      setLoading]      = useState(true)
  const [fetchError,   setFetchError]   = useState('')
  const [uploading,    setUploading]    = useState(false)
  const [uploadError,  setUploadError]  = useState('')
  const [actionError,  setActionError]  = useState('')
  const [activating,   setActivating]   = useState(false)
  const [archiving,    setArchiving]    = useState(false)

  const authHeader = { Authorization: `Bearer ${auth.user.access_token}` }

  async function load() {
    setLoading(true)
    setFetchError('')
    try {
      const res = await apiFetch(
        `${import.meta.env.VITE_API_URL}/api/v1/products/${id}`,
        { headers: authHeader }
      )
      if (!res) return
      if (!res.ok) {
        setFetchError('Could not load product. Please try again.')
        return
      }
      setProduct(await res.json())
    } catch {
      setFetchError('Could not reach the server. Check your connection.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [id, auth.user?.access_token])

  async function uploadImage(file, isCover) {
    if (!ALLOWED_TYPES.includes(file.type)) {
      setUploadError('Only JPEG, PNG or WebP images are allowed.')
      return
    }
    if (file.size > MAX_FILE_SIZE) {
      setUploadError('File is too large. Maximum size is 5 MB.')
      return
    }

    setUploading(true)
    setUploadError('')

    try {
      const formData = new FormData()
      formData.append('file', file)

      const res = await apiFetch(
        `${import.meta.env.VITE_API_URL}/api/v1/products/images/${id}`,
        {
          method: 'POST',
          headers: {
            ...authHeader,
            order:   isCover ? '1' : String((product?.images?.length ?? 0) + 1),
            isCover: isCover ? 'true' : 'false',
          },
          body: formData,
        }
      )
      if (!res) return
      if (!res.ok) {
        const body = await res.json().catch(() => ({}))
        setUploadError(body.message || 'Upload failed. Please try again.')
        return
      }
      await load()
    } catch {
      setUploadError('Could not reach the server. Check your connection.')
    } finally {
      setUploading(false)
    }
  }

  async function handleActivate() {
    setActivating(true)
    setActionError('')
    try {
      const res = await apiFetch(
        `${import.meta.env.VITE_API_URL}/api/v1/products/${id}/activate`,
        { method: 'PUT', headers: authHeader }
      )
      if (!res) return
      if (!res.ok) {
        const body = await res.json().catch(() => ({}))
        setActionError(body.message || 'Could not activate product.')
        return
      }
      await load()
    } catch {
      setActionError('Could not reach the server.')
    } finally {
      setActivating(false)
    }
  }

  async function handleArchive() {
    setArchiving(true)
    setActionError('')
    try {
      const res = await apiFetch(
        `${import.meta.env.VITE_API_URL}/api/v1/products/${id}/archive`,
        { method: 'PUT', headers: authHeader }
      )
      if (!res) return
      if (!res.ok) {
        const body = await res.json().catch(() => ({}))
        setActionError(body.message || 'Could not archive product.')
        return
      }
      await load()
    } catch {
      setActionError('Could not reach the server.')
    } finally {
      setArchiving(false)
    }
  }

  if (loading) {
    return (
      <div className={styles.page}>
        <Navbar />
        <div className={styles.inner}>
          <div className={styles.skeletonBack} />
          <div className={styles.skeletonHeader} />
          <div className={styles.skeletonBody} />
        </div>
      </div>
    )
  }

  if (fetchError) {
    return (
      <div className={styles.page}>
        <Navbar />
        <div className={styles.inner}>
          <button className={styles.backLink} onClick={() => navigate('/seller/products')}>
            <ChevronLeft size={14} /> Back to products
          </button>
          <div className={styles.errorBox}>
            <AlertCircle size={18} /> {fetchError}
          </div>
        </div>
      </div>
    )
  }

  const cover  = product.images?.find(i => i.isCover)
  const extras = product.images?.filter(i => !i.isCover) ?? []
  const status = STATUS_LABEL[product.status] ?? STATUS_LABEL.Draft
  const isDraft    = !product.status || product.status === 'Draft' || product.status === 'None'
  const isArchived = product.status === 'Archive'

  return (
    <div className={styles.page}>
      <Navbar />

      <div className={styles.inner}>
        <button className={styles.backLink} onClick={() => navigate('/seller/products')}>
          <ChevronLeft size={14} /> Back to products
        </button>

        {/* Header */}
        <div className={styles.pageHeader}>
          <div className={styles.headerLeft}>
            <h1 className={styles.productName}>{product.name}</h1>
            <div className={styles.headerMeta}>
              {product.category && (
                <span className={styles.categoryTag}>
                  <Tag size={10} /> {product.category.categoryName}
                </span>
              )}
              <span className={`${styles.statusBadge} ${styles[status.cls]}`}>{status.label}</span>
            </div>
          </div>

          <div className={styles.headerActions}>
            {actionError && (
              <span className={styles.actionError}><AlertCircle size={13} /> {actionError}</span>
            )}
            {isDraft && (
              <Button
                variant="primary"
                size="sm"
                loading={activating}
                onClick={handleActivate}
              >
                <CheckCircle2 size={13} /> Activate
              </Button>
            )}
            {!isArchived && (
              <Button
                variant="outline"
                size="sm"
                loading={archiving}
                onClick={handleArchive}
              >
                <Archive size={13} /> Archive
              </Button>
            )}
          </div>
        </div>

        <div className={styles.grid}>

          {/* Images */}
          <div className={styles.card}>
            <p className={styles.cardLabel}>Cover image</p>

            <div className={styles.coverArea}>
              {cover ? (
                <img src={cover.url} alt="Cover" className={styles.coverImg} />
              ) : (
                <div className={styles.coverPlaceholder}>
                  <ImagePlus size={28} />
                  <span>No cover image</span>
                </div>
              )}
              <button
                className={styles.coverOverlay}
                onClick={() => !uploading && coverRef.current?.click()}
                disabled={uploading}
              >
                {uploading ? <span className={styles.spinner} /> : <ImagePlus size={16} />}
                <span>{cover ? 'Change cover' : 'Upload cover'}</span>
              </button>
            </div>

            <input
              ref={coverRef}
              type="file"
              accept="image/jpeg,image/png,image/webp"
              className={styles.fileInput}
              onChange={e => { const f = e.target.files?.[0]; e.target.value = ''; if (f) uploadImage(f, true) }}
            />

            {extras.length > 0 && (
              <>
                <p className={styles.cardLabel} style={{ marginTop: 8 }}>Additional images</p>
                <div className={styles.extrasGrid}>
                  {extras.map(img => (
                    <div key={img.imageId} className={styles.extraThumb}>
                      <img src={img.url} alt="" className={styles.thumbImg} />
                    </div>
                  ))}
                  <button
                    className={styles.addExtra}
                    onClick={() => !uploading && extraRef.current?.click()}
                    disabled={uploading}
                  >
                    <ImagePlus size={18} />
                  </button>
                </div>
              </>
            )}

            {extras.length === 0 && (
              <button
                className={styles.addExtraRow}
                onClick={() => !uploading && extraRef.current?.click()}
                disabled={uploading}
              >
                <ImagePlus size={14} /> Add more images
              </button>
            )}

            <input
              ref={extraRef}
              type="file"
              accept="image/jpeg,image/png,image/webp"
              className={styles.fileInput}
              onChange={e => { const f = e.target.files?.[0]; e.target.value = ''; if (f) uploadImage(f, false) }}
            />

            {uploadError && (
              <p className={styles.uploadError}><AlertCircle size={12} /> {uploadError}</p>
            )}
          </div>

          {/* Info */}
          <div className={styles.card}>
            <p className={styles.cardLabel}>Product information</p>

            <div className={styles.fields}>
              <div className={styles.fieldRow}>
                <span className={styles.fieldLabel}>Name</span>
                <span className={styles.fieldValue}>{product.name}</span>
              </div>
              <div className={styles.fieldRow}>
                <span className={styles.fieldLabel}>Category</span>
                <span className={styles.fieldValue}>{product.category?.categoryName ?? '—'}</span>
              </div>
              <div className={styles.fieldRow}>
                <span className={styles.fieldLabel}>Status</span>
                <span className={styles.fieldValue}>{status.label}</span>
              </div>
              <div className={styles.fieldRow}>
                <span className={styles.fieldLabel}>Images</span>
                <span className={styles.fieldValue}>{product.images?.length ?? 0}</span>
              </div>
            </div>

            <div className={styles.descSection}>
              <p className={styles.cardLabel}>Description</p>
              <p className={styles.descText}>{product.description}</p>
            </div>

            {isDraft && (
              <div className={styles.hintBox}>
                <Info size={13} />
                <span>Activate this product to make it available for launches.</span>
              </div>
            )}
          </div>

        </div>
      </div>
    </div>
  )
}
