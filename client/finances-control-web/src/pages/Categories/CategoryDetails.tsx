import { Pencil } from 'lucide-react'
import ActionButton from '../../components/ActionButton'
import { useChangeCategoryStatus } from '../../features/categories/useChangeCategoryStatus'
import { useCategoryDetails } from '../../features/categories/useCategoryDetails'
import { showPromise } from '../../shared/notifications/toastService'
import styles from './CategoryDetails.module.css'

interface CategoryDetailsProps {
  categoryId: number
  onEdit?: (categoryId: number) => void
  onStatusChanged?: () => Promise<void> | void
}

const brDateFormatter = new Intl.DateTimeFormat('pt-BR', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
})

const brDateTimeFormatter = new Intl.DateTimeFormat('pt-BR', {
  day: '2-digit',
  hour: '2-digit',
  hour12: false,
  minute: '2-digit',
  month: '2-digit',
  year: 'numeric',
})

function formatDate(value: string) {
  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return '-'
  }

  return brDateFormatter.format(date)
}

function formatDateTime(value: string | null) {
  if (!value) {
    return '-'
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return '-'
  }

  return brDateTimeFormatter.format(date).replace(',', '')
}

function CategoryDetails({ categoryId, onEdit, onStatusChanged }: CategoryDetailsProps) {
  const {
    categoryDetails,
    error: categoryDetailsError,
    errors: categoryDetailsErrors,
    isLoading: isCategoryDetailsLoading,
    reload: reloadCategoryDetails,
  } = useCategoryDetails(categoryId)
  const { changeCategoryStatus, isLoading: isChangingCategoryStatus } = useChangeCategoryStatus()
  const isActive = categoryDetails?.status.toLowerCase() === 'active'
  const statusActionText = isActive ? 'Inactivate' : 'Activate'
  const nextStatus = isActive ? 'inactive' : 'active'
  const nextStatusActionLabel = nextStatus === 'active' ? 'Activating' : 'Inactivating'
  const nextStatusSuccessLabel = nextStatus === 'active' ? 'activated' : 'inactivated'
  const statusActionButtonText = isChangingCategoryStatus
    ? `${isActive ? 'Inactivating' : 'Activating'}...`
    : statusActionText

  async function handleStatusChange() {
    try {
      await showPromise(changeCategoryStatus({ categoryId, nextStatus }), {
        loading: `${nextStatusActionLabel} category...`,
        success: `Category ${nextStatusSuccessLabel} successfully`,
        error: 'Could not change category status',
      })
      await reloadCategoryDetails()
      await onStatusChanged?.()
    } catch {
      // The promise toast displays the failure state and the details remain unchanged.
    }
  }

  if (isCategoryDetailsLoading) {
    return <p className={styles.stateMessage}>Loading category details...</p>
  }

  if (categoryDetailsError || !categoryDetails) {
    return <p className={styles.stateMessage}>Could not load category details.</p>
  }

  return (
    <section aria-label="Category details" className={styles.details} data-category-id={categoryId}>
      {categoryDetailsErrors.length > 0 && (
        <p className={styles.stateMessage}>{categoryDetailsErrors.join('\n')}</p>
      )}
      <div aria-label="Category actions" className={styles.actions}>
        {categoryDetails.isDefault ? (
          <span className={styles.defaultBadge}>System Default</span>
        ) : (
          <>
            <ActionButton
              dangerHover={isActive}
              disabled={isChangingCategoryStatus}
              onClick={handleStatusChange}
            >
              {statusActionButtonText}
            </ActionButton>
            <ActionButton icon={<Pencil aria-hidden="true" />} onClick={() => onEdit?.(categoryId)}>
              Edit
            </ActionButton>
          </>
        )}
      </div>
      <dl className={styles.infoGrid}>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Name</dt>
          <dd className={styles.infoValue}>{categoryDetails.name}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Status</dt>
          <span className={`${styles.statusBadge} ${isActive ? styles.statusActive : styles.statusInactive}`}>
            {isActive ? 'Active' : 'Inactive'}
          </span>
        </div>
        {!categoryDetails.isDefault && (
          <>
            <div className={styles.infoItem}>
              <dt className={styles.infoLabel}>Created At</dt>
              <dd className={styles.infoValue}>{formatDate(categoryDetails.createdAt)}</dd>
            </div>
            <div className={styles.infoItem}>
              <dt className={styles.infoLabel}>Last Update</dt>
              <dd className={styles.infoValue}>{formatDateTime(categoryDetails.lastUpdate)}</dd>
            </div>
          </>
        )}
      </dl>
    </section>
  )
}

export default CategoryDetails
