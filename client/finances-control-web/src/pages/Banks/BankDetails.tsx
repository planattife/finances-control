import { Pencil } from 'lucide-react'
import ActionButton from '../../components/ActionButton'
import { useBankDetails } from '../../features/banks/useBankDetails'
import { useChangeBankStatus } from '../../features/banks/useChangeBankStatus'
import { showPromise } from '../../shared/notifications/toastService'
import { getBankAccountTypeLabel } from '../../types/bank'
import styles from './BankDetails.module.css'

interface BankDetailsProps {
  bankId: number
  onEdit?: (bankId: number) => void
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

function BankDetails({ bankId, onEdit, onStatusChanged }: BankDetailsProps) {
  const {
    bankDetails,
    error: bankDetailsError,
    errors: bankDetailsErrors,
    isLoading: isBankDetailsLoading,
    reload: reloadBankDetails,
  } = useBankDetails(bankId)
  const { changeBankStatus, isLoading: isChangingBankStatus } = useChangeBankStatus()
  const isActive = bankDetails?.status.toLowerCase() === 'active'
  const nextStatus = isActive ? 'inactive' : 'active'
  const nextStatusActionLabel = nextStatus === 'active' ? 'Activating' : 'Inactivating'
  const nextStatusSuccessLabel = nextStatus === 'active' ? 'activated' : 'inactivated'
  const statusActionText = isChangingBankStatus
    ? `${isActive ? 'Inactivating' : 'Activating'}...`
    : isActive ? 'Inactivate' : 'Activate'

  async function handleStatusChange() {
    try {
      await showPromise(changeBankStatus({ bankId, nextStatus }), {
        loading: `${nextStatusActionLabel} bank...`,
        success: `Bank ${nextStatusSuccessLabel} successfully`,
        error: 'Could not change bank status',
      })
      await reloadBankDetails()
      await onStatusChanged?.()
    } catch {
      // The promise toast displays the failure state and the details remain unchanged.
    }
  }

  if (isBankDetailsLoading) {
    return <p className={styles.stateMessage}>Loading bank details...</p>
  }

  if (bankDetailsError || !bankDetails) {
    return <p className={styles.stateMessage}>Could not load bank details.</p>
  }

  return (
    <section aria-label="Bank details" className={styles.details} data-bank-id={bankId}>
      {bankDetailsErrors.length > 0 && (
        <p className={styles.stateMessage}>{bankDetailsErrors.join('\n')}</p>
      )}
      <div aria-label="Bank actions" className={styles.actions}>
        <ActionButton
          dangerHover={isActive}
          disabled={isChangingBankStatus}
          onClick={handleStatusChange}
        >
          {statusActionText}
        </ActionButton>
        <ActionButton icon={<Pencil aria-hidden="true" />} onClick={() => onEdit?.(bankId)}>
          Edit
        </ActionButton>
      </div>
      <dl className={styles.infoGrid}>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Name</dt>
          <dd className={styles.infoValue}>{bankDetails.name}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Account Type</dt>
          <dd className={styles.infoValue}>{getBankAccountTypeLabel(bankDetails.accountType)}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Status</dt>
          <span className={`${styles.statusBadge} ${isActive ? styles.statusActive : styles.statusInactive}`}>
            {isActive ? 'Active' : 'Inactive'}
          </span>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Created At</dt>
          <dd className={styles.infoValue}>{formatDate(bankDetails.createdAt)}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Last Update</dt>
          <dd className={styles.infoValue}>{formatDateTime(bankDetails.lastUpdate)}</dd>
        </div>
      </dl>
    </section>
  )
}

export default BankDetails
