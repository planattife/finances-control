import { useMemo } from 'react'
import CreateTransactionForm, {
  type CreateTransactionFormData,
} from '../../components/forms/CreateTransactionForm/CreateTransactionForm'
import Modal from '../../components/Modal'
import { usePaginatedBanks } from '../../features/banks/usePaginatedBanks'
import { useLoadableCards } from '../../features/cards/useLoadableCards'
import { useLoadableCategories } from '../../features/categories/useLoadableCategories'
import { useCreateTransaction } from '../../features/transactions/useCreateTransaction'
import { showPromise } from '../../shared/notifications/toastService'
import { PaymentMethodCode, requiresBank, requiresCard, TransactionTypeCode } from '../../types/transaction'

type CreateTransactionModalProps = {
  accountId: number
  isOpen: boolean
  onClose: () => void
  onCreated: () => Promise<void> | void
}

function getLocalDateInputValue(date: Date) {
  const timezoneOffset = date.getTimezoneOffset() * 60_000

  return new Date(date.getTime() - timezoneOffset).toISOString().slice(0, 10)
}

function normalizeDescription(value: string | null | undefined) {
  const trimmedValue = value?.trim() ?? ''

  return trimmedValue.length > 0 ? trimmedValue : null
}

function CreateTransactionModal({
  accountId,
  isOpen,
  onClose,
  onCreated,
}: CreateTransactionModalProps) {
  const {
    banks,
    error: banksError,
    hasNextPage: hasMoreBanks,
    isLoading: isBanksLoading,
    isLoadingMore: isLoadingMoreBanks,
    loadNextPage: loadNextBankPage,
  } = usePaginatedBanks({ pageSize: 5 })
  const {
    cards,
    error: cardsError,
    hasNextPage: hasMoreCards,
    isLoading: isCardsLoading,
    isLoadingMore: isLoadingMoreCards,
    loadNextPage: loadNextCardPage,
  } = useLoadableCards({ accountId, pageSize: 5 })
  const {
    categories,
    error: categoriesError,
    hasNextPage: hasMoreCategories,
    isLoading: isCategoriesLoading,
    isLoadingMore: isLoadingMoreCategories,
    loadNextPage: loadNextCategoryPage,
  } = useLoadableCategories({ pageSize: 5 })
  const { createTransaction, isLoading: isCreating } = useCreateTransaction(accountId)
  const activeBanks = useMemo(
    () => banks.filter((bank) => bank.isActive),
    [banks],
  )

  async function handleSubmit(data: CreateTransactionFormData) {
    if (isCreating) {
      return
    }

    try {
      await showPromise(
        createTransaction({
          amount: data.amount,
          paymentMethod: data.paymentMethod,
          transactionType: data.transactionType,
          categoryId: data.categoryId,
          accountId,
          cardId: requiresCard(data.paymentMethod) ? data.cardId : null,
          bankId: requiresBank(data.paymentMethod) ? data.bankId : null,
          transactionDate: new Date(`${data.transactionDate}T00:00:00`).toISOString(),
          description: normalizeDescription(data.description),
        }),
        {
          loading: 'Creating transaction...',
          success: 'Transaction created successfully',
          error: 'Could not create transaction',
        },
      )
      await onCreated()
      onClose()
    } catch {
      // The promise toast displays the failure state and the modal remains open.
    }
  }

  function handleClose() {
    if (isCreating) {
      return
    }

    onClose()
  }

  return (
    <Modal hideFooter isOpen={isOpen} onClose={handleClose} title="Create Transaction">
      {categoriesError != null && <p>Could not load categories.</p>}
      {banksError != null && <p>Could not load banks.</p>}
      {cardsError != null && <p>Could not load cards.</p>}
      {categoriesError == null && banksError == null && cardsError == null && (
        <CreateTransactionForm
          banks={activeBanks}
          cards={cards}
          categories={categories}
          defaultValues={{
            amount: 0,
            paymentMethod: PaymentMethodCode.Cash,
            transactionType: TransactionTypeCode.Expense,
            categoryId: categories[0]?.id ?? 0,
            transactionDate: getLocalDateInputValue(new Date()),
            description: '',
            cardId: null,
            bankId: null,
          }}
          hasMoreBanks={hasMoreBanks}
          hasMoreCards={hasMoreCards}
          hasMoreCategories={hasMoreCategories}
          isLoading={isCreating}
          isLoadingBanks={isBanksLoading}
          isLoadingCards={isCardsLoading}
          isLoadingCategories={isCategoriesLoading}
          isLoadingMoreBanks={isLoadingMoreBanks}
          isLoadingMoreCards={isLoadingMoreCards}
          isLoadingMoreCategories={isLoadingMoreCategories}
          onCancel={handleClose}
          onLoadMoreBanks={async () => {
            await loadNextBankPage()
          }}
          onLoadMoreCards={async () => {
            await loadNextCardPage()
          }}
          onLoadMoreCategories={async () => {
            await loadNextCategoryPage()
          }}
          onSubmit={handleSubmit}
        />
      )}
    </Modal>
  )
}

export default CreateTransactionModal
