import { useState, useEffect } from 'react'
import { useAuth } from 'react-oidc-context'
import { useNavigate } from 'react-router-dom'
import { ChevronLeft, Rocket, AlertCircle, ArrowRight } from 'lucide-react'
import { useApiFetch } from '../hooks/useApiFetch.js'
import Navbar from '../components/Navbar.jsx'
import Input from '../components/Input.jsx'
import Button from '../components/Button.jsx'
import styles from './CreateLaunchPage.module.css'

function validate(v) {
  const e = {}
  if (!v.productId)          e.productId    = 'Select a product'
  if (!v.title?.trim())      e.title        = 'Launch title is required'
  if (!v.description?.trim()) e.description = 'Description is required'
  return e
}

export default function CreateLaunchPage() {
  const auth     = useAuth()
  const navigate = useNavigate()
  const apiFetch = useApiFetch()

  const [values,       setValues]       = useState({ productId: '', title: '', description: '' })
  const [errors,       setErrors]       = useState({})
  const [touched,      setTouched]      = useState({})
  const [loading,      setLoading]      = useState(false)
  const [apiError,     setApiError]     = useState('')
  const [products,     setProducts]     = useState([])
  const [prodLoading,  setProdLoading]  = useState(true)

  useEffect(() => {
    async function loadProducts() {
      try {
        const res = await apiFetch(
          `${import.meta.env.VITE_API_URL}/api/v1/products/mine?page=1&size=100`,
          { headers: { Authorization: `Bearer ${auth.user.access_token}` } }
        )
        if (!res || !res.ok) return
        const data = await res.json()
        setProducts((data.items ?? []).filter(p => p.status === 'Active'))
      } catch {
        // non-critical — form will show empty select
      } finally {
        setProdLoading(false)
      }
    }
    loadProducts()
  }, [auth.user?.access_token])

  const set = (field) => (e) => {
    const val = e.target.value
    setValues(v => ({ ...v, [field]: val }))
    if (touched[field]) {
      const errs = validate({ ...values, [field]: val })
      setErrors(prev => ({ ...prev, [field]: errs[field] }))
    }
  }

  const blur = (field) => () => {
    setTouched(v => ({ ...v, [field]: true }))
    setErrors(prev => ({ ...prev, [field]: validate(values)[field] }))
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
      const res = await apiFetch(`${import.meta.env.VITE_API_URL}/api/v1/launches`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${auth.user.access_token}`,
        },
        body: JSON.stringify({
          productId:   values.productId,
          title:       values.title.trim(),
          description: values.description.trim(),
        }),
      })
      if (!res) return
      if (!res.ok) {
        const body = await res.json().catch(() => ({}))
        setApiError(body.detail || body.message || 'Could not create launch. Please try again.')
        return
      }
      const { id } = await res.json()
      navigate(`/seller/launches/${id}`)
    } catch {
      setApiError('Could not reach the server. Check your connection.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className={styles.page}>
      <Navbar />

      <div className={styles.inner}>
        <button className={styles.backLink} onClick={() => navigate('/seller/launches')}>
          <ChevronLeft size={14} /> Back to launches
        </button>

        <div className={styles.card}>
          <div className={styles.cardHeader}>
            <div className={styles.iconWrap}><Rocket size={20} /></div>
            <div>
              <h2 className={styles.title}>New launch</h2>
              <p className={styles.sub}>Create a launch event for one of your active products</p>
            </div>
          </div>

          <form onSubmit={handleSubmit} noValidate className={styles.form}>
            {apiError && (
              <div className={styles.apiError}>
                <AlertCircle size={14} /> {apiError}
              </div>
            )}

            <div className={styles.fieldGroup}>
              <label className={styles.label}>
                Product
                {errors.productId && touched.productId && (
                  <span className={styles.fieldError}>{errors.productId}</span>
                )}
              </label>
              <select
                className={`${styles.select} ${errors.productId && touched.productId ? styles.selectError : ''}`}
                value={values.productId}
                onChange={set('productId')}
                onBlur={blur('productId')}
                disabled={prodLoading}
              >
                <option value="">
                  {prodLoading ? 'Loading products…' : products.length === 0 ? 'No active products available' : 'Select a product'}
                </option>
                {products.map(p => (
                  <option key={p.id} value={p.id}>{p.name}</option>
                ))}
              </select>
              {!prodLoading && products.length === 0 && (
                <p className={styles.prodHint}>
                  You need at least one active product to create a launch.{' '}
                  <button type="button" className={styles.hintLink} onClick={() => navigate('/seller/products')}>
                    Manage products
                  </button>
                </p>
              )}
            </div>

            <Input
              label="Launch title"
              name="title"
              placeholder="e.g. Advanced React Masterclass — Cohort 3"
              value={values.title}
              onChange={set('title')}
              onBlur={blur('title')}
              error={errors.title}
            />

            <div className={styles.fieldGroup}>
              <label className={styles.label}>
                Description
                {errors.description && touched.description && (
                  <span className={styles.fieldError}>{errors.description}</span>
                )}
              </label>
              <textarea
                className={`${styles.textarea} ${errors.description && touched.description ? styles.textareaError : ''}`}
                placeholder="What does this launch offer? Who is it for? What makes it special?"
                rows={5}
                value={values.description}
                onChange={set('description')}
                onBlur={blur('description')}
              />
            </div>

            <Button type="submit" variant="primary" size="md" loading={loading}>
              Create launch <ArrowRight size={14} />
            </Button>
          </form>
        </div>
      </div>
    </div>
  )
}
