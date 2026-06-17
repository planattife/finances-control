import KpiCard from '../../components/KpiCard'
import { useAccountKpi } from '../../features/accounts/useAccountKpi'
import styles from './AccountDetails.module.css'

interface AccountDetailsProps {
  accountId: number
}

function AccountDetails({ accountId }: AccountDetailsProps) {
  const { accountKpi, error, errors, isLoading } = useAccountKpi(accountId)

  if (isLoading) {
    return <p className={styles.stateMessage}>Loading account details...</p>
  }

  if (error || !accountKpi) {
    return <p className={styles.stateMessage}>Could not load account details.</p>
  }

  return (
    <section aria-label="Account KPIs" className={styles.kpiSection} data-account-id={accountId}>
      {errors.length > 0 && <p className={styles.stateMessage}>{errors.join('\n')}</p>}
      <KpiCard label="CARDS" value={accountKpi.cardsCount} />
      <KpiCard label="TRANSACTIONS" value={accountKpi.transactionsCount} />
    </section>
  )
}

export default AccountDetails
