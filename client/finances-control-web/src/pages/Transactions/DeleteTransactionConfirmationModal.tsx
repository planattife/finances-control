import Modal from '../../components/Modal'
import styles from './DeleteTransactionConfirmationModal.module.css'

interface DeleteTransactionConfirmationModalProps {
  isDeleting: boolean
  isOpen: boolean
  onCancel: () => void
  onConfirm: () => void
}

function DeleteTransactionConfirmationModal({
  isDeleting,
  isOpen,
  onCancel,
  onConfirm,
}: DeleteTransactionConfirmationModalProps) {
  function handleConfirm() {
    if (isDeleting) {
      return
    }

    onConfirm()
  }

  return (
    <Modal
      cancelText="Cancel"
      isFormModal
      isOpen={isOpen}
      onClose={onCancel}
      onSubmit={handleConfirm}
      submitText="Yes"
      title="Delete Transaction"
    >
      <p className={styles.message}>Are you sure you want to delete this transaction?</p>
    </Modal>
  )
}

export default DeleteTransactionConfirmationModal
