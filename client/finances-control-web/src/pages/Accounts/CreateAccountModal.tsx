import CreateAccountForm, {
  type CreateAccountFormData,
} from '../../components/forms/CreateAccountForm/CreateAccountForm'
import Modal from '../../components/Modal'
import { useCreateAccount } from '../../features/accounts/useCreateAccount'
import { showPromise } from '../../shared/notifications/toastService'

type CreateAccountModalProps = {
  isOpen: boolean
  onClose: () => void
  onCreated: () => Promise<void> | void
}

function CreateAccountModal({ isOpen, onClose, onCreated }: CreateAccountModalProps) {
  const { createAccount, isLoading } = useCreateAccount()

  async function handleSubmit(data: CreateAccountFormData) {
    try {
      await showPromise(createAccount({ name: data.name }), {
        loading: 'Creating account...',
        success: 'Account created successfully',
        error: 'Could not create account',
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
      title="Create Account"
    >
      <CreateAccountForm isLoading={isLoading} onCancel={handleClose} onSubmit={handleSubmit} />
    </Modal>
  )
}

export default CreateAccountModal
