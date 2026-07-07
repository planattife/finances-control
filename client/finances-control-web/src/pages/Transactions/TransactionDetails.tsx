import { Pencil, Trash2 } from 'lucide-react'
import ActionButton from '../../components/ActionButton'
import { useTransactionDetails } from '../../features/transactions/useTransactionDetails'
import { getPaymentMethodLabel, getTransactionTypeLabel } from '../../types/transaction'
import styles from './TransactionDetails.module.css'

interface TransactionDetailsProps {
  accountId: number
  isDeleting?: boolean
  onDeleteRequest?: (transactionId: number) => void
  onEdit?: (transactionId: number) => void
  transactionId: number
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

const currencyFormatter = new Intl.NumberFormat('pt-BR', {
  currency: 'BRL',
  style: 'currency',
})

function formatDate(value: string | null | undefined) {
  if (!value) {
    return '-'
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return '-'
  }

  return brDateFormatter.format(date)
}

function formatDateTime(value: string | null | undefined) {
  if (!value) {
    return '-'
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return '-'
  }

  return brDateTimeFormatter.format(date).replace(',', '')
}

function TransactionDetails({
  accountId,
  isDeleting = false,
  onDeleteRequest,
  onEdit,
  transactionId,
}: TransactionDetailsProps) {
  const {
    error: transactionDetailsError,
    errors: transactionDetailsErrors,
    isLoading: isTransactionDetailsLoading,
    transactionDetails,
  } = useTransactionDetails(accountId, transactionId)

  if (isTransactionDetailsLoading) {
    return <p className={styles.stateMessage}>Loading transaction details...</p>
  }

  if (transactionDetailsError || !transactionDetails) {
    return <p className={styles.stateMessage}>Could not load transaction details.</p>
  }

  return (
    <section
      aria-label="Transaction details"
      className={styles.details}
      data-transaction-id={transactionId}
    >
      {transactionDetailsErrors.length > 0 && (
        <p className={styles.stateMessage}>{transactionDetailsErrors.join('\n')}</p>
      )}
      <div aria-label="Transaction actions" className={styles.actions}>
        <ActionButton
          dangerHover
          disabled={isDeleting}
          icon={<Trash2 aria-hidden="true" />}
          onClick={() => onDeleteRequest?.(transactionId)}
        >
          Delete
        </ActionButton>
        <ActionButton icon={<Pencil aria-hidden="true" />} onClick={() => onEdit?.(transactionId)}>
          Edit
        </ActionButton>
      </div>
      <dl className={styles.infoGrid}>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Amount</dt>
          <dd className={styles.infoValue}>{currencyFormatter.format(transactionDetails.amount)}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Payment Method</dt>
          <dd className={styles.infoValue}>
            {getPaymentMethodLabel(transactionDetails.paymentMethod)}
          </dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Transaction Type</dt>
          <dd className={styles.infoValue}>
            {getTransactionTypeLabel(transactionDetails.transactionType)}
          </dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Transaction Date</dt>
          <dd className={styles.infoValue}>{formatDate(transactionDetails.transactionDate)}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Category</dt>
          <dd className={styles.infoValue}>{transactionDetails.category.categoryName}</dd>
        </div>
        {transactionDetails.bank && (
          <div className={styles.infoItem}>
            <dt className={styles.infoLabel}>Bank</dt>
            <dd className={styles.infoValue}>{transactionDetails.bank.bankName}</dd>
          </div>
        )}
        {transactionDetails.card && (
          <div className={styles.infoItem}>
            <dt className={styles.infoLabel}>Card</dt>
            <dd className={styles.infoValue}>{transactionDetails.card.cardName}</dd>
          </div>
        )}
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Created At</dt>
          <dd className={styles.infoValue}>{formatDateTime(transactionDetails.createdAt)}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Last Update</dt>
          <dd className={styles.infoValue}>{formatDateTime(transactionDetails.updatedAt)}</dd>
        </div>
        <div className={`${styles.infoItem} ${styles.descriptionItem}`}>
          <dt className={styles.infoLabel}>Description</dt>
          <dd className={styles.infoValue}>{transactionDetails.description || '-'}</dd>
        </div>
      </dl>
    </section>
  )
}

export default TransactionDetails
