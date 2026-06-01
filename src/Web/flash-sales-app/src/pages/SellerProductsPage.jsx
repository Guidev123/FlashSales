import { useState, useEffect } from 'react'
import { useAuth } from 'react-oidc-context'
import { useNavigate } from 'react-router-dom'
import { Plus, Package, AlertCircle, ImageOff, Tag } from 'lucide-react'
import { useApiFetch } from '../hooks/useApiFetch.js'
import Navbar from '../components/Navbar.jsx'
import Button from '../components/Button.jsx'
import styles from './SellerProductsPage.module.css'

const STATUS_LABEL = {
  None:    { label: 'Draft',    cls: 'statusDraft'    },
  Draft:   { label: 'Draft',    cls: 'statusDraft'    },
  Active:  { label: 'Active',   cls: 'statusActive'   },
  Archive: { label: 'Archived', cls: 'statusArchived' },
}

function ProductCard({ product, onClick }) {
  const cover = product.images?.find(i => i.isCover) ?? product.images?.[0]
  const status = STATUS_LABEL[product.status] ?? STATUS_LABEL.Draft

  return (
    <button className={styles.card} onClick={onClick}>
      <div className={styles.cardImg}>
        {cover ? (
          <img src={cover.url} alt={product.name} className={styles.coverImg} />
        ) : (
          <div className={styles.coverPlaceholder}>
            <ImageOff size={24} />
          </div>
        )}
        <span className={`${styles.statusBadge} ${styles[status.cls]}`}>{status.label}</span>
      </div>

      <div className={styles.cardBody}>
        <p className={styles.productName}>{product.name}</p>
        {product.category && (
          <span className={styles.categoryTag}>
            <Tag size={10} />
            {product.category.categoryName}
          </span>
        )}
        <p className={styles.productDesc}>{product.description}</p>
      </div>
    </button>
  )
}

export default function SellerProductsPage() {
  const auth      = useAuth()
  const navigate  = useNavigate()
  const apiFetch  = useApiFetch()

  const [products, setProducts] = useState([])
  const [loading,  setLoading]  = useState(true)
  const [error,    setError]    = useState('')
  const [page,     setPage]     = useState(1)
  const [meta,     setMeta]     = useState(null)

  useEffect(() => {
    let cancelled = false

    async function load() {
      setLoading(true)
      setError('')
      try {
        const res = await apiFetch(
          `${import.meta.env.VITE_API_URL}/api/v1/products/mine?page=${page}&size=12`,
          { headers: { Authorization: `Bearer ${auth.user.access_token}` } }
        )
        if (!res || cancelled) return
        if (!res.ok) {
          setError('Could not load your products. Please try again.')
          return
        }
        const data = await res.json()
        setProducts(data.items ?? [])
        setMeta(data)
      } catch {
        if (!cancelled) setError('Could not reach the server. Check your connection.')
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    load()
    return () => { cancelled = true }
  }, [auth.user?.access_token, page])

  return (
    <div className={styles.page}>
      <Navbar />

      <div className={styles.inner}>
        <div className={styles.pageHeader}>
          <div>
            <h1 className={styles.pageTitle}>My products</h1>
            <p className={styles.pageSub}>Manage your digital product catalog</p>
          </div>
          <Button variant="primary" size="sm" onClick={() => navigate('/seller/products/new')}>
            <Plus size={14} />
            New product
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
            <AlertCircle size={18} />
            {error}
          </div>
        )}

        {!loading && !error && products.length === 0 && (
          <div className={styles.emptyState}>
            <Package size={40} strokeWidth={1.2} />
            <p className={styles.emptyTitle}>No products yet</p>
            <p className={styles.emptySub}>Create your first digital product to start selling.</p>
            <Button variant="primary" size="sm" onClick={() => navigate('/seller/products/new')}>
              <Plus size={14} />
              Create product
            </Button>
          </div>
        )}

        {!loading && !error && products.length > 0 && (
          <>
            <div className={styles.grid}>
              {products.map(p => (
                <ProductCard
                  key={p.id}
                  product={p}
                  onClick={() => navigate(`/seller/products/${p.id}`)}
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
                <span className={styles.pageInfo}>
                  Page {meta.page} of {meta.totalPages}
                </span>
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
