import { Pencil } from 'lucide-react'
import ActionButton from '../../components/ActionButton'
import { useCardDetails } from '../../features/cards/useCardDetails'
import {
  CardType,
  getCardNetworkLabel,
  getCardTypeLabel,
  type CardType as CardTypeValue,
  type CreditCardDetails,
} from '../../types/card'
import styles from './CardDetails.module.css'

interface CardDetailsProps {
  accountId: number
  cardId: number
  cardType: CardTypeValue
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

function isCreditCardDetails(
  cardDetails: ReturnType<typeof useCardDetails>['cardDetails'],
): cardDetails is CreditCardDetails {
  return cardDetails?.cardType === CardType.Credit
}

function CardDetails({ accountId, cardId, cardType }: CardDetailsProps) {
  const {
    cardDetails,
    error: cardDetailsError,
    errors: cardDetailsErrors,
    isLoading: isCardDetailsLoading,
  } = useCardDetails(accountId, cardId, cardType)
  const isActive = cardDetails?.status.toLowerCase() === 'active'
  const statusActionText = isActive ? 'Inactivate' : 'Activate'

  if (isCardDetailsLoading) {
    return <p className={styles.stateMessage}>Loading card details...</p>
  }

  if (cardDetailsError || !cardDetails) {
    return <p className={styles.stateMessage}>Could not load card details.</p>
  }

  return (
    <section aria-label="Card details" className={styles.details} data-card-id={cardId}>
      {cardDetailsErrors.length > 0 && (
        <p className={styles.stateMessage}>{cardDetailsErrors.join('\n')}</p>
      )}
      <div aria-label="Card actions" className={styles.actions}>
        <ActionButton dangerHover={isActive} disabled>
          {statusActionText}
        </ActionButton>
        <ActionButton disabled icon={<Pencil aria-hidden="true" />}>
          Edit
        </ActionButton>
      </div>
      <dl className={styles.infoGrid}>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Card Name</dt>
          <dd className={styles.infoValue}>{cardDetails.cardName ?? 'Unnamed card'}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Card Number</dt>
          <dd className={styles.infoValue}>{cardDetails.cardNumber}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Type</dt>
          <dd className={styles.infoValue}>{getCardTypeLabel(cardDetails.cardType)}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Network</dt>
          <dd className={styles.infoValue}>{getCardNetworkLabel(cardDetails.cardNetwork)}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Expiration Date</dt>
          <dd className={styles.infoValue}>{formatDate(cardDetails.expirationDate)}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Bank</dt>
          <dd className={styles.infoValue}>{cardDetails.bank.name}</dd>
        </div>
        {isCreditCardDetails(cardDetails) && (
          <>
            <div className={styles.infoItem}>
              <dt className={styles.infoLabel}>Next Closing Day</dt>
              <dd className={styles.infoValue}>{formatDate(cardDetails.nextClosingDay)}</dd>
            </div>
            <div className={styles.infoItem}>
              <dt className={styles.infoLabel}>Next Due Date Day</dt>
              <dd className={styles.infoValue}>{formatDate(cardDetails.nextDueDateDay)}</dd>
            </div>
          </>
        )}
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Created At</dt>
          <dd className={styles.infoValue}>{formatDateTime(cardDetails.createdAt)}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Last Update</dt>
          <dd className={styles.infoValue}>{formatDateTime(cardDetails.lastUpdate)}</dd>
        </div>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Status</dt>
          <span
            className={`${styles.statusBadge} ${
              isActive ? styles.statusActive : styles.statusInactive
            }`}
          >
            {isActive ? 'Active' : 'Inactive'}
          </span>
        </div>
      </dl>
    </section>
  )
}

export default CardDetails
