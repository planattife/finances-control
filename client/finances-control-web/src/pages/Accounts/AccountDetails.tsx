import KpiCard from '../../components/KpiCard'
import { useAccountDetails } from '../../features/accounts/useAccountDetails'
import { useAccountKpi } from '../../features/accounts/useAccountKpi'
import styles from './AccountDetails.module.css'

interface AccountDetailsProps {
  accountId: number
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

function AccountDetails({ accountId }: AccountDetailsProps) {
  const {
    accountDetails,
    error: accountDetailsError,
    errors: accountDetailsErrors,
    isLoading: isAccountDetailsLoading,
  } = useAccountDetails(accountId)
  const {
    accountKpi,
    error: accountKpiError,
    errors: accountKpiErrors,
    isLoading: isAccountKpiLoading,
  } = useAccountKpi(accountId)
  const errors = [...accountKpiErrors, ...accountDetailsErrors]
  const isLoading = isAccountKpiLoading || isAccountDetailsLoading
  const isActive = accountDetails?.status.toLowerCase() === 'active'

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
