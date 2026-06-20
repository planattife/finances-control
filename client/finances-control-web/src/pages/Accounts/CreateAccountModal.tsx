import CreateAccountForm, {
  type CreateAccountFormData,
} from '../../components/forms/CreateAccountForm/CreateAccountForm'
import Modal from '../../components/Modal'
import { useCreateAccount } from '../../features/accounts/useCreateAccount'

type CreateAccountModalProps = {
  isOpen: boolean
  onClose: () => void
  onCreated: () => Promise<void> | void
}

function CreateAccountModal({ isOpen, onClose, onCreated }: CreateAccountModalProps) {
  const { createAccount, isLoading } = useCreateAccount()

  async function handleSubmit(data: CreateAccountFormData) {
    try {
      await createAccount({ name: data.name })
      await onCreated()
      onClose()
    } catch (error) {
      const createError = error as Error
      alert(createError.message || 'Could not create account.')
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
