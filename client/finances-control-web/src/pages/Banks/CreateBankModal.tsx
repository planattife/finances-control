import CreateBankForm, {
  type CreateBankFormData,
} from '../../components/forms/CreateBankForm/CreateBankForm'
import Modal from '../../components/Modal'
import { useCreateBank } from '../../features/banks/useCreateBank'
import { showPromise } from '../../shared/notifications/toastService'

type CreateBankModalProps = {
  isOpen: boolean
  onClose: () => void
  onCreated: () => Promise<void> | void
}

function CreateBankModal({ isOpen, onClose, onCreated }: CreateBankModalProps) {
  const { createBank, isLoading } = useCreateBank()

  async function handleSubmit(data: CreateBankFormData) {
    try {
      await showPromise(createBank({ name: data.name, accountType: data.accountType }), {
        loading: 'Creating bank...',
        success: 'Bank created successfully',
        error: 'Could not create bank',
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
      title="Create Bank"
    >
      <CreateBankForm isLoading={isLoading} onCancel={handleClose} onSubmit={handleSubmit} />
    </Modal>
  )
}

export default CreateBankModal
