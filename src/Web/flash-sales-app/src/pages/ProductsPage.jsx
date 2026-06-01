import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useApiFetch } from '../hooks/useApiFetch.js'
import { ImageOff, Tag, AlertCircle } from 'lucide-react'
import Navbar from '../components/Navbar.jsx'
import Button from '../components/Button.jsx'
import styles from './ProductsPage.module.css'

function ProductCard({ product, onClick }) {
  const cover = product.images?.find(i => i.isCover) ?? product.images?.[0]

  return (
    <button className={styles.card} onClick={onClick}>
      <div className={styles.cardImg}>
        {cover
          ? <img src={cover.url} alt={product.name} className={styles.coverImg} />
          : <div className={styles.coverPlaceholder}><ImageOff size={22} /></div>
        }
      </div>
      <div className={styles.cardBody}>
        {product.category && (
          <span className={styles.categoryTag}>
            <Tag size={10} /> {product.category.categoryName}
          </span>
        )}
        <p className={styles.productName}>{product.name}</p>
        <p className={styles.productDesc}>{product.description}</p>
      </div>
    </button>
  )
}

export default function ProductsPage() {
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
          `${import.meta.env.VITE_API_URL}/api/v1/products?page=${page}&size=12`
        )
        if (!res || cancelled) return
        if (!res.ok) { setError('Could not load products. Please try again.'); return }
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
  }, [page])

  return (
    <div className={styles.page}>
      <Navbar />

      <div className={styles.inner}>
        <div className={styles.pageHeader}>
          <div>
            <h1 className={styles.pageTitle}>Products</h1>
            <p className={styles.pageSub}>Browse all available digital products</p>
          </div>
          {meta && (
            <span className={styles.totalCount}>{meta.totalCount} products</span>
          )}
        </div>

        {loading && (
          <div className={styles.grid}>
            {Array.from({ length: 12 }).map((_, i) => (
              <div key={i} className={styles.skeletonCard} />
            ))}
          </div>
        )}

        {!loading && error && (
          <div className={styles.errorBox}>
            <AlertCircle size={18} /> {error}
          </div>
        )}

        {!loading && !error && products.length === 0 && (
          <div className={styles.emptyState}>
            <p className={styles.emptyTitle}>No products available yet</p>
            <p className={styles.emptySub}>Check back soon — sellers are adding products.</p>
          </div>
        )}

        {!loading && !error && products.length > 0 && (
          <>
            <div className={styles.grid}>
              {products.map(p => (
                <ProductCard
                  key={p.id}
                  product={p}
                  onClick={() => navigate(`/products/${p.id}`)}
                />
              ))}
            </div>

            {meta && (meta.hasNextPage || meta.hasPreviousPage) && (
              <div className={styles.pagination}>
                <Button variant="outline" size="sm" disabled={!meta.hasPreviousPage} onClick={() => setPage(p => p - 1)}>
                  Previous
                </Button>
                <span className={styles.pageInfo}>Page {meta.page} of {meta.totalPages}</span>
                <Button variant="outline" size="sm" disabled={!meta.hasNextPage} onClick={() => setPage(p => p + 1)}>
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
