import ActionButton from '../../components/ActionButton'
import UpdateCardForm, {
  type UpdateCardFormData,
} from '../../components/forms/UpdateCardForm/UpdateCardForm'
import Modal from '../../components/Modal'
import { useChangeCardStatus } from '../../features/cards/useChangeCardStatus'
import { useCardDetails } from '../../features/cards/useCardDetails'
import { useUpdateCard } from '../../features/cards/useUpdateCard'
import { showPromise } from '../../shared/notifications/toastService'
import { CardType, type CardType as CardTypeValue, type CreditCardDetails } from '../../types/card'
import styles from './CardsPage.module.css'

type UpdateCardModalProps = {
  accountId: number
  cardId: number
  cardType: CardTypeValue
  isOpen: boolean
  onClose: () => void
  onUpdated: () => Promise<void> | void
}

function getDayOfMonth(value: string | null | undefined) {
  if (!value) {
    return undefined
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return undefined
  }

  return date.getDate()
}

function isCreditCardDetails(
  cardDetails: ReturnType<typeof useCardDetails>['cardDetails'],
): cardDetails is CreditCardDetails {
  return cardDetails?.cardType === CardType.Credit
}

function UpdateCardModal({
  accountId,
  cardId,
  cardType,
  isOpen,
  onClose,
  onUpdated,
}: UpdateCardModalProps) {
  const {
    cardDetails,
    error: cardDetailsError,
    isLoading: isCardDetailsLoading,
    reload: reloadCardDetails,
  } = useCardDetails(accountId, cardId, cardType)
  const { changeCardStatus, isLoading: isChangingCardStatus } = useChangeCardStatus()
  const { updateCard, isLoading: isUpdating } = useUpdateCard()
  const isBusy = isCardDetailsLoading || isUpdating || isChangingCardStatus
  const isActive = cardDetails?.status.toLowerCase() === 'active'
  const nextStatus = isActive ? 'inactive' : 'active'
  const nextStatusActionLabel = nextStatus === 'active' ? 'Activating' : 'Inactivating'
  const nextStatusSuccessLabel = nextStatus === 'active' ? 'activated' : 'inactivated'
  const statusActionText = isChangingCardStatus
    ? `${isActive ? 'Inactivating' : 'Activating'}...`
    : isActive ? 'Inactivate' : 'Activate'

  async function handleSubmit(data: UpdateCardFormData) {
    if (isBusy) {
      return
    }

    try {
      await showPromise(
        updateCard(
          data.cardType === CardType.Credit
            ? {
                accountId,
                cardId,
                cardType: CardType.Credit,
                cardName: data.cardName,
                closingDay: Number(data.closingDay),
                dueDateDay: Number(data.dueDateDay),
              }
            : {
                accountId,
                cardId,
                cardType: CardType.Debit,
                cardName: data.cardName,
              },
        ),
        {
          loading: 'Updating card...',
          success: 'Card updated successfully',
          error: 'Could not update card',
        },
      )
      await onUpdated()
      onClose()
    } catch {
      // The promise toast displays the failure state and the modal remains open.
    }
  }

  function handleClose() {
    if (isUpdating || isChangingCardStatus) {
      return
    }

    onClose()
  }

  async function handleStatusChange() {
    if (isBusy) {
      return
    }

    try {
      await showPromise(changeCardStatus({ accountId, cardId, nextStatus }), {
        loading: `${nextStatusActionLabel} card...`,
        success: `Card ${nextStatusSuccessLabel} successfully`,
        error: 'Could not change card status',
      })
      await reloadCardDetails()
      await onUpdated()
    } catch {
      // The promise toast displays the failure state and the modal remains open.
    }
  }

  return (
    <Modal hideFooter isOpen={isOpen} onClose={handleClose} title="Edit Card">
      {isCardDetailsLoading && <p className={styles.stateCell}>Loading card details...</p>}

      {!isCardDetailsLoading && (cardDetailsError || !cardDetails) && (
        <p className={styles.stateCell}>Could not load card details.</p>
      )}

      {!isCardDetailsLoading && !cardDetailsError && cardDetails && (
        <UpdateCardForm
          cardType={cardType}
          defaultValues={{
            cardType,
            cardName: cardDetails.cardName ?? '',
            closingDay: isCreditCardDetails(cardDetails)
              ? getDayOfMonth(cardDetails.nextClosingDay)
              : undefined,
            dueDateDay: isCreditCardDetails(cardDetails)
              ? getDayOfMonth(cardDetails.nextDueDateDay)
              : undefined,
          }}
          isLoading={isBusy}
          onCancel={handleClose}
          onSubmit={handleSubmit}
          statusAction={(
            <ActionButton dangerHover={isActive} disabled={isBusy} onClick={handleStatusChange}>
              {statusActionText}
            </ActionButton>
          )}
        />
      )}
    </Modal>
  )
}

export default UpdateCardModal
