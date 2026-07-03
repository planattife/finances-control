import { Pencil } from 'lucide-react'
import ActionButton from '../../components/ActionButton'
import { useChangeCardStatus } from '../../features/cards/useChangeCardStatus'
import { useCardDetails } from '../../features/cards/useCardDetails'
import { showPromise } from '../../shared/notifications/toastService'
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
  onEdit?: (cardId: number, cardType: CardTypeValue) => void
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

function CardDetails({ accountId, cardId, cardType, onEdit, onStatusChanged }: CardDetailsProps) {
  const {
    cardDetails,
    error: cardDetailsError,
    errors: cardDetailsErrors,
    isLoading: isCardDetailsLoading,
    reload: reloadCardDetails,
  } = useCardDetails(accountId, cardId, cardType)
  const { changeCardStatus, isLoading: isChangingCardStatus } = useChangeCardStatus()
  const isActive = cardDetails?.status.toLowerCase() === 'active'
  const nextStatus = isActive ? 'inactive' : 'active'
  const nextStatusActionLabel = nextStatus === 'active' ? 'Activating' : 'Inactivating'
  const nextStatusSuccessLabel = nextStatus === 'active' ? 'activated' : 'inactivated'
  const statusActionText = isChangingCardStatus
    ? `${isActive ? 'Inactivating' : 'Activating'}...`
    : isActive ? 'Inactivate' : 'Activate'

  async function handleStatusChange() {
    try {
      await showPromise(changeCardStatus({ accountId, cardId, nextStatus }), {
        loading: `${nextStatusActionLabel} card...`,
        success: `Card ${nextStatusSuccessLabel} successfully`,
        error: 'Could not change card status',
      })
      await reloadCardDetails()
      await onStatusChanged?.()
    } catch {
      // The promise toast displays the failure state and the details remain unchanged.
    }
  }

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
        <ActionButton
          dangerHover={isActive}
          disabled={isChangingCardStatus}
          onClick={handleStatusChange}
        >
          {statusActionText}
        </ActionButton>
        <ActionButton icon={<Pencil aria-hidden="true" />} onClick={() => onEdit?.(cardId, cardType)}>
          Edit
        </ActionButton>
      </div>
      <dl className={styles.infoGrid}>
        <div className={styles.infoItem}>
          <dt className={styles.infoLabel}>Card Name</dt>
          <dd className={styles.infoValue}>{cardDetails.cardName ?? 'Unnamed card'}</dd>
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
      </dl>
    </section>
  )
}

export default CardDetails
