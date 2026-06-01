import { useState, useEffect } from 'react'
import { useAuth } from 'react-oidc-context'
import { useNavigate } from 'react-router-dom'
import { ChevronLeft, ArrowRight, Package, AlertCircle } from 'lucide-react'
import { useApiFetch } from '../hooks/useApiFetch.js'
import Navbar from '../components/Navbar.jsx'
import Input from '../components/Input.jsx'
import Button from '../components/Button.jsx'
import styles from './CreateProductPage.module.css'

function validate(v) {
  const e = {}
  if (!v.name?.trim())        e.name        = 'Product name is required'
  if (!v.description?.trim()) e.description = 'Description is required'
  if (!v.categoryId)          e.categoryId  = 'Select a category'
  return e
}

export default function CreateProductPage() {
  const auth     = useAuth()
  const navigate = useNavigate()
  const apiFetch = useApiFetch()

  const [values,      setValues]      = useState({ name: '', description: '', categoryId: '' })
  const [errors,      setErrors]      = useState({})
  const [touched,     setTouched]     = useState({})
  const [loading,     setLoading]     = useState(false)
  const [apiError,    setApiError]    = useState('')
  const [categories,  setCategories]  = useState([])
  const [catLoading,  setCatLoading]  = useState(true)

  useEffect(() => {
    async function loadCategories() {
      try {
        const res = await apiFetch(
          `${import.meta.env.VITE_API_URL}/api/v1/products/categories?page=1&size=100`
        )
        if (!res || !res.ok) return
        const data = await res.json()
        setCategories(data.items ?? [])
      } catch {
        // non-critical — form will show empty select
      } finally {
        setCatLoading(false)
      }
    }
    loadCategories()
  }, [])

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
      const res = await apiFetch(`${import.meta.env.VITE_API_URL}/api/v1/products`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${auth.user.access_token}`,
        },
        body: JSON.stringify({
          name:        values.name.trim(),
          description: values.description.trim(),
          categoryId:  values.categoryId,
        }),
      })
      if (!res) return
      if (!res.ok) {
        const body = await res.json().catch(() => ({}))
        setApiError(body.message || 'Could not create product. Please try again.')
        return
      }
      const { productId } = await res.json()
      navigate(`/seller/products/${productId}`)
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
        <button className={styles.backLink} onClick={() => navigate('/seller/products')}>
          <ChevronLeft size={14} /> Back to products
        </button>

        <div className={styles.card}>
          <div className={styles.cardHeader}>
            <div className={styles.iconWrap}><Package size={20} /></div>
            <div>
              <h2 className={styles.title}>New product</h2>
              <p className={styles.sub}>Add a digital product to your catalog</p>
            </div>
          </div>

          <form onSubmit={handleSubmit} noValidate className={styles.form}>
            {apiError && (
              <div className={styles.apiError}>
                <AlertCircle size={14} /> {apiError}
              </div>
            )}

            <Input
              label="Product name"
              name="name"
              placeholder="e.g. Advanced React Course"
              value={values.name}
              onChange={set('name')}
              onBlur={blur('name')}
              error={errors.name}
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
                name="description"
                placeholder="Describe your product — what it covers, who it's for, what buyers get…"
                rows={5}
                value={values.description}
                onChange={set('description')}
                onBlur={blur('description')}
              />
            </div>

            <div className={styles.fieldGroup}>
              <label className={styles.label}>
                Category
                {errors.categoryId && touched.categoryId && (
                  <span className={styles.fieldError}>{errors.categoryId}</span>
                )}
              </label>
              <select
                className={`${styles.select} ${errors.categoryId && touched.categoryId ? styles.selectError : ''}`}
                name="categoryId"
                value={values.categoryId}
                onChange={set('categoryId')}
                onBlur={blur('categoryId')}
                disabled={catLoading}
              >
                <option value="">{catLoading ? 'Loading categories…' : 'Select a category'}</option>
                {categories.map(c => (
                  <option key={c.categoryId} value={c.categoryId}>{c.categoryName}</option>
                ))}
              </select>
            </div>

            <Button type="submit" variant="primary" size="md" loading={loading}>
              Create product <ArrowRight size={14} />
            </Button>
          </form>
        </div>
      </div>
    </div>
  )
}
