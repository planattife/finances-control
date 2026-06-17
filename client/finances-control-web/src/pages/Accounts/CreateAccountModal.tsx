import { useState } from 'react'
import Modal from '../../components/Modal'
import { useCreateAccount } from '../../features/accounts/useCreateAccount'
import styles from './AccountsPage.module.css'

type CreateAccountModalProps = {
  isOpen: boolean
  onClose: () => void
  onCreated: () => Promise<void> | void
}

function CreateAccountModal({ isOpen, onClose, onCreated }: CreateAccountModalProps) {
  const [name, setName] = useState('')
  const { createAccount, isLoading } = useCreateAccount()
  const trimmedName = name.trim()

  async function handleSubmit() {
    if (trimmedName.length === 0) {
      return
    }

    try {
      await createAccount({ name: trimmedName })
      setName('')
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
      cancelText="Cancel"
      isFormModal
      isOpen={isOpen}
      onClose={handleClose}
      onSubmit={handleSubmit}
      submitText={isLoading ? 'Creating...' : 'Create'}
      title="Create Account"
    >
      <div className={styles.createForm}>
        <label className={styles.field}>
          <span className={styles.fieldLabel}>Account Name</span>
          <input
            className={styles.textInput}
            disabled={isLoading}
            maxLength={50}
            minLength={2}
            name="name"
            onChange={(event) => setName(event.target.value)}
            type="text"
            value={name}
          />
        </label>
      </div>
    </Modal>
  )
}

export default CreateAccountModal
