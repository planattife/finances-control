import styles from './ActionButtons.module.css'

interface ActionButtonsProps {
  isFormModal?: boolean
  onClose: () => void
  onSubmit?: () => void
  submitText?: string
  cancelText?: string
}

function ActionButtons({
  isFormModal = false,
  onClose,
  onSubmit,
  submitText = 'Submit',
  cancelText = 'Cancel',
}: ActionButtonsProps) {
  return (
    <div className={styles.actions}>
      <button className={styles.secondaryButton} onClick={onClose} type="button">
        {cancelText}
      </button>

      {isFormModal && (
        <button className={styles.primaryButton} onClick={onSubmit} type="button">
          {submitText}
        </button>
      )}
    </div>
  )
}

export default ActionButtons
