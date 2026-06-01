import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useApiFetch } from '../hooks/useApiFetch.js'
import { ChevronLeft, Tag, ImageOff, AlertCircle } from 'lucide-react'
import Navbar from '../components/Navbar.jsx'
import styles from './ProductPage.module.css'

export default function ProductPage() {
  const { id }   = useParams()
  const navigate = useNavigate()
  const apiFetch = useApiFetch()

  const [product,    setProduct]    = useState(null)
  const [loading,    setLoading]    = useState(true)
  const [error,      setError]      = useState('')
  const [activeImg,  setActiveImg]  = useState(null)

  useEffect(() => {
    let cancelled = false
    async function load() {
      setLoading(true)
      setError('')
      try {
        const res = await apiFetch(`${import.meta.env.VITE_API_URL}/api/v1/products/${id}`)
        if (!res || cancelled) return
        if (!res.ok) { setError('Product not found.'); return }
        const data = await res.json()
        setProduct(data)
        const cover = data.images?.find(i => i.isCover) ?? data.images?.[0] ?? null
        setActiveImg(cover)
      } catch {
        if (!cancelled) setError('Could not reach the server. Check your connection.')
      } finally {
        if (!cancelled) setLoading(false)
      }
    }
    load()
    return () => { cancelled = true }
  }, [id])

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

  if (error) {
    return (
      <div className={styles.page}>
        <Navbar />
        <div className={styles.inner}>
          <button className={styles.backLink} onClick={() => navigate('/products')}>
            <ChevronLeft size={14} /> Back to products
          </button>
          <div className={styles.errorBox}><AlertCircle size={18} /> {error}</div>
        </div>
      </div>
    )
  }

  const images = product.images ?? []

  return (
    <div className={styles.page}>
      <Navbar />

      <div className={styles.inner}>
        <button className={styles.backLink} onClick={() => navigate('/products')}>
          <ChevronLeft size={14} /> Back to products
        </button>

        <div className={styles.layout}>

          {/* Gallery */}
          <div className={styles.gallery}>
            <div className={styles.mainImg}>
              {activeImg
                ? <img src={activeImg.url} alt={product.name} className={styles.mainImgEl} />
                : <div className={styles.imgPlaceholder}><ImageOff size={32} /></div>
              }
            </div>

            {images.length > 1 && (
              <div className={styles.thumbRow}>
                {images.map(img => (
                  <button
                    key={img.imageId}
                    className={`${styles.thumb} ${activeImg?.imageId === img.imageId ? styles.thumbActive : ''}`}
                    onClick={() => setActiveImg(img)}
                  >
                    <img src={img.url} alt="" className={styles.thumbImg} />
                  </button>
                ))}
              </div>
            )}
          </div>

          {/* Info */}
          <div className={styles.info}>
            {product.category && (
              <span className={styles.categoryTag}>
                <Tag size={11} /> {product.category.categoryName}
              </span>
            )}

            <h1 className={styles.productName}>{product.name}</h1>

            <p className={styles.description}>{product.description}</p>

            <div className={styles.launchNotice}>
              This product is available through launches. Browse active launches to purchase.
            </div>
          </div>

        </div>
      </div>
    </div>
  )
}
