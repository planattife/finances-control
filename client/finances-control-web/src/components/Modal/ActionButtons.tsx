import ActionButton from '../ActionButton'
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
      <ActionButton onClick={onClose}>
        {cancelText}
      </ActionButton>

      {isFormModal && (
        <ActionButton onClick={onSubmit} variant="primary">
          {submitText}
        </ActionButton>
      )}
    </div>
  )
}

export default ActionButtons
