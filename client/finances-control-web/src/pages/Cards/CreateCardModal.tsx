import CreateCardForm, {
  type CreateCardFormData,
} from '../../components/forms/CreateCardForm/CreateCardForm'
import Modal from '../../components/Modal'
import { useCreateCard } from '../../features/cards/useCreateCard'
import { showPromise } from '../../shared/notifications/toastService'
import { CardType } from '../../types/card'

type CreateCardModalProps = {
  accountId: number
  isOpen: boolean
  onClose: () => void
  onCreated: () => Promise<void> | void
}

function CreateCardModal({ accountId, isOpen, onClose, onCreated }: CreateCardModalProps) {
  const { createCard, isLoading } = useCreateCard(accountId)

  async function handleSubmit(data: CreateCardFormData) {
    try {
      const payload =
        data.cardType === CardType.Credit
          ? {
              cardType: data.cardType,
              cardNumber: data.cardNumber,
              expirationDate: data.expirationDate,
              cardNetwork: data.cardNetwork,
              bankId: data.bankId,
              cardName: data.cardName,
              closingDay: Number(data.closingDay),
              dueDateDay: Number(data.dueDateDay),
            }
          : {
              cardType: data.cardType,
              cardNumber: data.cardNumber,
              expirationDate: data.expirationDate,
              cardNetwork: data.cardNetwork,
              bankId: data.bankId,
              cardName: data.cardName,
            }

      await showPromise(createCard(payload), {
        loading: 'Creating card...',
        success: 'Card created successfully',
        error: 'Could not create card',
      })
      await onCreated()
      onClose()
    } catch {
      // The promise toast displays the failure state and the modal remains open.
    }
  }

  function handleClose() {
    if (isLoading) {
      return
    }

    onClose()
  }

  return (
    <Modal
      hideFooter
      isOpen={isOpen}
      onClose={handleClose}
      title="Create Card"
    >
      <CreateCardForm isLoading={isLoading} onCancel={handleClose} onSubmit={handleSubmit} />
    </Modal>
  )
}

export default CreateCardModal
