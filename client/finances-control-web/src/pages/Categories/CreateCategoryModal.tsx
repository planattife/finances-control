import CreateCategoryForm, {
  type CreateCategoryFormData,
} from '../../components/forms/CreateCategoryForm/CreateCategoryForm'
import Modal from '../../components/Modal'
import { useCreateCategory } from '../../features/categories/useCreateCategory'
import { showPromise } from '../../shared/notifications/toastService'

type CreateCategoryModalProps = {
  isOpen: boolean
  onClose: () => void
  onCreated: () => Promise<void> | void
}

function CreateCategoryModal({ isOpen, onClose, onCreated }: CreateCategoryModalProps) {
  const { createCategory, isLoading } = useCreateCategory()

  async function handleSubmit(data: CreateCategoryFormData) {
    try {
      await showPromise(createCategory({ name: data.name }), {
        loading: 'Creating category...',
        success: 'Category created successfully',
        error: 'Could not create category',
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
      title="Create Category"
    >
      <CreateCategoryForm isLoading={isLoading} onCancel={handleClose} onSubmit={handleSubmit} />
    </Modal>
  )
}

export default CreateCategoryModal
