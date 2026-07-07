import { useEffect, useMemo } from 'react'
import UpdateTransactionForm, {
  type UpdateTransactionFormData,
} from '../../components/forms/UpdateTransactionForm/UpdateTransactionForm'
import Modal from '../../components/Modal'
import { useLoadableCategories } from '../../features/categories/useLoadableCategories'
import { useTransactionDetails } from '../../features/transactions/useTransactionDetails'
import { useUpdateTransaction } from '../../features/transactions/useUpdateTransaction'
import { showPromise } from '../../shared/notifications/toastService'
import type { Category } from '../../types/category'
import styles from './TransactionsPage.module.css'

type UpdateTransactionModalProps = {
  accountId: number
  isOpen: boolean
  onClose: () => void
  onUpdated: () => Promise<void> | void
  transactionId: number
}

function normalizeDescription(value: string | null | undefined) {
  const trimmedValue = value?.trim() ?? ''

  return trimmedValue.length > 0 ? trimmedValue : null
}

function UpdateTransactionModal({
  accountId,
  isOpen,
  onClose,
  onUpdated,
  transactionId,
}: UpdateTransactionModalProps) {
  const {
    transactionDetails,
    error: transactionDetailsError,
    isLoading: isTransactionDetailsLoading,
  } = useTransactionDetails(accountId, transactionId)
  const {
    categories,
    error: categoriesError,
    hasNextPage: hasMoreCategories,
    isLoading: isCategoriesLoading,
    isLoadingMore: isLoadingMoreCategories,
    loadNextPage,
    loadUntilCategory,
  } = useLoadableCategories({ pageSize: 5 })
  const { isLoading: isUpdating, updateTransaction } = useUpdateTransaction()
  const isBusy = isTransactionDetailsLoading || isCategoriesLoading || isUpdating

  useEffect(() => {
    if (!isOpen || !transactionDetails || isCategoriesLoading) {
      return
    }

    const categoryId = transactionDetails.category.categoryId
    const isCurrentCategoryLoaded = categories.some((category) => category.id === categoryId)

    if (!isCurrentCategoryLoaded) {
      void loadUntilCategory(categoryId)
    }
  }, [categories, isCategoriesLoading, isOpen, loadUntilCategory, transactionDetails])

  const categoryOptions = useMemo<Category[]>(() => {
    if (!transactionDetails) {
      return categories
    }

    const currentCategory = transactionDetails.category

    if (categories.some((category) => category.id === currentCategory.categoryId)) {
      return categories
    }

    return [
      {
        id: currentCategory.categoryId,
        name: currentCategory.categoryName,
        isActive: true,
        isDefault: false,
      },
      ...categories,
    ]
  }, [categories, transactionDetails])

  async function handleSubmit(data: UpdateTransactionFormData) {
    if (isBusy || !transactionDetails) {
      return
    }

    const initialDescription = normalizeDescription(transactionDetails.description)
    const nextDescription = normalizeDescription(data.description)

    try {
      await showPromise(
        updateTransaction({
          accountId,
          transactionId,
          payload: {
            newAmount: data.amount,
            newCategoryId: data.categoryId,
            newDescription: nextDescription,
            hasDescriptionUpdate: nextDescription !== initialDescription,
          },
        }),
        {
          loading: 'Updating transaction...',
          success: 'Transaction updated successfully',
          error: 'Could not update transaction',
        },
      )
      await onUpdated()
      onClose()
    } catch {
      // The promise toast displays the failure state and the modal remains open.
    }
  }

  function handleClose() {
    if (isUpdating) {
      return
    }

    onClose()
  }

  return (
    <Modal hideFooter isOpen={isOpen} onClose={handleClose} title="Edit Transaction">
      {isTransactionDetailsLoading && (
        <p className={styles.stateCell}>Loading transaction details...</p>
      )}

      {!isTransactionDetailsLoading && (transactionDetailsError || !transactionDetails) && (
        <p className={styles.stateCell}>Could not load transaction details.</p>
      )}

      {!isTransactionDetailsLoading && transactionDetails && categoriesError != null && (
        <p className={styles.stateCell}>Could not load categories.</p>
      )}

      {!isTransactionDetailsLoading && transactionDetails && categoriesError == null && (
        <UpdateTransactionForm
          categories={categoryOptions}
          defaultValues={{
            amount: transactionDetails.amount,
            categoryId: transactionDetails.category.categoryId,
            description: transactionDetails.description ?? '',
          }}
          hasMoreCategories={hasMoreCategories}
          isLoading={isBusy}
          isLoadingCategories={isCategoriesLoading}
          isLoadingMoreCategories={isLoadingMoreCategories}
          onCancel={handleClose}
          onLoadMoreCategories={async () => {
            await loadNextPage()
          }}
          onSubmit={handleSubmit}
        />
      )}
    </Modal>
  )
}

export default UpdateTransactionModal
