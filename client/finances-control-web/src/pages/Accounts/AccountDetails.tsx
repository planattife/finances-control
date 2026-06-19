import { Pencil } from 'lucide-react'
import ActionButton from '../../components/ActionButton'
import KpiCard from '../../components/KpiCard'
import { useAccountDetails } from '../../features/accounts/useAccountDetails'
import { useAccountKpi } from '../../features/accounts/useAccountKpi'
import { useChangeAccountStatus } from '../../features/accounts/useChangeAccountStatus'
import styles from './AccountDetails.module.css'

interface AccountDetailsProps {
  accountId: number
  onEdit?: (accountId: number) => void
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

function formatDateTime(value: string) {
  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return '-'
  }

  return brDateTimeFormatter.format(date).replace(',', '')
}

function AccountDetails({ accountId, onEdit, onStatusChanged }: AccountDetailsProps) {
  const {
    accountDetails,
    error: accountDetailsError,
    errors: accountDetailsErrors,
    isLoading: isAccountDetailsLoading,
    reload: reloadAccountDetails,
  } = useAccountDetails(accountId)
  const {
    accountKpi,
    error: accountKpiError,
    errors: accountKpiErrors,
    isLoading: isAccountKpiLoading,
  } = useAccountKpi(accountId)
  const { changeAccountStatus, isLoading: isChangingAccountStatus } = useChangeAccountStatus()
  const errors = [...accountKpiErrors, ...accountDetailsErrors]
  const isLoading = isAccountKpiLoading || isAccountDetailsLoading
  const isActive = accountDetails?.status.toLowerCase() === 'active'
  const nextStatus = isActive ? 'inactive' : 'active'
  const nextStatusLabel = nextStatus === 'active' ? 'Active' : 'Inactive'
  const statusActionText = isChangingAccountStatus
    ? `${isActive ? 'Inactivating' : 'Activating'}...`
    : isActive ? 'Inactivate' : 'Activate'

  async function handleStatusChange() {
    await changeAccountStatus({ accountId, nextStatus })
    await reloadAccountDetails()
    await onStatusChanged?.()
    alert(`The Account Status was changed to ${nextStatusLabel}`)
  }

  if (isLoading) {
    return <p className={styles.stateMessage}>Loading account details...</p>
  }

  if (accountKpiError || accountDetailsError || !accountKpi || !accountDetails) {
    return <p className={styles.stateMessage}>Could not load account details.</p>
  }

  return (
    <section aria-label="Account details" className={styles.details} data-account-id={accountId}>
      {errors.length > 0 && <p className={styles.stateMessage}>{errors.join('\n')}</p>}
      <div aria-label="Account KPIs" className={styles.kpiSection}>
        <KpiCard label="CARDS" value={accountKpi.cardsCount} />
        <KpiCard label="TRANSACTIONS" value={accountKpi.transactionsCount} />
      </div>
      <div aria-label="Account actions" className={styles.actions}>
        <ActionButton
          dangerHover={isActive}
          disabled={isChangingAccountStatus}
          onClick={handleStatusChange}
        >
          {statusActionText}
        </ActionButton>
        <ActionButton icon={<Pencil aria-hidden="true" />} onClick={() => onEdit?.(accountId)}>
          Edit
        </ActionButton>
      </div>
      <dl className={styles.infoGrid}>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Name</dt>
          <dd className={styles.infoValue}>{accountDetails.name}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Status</dt>
          <span className={`${styles.statusBadge} ${isActive ? styles.statusActive : styles.statusInactive}`}>
            {isActive ? 'Active' : 'Inactive'}
          </span>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Created At</dt>
          <dd className={styles.infoValue}>{formatDate(accountDetails.createdAt)}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Last Update</dt>
          <dd className={styles.infoValue}>{formatDateTime(accountDetails.lastUpdate)}</dd>
        </div>
      </dl>
    </section>
  )
}

export default AccountDetails
