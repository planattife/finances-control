import ActionButton from '../../components/ActionButton'
import UpdateCategoryForm, {
  type UpdateCategoryFormData,
} from '../../components/forms/UpdateCategoryForm/UpdateCategoryForm'
import Modal from '../../components/Modal'
import { useChangeCategoryStatus } from '../../features/categories/useChangeCategoryStatus'
import { useCategoryDetails } from '../../features/categories/useCategoryDetails'
import { useUpdateCategory } from '../../features/categories/useUpdateCategory'
import { showPromise } from '../../shared/notifications/toastService'
import styles from './CategoriesPage.module.css'

type UpdateCategoryModalProps = {
  categoryId: number
  isOpen: boolean
  onClose: () => void
  onUpdated: () => Promise<void> | void
}

function UpdateCategoryModal({ categoryId, isOpen, onClose, onUpdated }: UpdateCategoryModalProps) {
  const {
    categoryDetails,
    error: categoryDetailsError,
    isLoading: isCategoryDetailsLoading,
    reload: reloadCategoryDetails,
  } = useCategoryDetails(categoryId)
  const { changeCategoryStatus, isLoading: isChangingCategoryStatus } = useChangeCategoryStatus()
  const { updateCategory, isLoading: isUpdating } = useUpdateCategory()
  const isBusy = isCategoryDetailsLoading || isUpdating || isChangingCategoryStatus
  const isActive = categoryDetails?.status.toLowerCase() === 'active'
  const nextStatus = isActive ? 'inactive' : 'active'
  const nextStatusActionLabel = nextStatus === 'active' ? 'Activating' : 'Inactivating'
  const nextStatusSuccessLabel = nextStatus === 'active' ? 'activated' : 'inactivated'
  const statusActionText = isChangingCategoryStatus
    ? `${isActive ? 'Inactivating' : 'Activating'}...`
    : isActive ? 'Inactivate' : 'Activate'

  async function handleSubmit(data: UpdateCategoryFormData) {
    if (isBusy) {
      return
    }

    try {
      await showPromise(
        updateCategory({
          categoryId,
          payload: {
            name: data.name,
          },
        }),
        {
          loading: 'Updating category...',
          success: 'Category updated successfully',
          error: 'Could not update category',
        },
      )
      await onUpdated()
      onClose()
    } catch {
      // The promise toast displays the failure state and the modal remains open.
    }
  }

  function handleClose() {
    if (isUpdating || isChangingCategoryStatus) {
      return
    }

    onClose()
  }

  async function handleStatusChange() {
    if (isBusy) {
      return
    }

    try {
      await showPromise(changeCategoryStatus({ categoryId, nextStatus }), {
        loading: `${nextStatusActionLabel} category...`,
        success: `Category ${nextStatusSuccessLabel} successfully`,
        error: 'Could not change category status',
      })
      await reloadCategoryDetails()
      await onUpdated()
    } catch {
      // The promise toast displays the failure state and the modal remains open.
    }
  }

  return (
    <Modal
      hideFooter
      isOpen={isOpen}
      onClose={handleClose}
      title="Edit Category"
    >
      {isCategoryDetailsLoading && (
        <p className={styles.stateCell}>Loading category details...</p>
      )}

      {!isCategoryDetailsLoading && (categoryDetailsError || !categoryDetails) && (
        <p className={styles.stateCell}>Could not load category details.</p>
      )}

      {!isCategoryDetailsLoading && !categoryDetailsError && categoryDetails && (
        <UpdateCategoryForm
          defaultValues={{ name: categoryDetails.name }}
          isLoading={isBusy}
          onCancel={handleClose}
          onSubmit={handleSubmit}
          statusAction={(
            <ActionButton
              dangerHover={isActive}
              disabled={isBusy}
              onClick={handleStatusChange}
            >
              {statusActionText}
            </ActionButton>
          )}
        />
      )}
    </Modal>
  )
}

export default UpdateCategoryModal
