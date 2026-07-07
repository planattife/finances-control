import { useEffect, useMemo, useRef, useState } from 'react'
import { Pencil, Trash2 } from 'lucide-react'
import { Link } from 'react-router-dom'
import CreateNewButton from '../../components/CreateNewButton'
import IconButton from '../../components/IconButton'
import Modal from '../../components/Modal'
import {
  getStoredSelectedAccountId,
  SELECTED_ACCOUNT_CHANGED_EVENT,
} from '../../features/accounts/accountSelectionStorage'
import { useDeleteTransaction } from '../../features/transactions/useDeleteTransaction'
import { usePaginatedTransactions } from '../../features/transactions/usePaginatedTransactions'
import { showPromise } from '../../shared/notifications/toastService'
import {
  getPaymentMethodLabel,
  getTransactionTypeLabel,
  type Transaction,
} from '../../types/transaction'
import CreateTransactionModal from './CreateTransactionModal'
import DeleteTransactionConfirmationModal from './DeleteTransactionConfirmationModal'
import TransactionDetails from './TransactionDetails'
import UpdateTransactionModal from './UpdateTransactionModal'
import styles from './TransactionsPage.module.css'

const pageSize = 10

const dateFormatter = new Intl.DateTimeFormat('pt-BR')
const currencyFormatter = new Intl.NumberFormat('pt-BR', {
  currency: 'BRL',
  style: 'currency',
})

function getPaginationItems(currentPage: number, totalPages: number) {
  if (totalPages <= 0) {
    return []
  }

  const visiblePages = new Set([1, totalPages, currentPage - 1, currentPage, currentPage + 1])
  const pages = Array.from(visiblePages)
    .filter((page) => page >= 1 && page <= totalPages)
    .sort((firstPage, secondPage) => firstPage - secondPage)

  return pages.reduce<Array<number | 'ellipsis'>>((items, page, index) => {
    const previousPage = pages[index - 1]

    if (previousPage && page - previousPage > 1) {
      items.push('ellipsis')
    }

    items.push(page)

    return items
  }, [])
}

function formatTransactionDate(transactionDate: string) {
  const date = new Date(transactionDate)

  if (Number.isNaN(date.getTime())) {
    return transactionDate
  }

  return dateFormatter.format(date)
}

function TransactionRow({
  isDeleting,
  onDeleteRequest,
  onEdit,
  onClick,
  transaction,
}: {
  isDeleting: boolean
  onDeleteRequest: (transactionId: number) => void
  onEdit: (transactionId: number) => void
  onClick: (transactionId: number) => void
  transaction: Transaction
}) {
  const transactionTypeClassName = [
    styles.transactionTypeBadge,
    transaction.transactionType === 'Expense' ? styles.transactionTypeExpense : '',
    transaction.transactionType === 'Income' ? styles.transactionTypeIncome : '',
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <tr
      className={styles.clickableRow}
      onClick={() => onClick(transaction.id)}
      tabIndex={0}
      onKeyDown={(event) => {
        if (event.key === 'Enter' || event.key === ' ') {
          event.preventDefault()
          onClick(transaction.id)
        }
      }}
    >
      <td>{formatTransactionDate(transaction.transactionDate)}</td>
      <td className={styles.amountCell}>{currencyFormatter.format(transaction.amount)}</td>
      <td>{getPaymentMethodLabel(transaction.paymentMethod)}</td>
      <td>
        <span className={transactionTypeClassName}>
          {getTransactionTypeLabel(transaction.transactionType)}
        </span>
      </td>
      <td>{transaction.category?.categoryName ?? 'Unknown'}</td>
      <td className={styles.actionsCell}>
        <div className={styles.actions}>
          <IconButton
            className={styles.dangerIconButton}
            disabled={isDeleting}
            label={`Delete transaction ${transaction.id}`}
            onClick={(event) => {
              event.stopPropagation()
              onDeleteRequest(transaction.id)
            }}
          >
            <Trash2 aria-hidden="true" />
          </IconButton>
          <IconButton
            label={`Edit transaction ${transaction.id}`}
            onClick={(event) => {
              event.stopPropagation()
              onEdit(transaction.id)
            }}
          >
            <Pencil aria-hidden="true" />
          </IconButton>
        </div>
      </td>
    </tr>
  )
}

function TransactionsPage() {
  const [selectedAccountId, setSelectedAccountId] = useState<number | null>(() =>
    getStoredSelectedAccountId(),
  )
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [selectedPage, setSelectedPage] = useState(1)
  const [selectedTransactionId, setSelectedTransactionId] = useState<number | null>(null)
  const [editingTransactionId, setEditingTransactionId] = useState<number | null>(null)
  const [transactionPendingDeleteId, setTransactionPendingDeleteId] = useState<number | null>(null)
  const alertedErrorsRef = useRef('')
  const {
    currentPage,
    error,
    errors,
    hasNextPage,
    hasPrevPage,
    isLoading,
    reload,
    totalPages,
    transactions,
  } = usePaginatedTransactions({
    accountId: selectedAccountId,
    pageNumber: selectedPage,
    pageSize,
  })
  const { deleteTransaction, isLoading: isDeletingTransaction } = useDeleteTransaction()
  const paginationItems = useMemo(
    () => getPaginationItems(currentPage, totalPages),
    [currentPage, totalPages],
  )

  useEffect(() => {
    function handleSelectedAccountChange(event: Event) {
      const selectedAccountEvent = event as CustomEvent<{ accountId: number | null }>

      setSelectedAccountId(selectedAccountEvent.detail.accountId)
      setSelectedPage(1)
      setIsCreateModalOpen(false)
      setSelectedTransactionId(null)
      setEditingTransactionId(null)
      setTransactionPendingDeleteId(null)
    }

    window.addEventListener(SELECTED_ACCOUNT_CHANGED_EVENT, handleSelectedAccountChange)

    return () => {
      window.removeEventListener(SELECTED_ACCOUNT_CHANGED_EVENT, handleSelectedAccountChange)
    }
  }, [])

  useEffect(() => {
    if (errors.length === 0) {
      alertedErrorsRef.current = ''
      return
    }

    const errorMessage = errors.join('\n')

    if (alertedErrorsRef.current !== errorMessage) {
      alertedErrorsRef.current = errorMessage
      alert(errorMessage)
    }
  }, [errors])

  function goToPage(pageNumber: number) {
    if (pageNumber < 1 || pageNumber > totalPages || pageNumber === currentPage) {
      return
    }

    setSelectedPage(pageNumber)
  }

  function goToPreviousPage() {
    if (hasPrevPage) {
      setSelectedPage(currentPage - 1)
    }
  }

  function goToNextPage() {
    if (hasNextPage) {
      setSelectedPage(currentPage + 1)
    }
  }

  function openCreateModal() {
    setIsCreateModalOpen(true)
  }

  function closeCreateModal() {
    setIsCreateModalOpen(false)
  }

  function openTransactionModal(transactionId: number) {
    setSelectedTransactionId(transactionId)
  }

  function closeTransactionModal() {
    setSelectedTransactionId(null)
  }

  function openEditModal(transactionId: number) {
    setEditingTransactionId(transactionId)
  }

  function openEditModalFromDetails(transactionId: number) {
    closeTransactionModal()
    openEditModal(transactionId)
  }

  function closeEditModal() {
    setEditingTransactionId(null)
  }

  function openDeleteConfirmation(transactionId: number) {
    setTransactionPendingDeleteId(transactionId)
  }

  function closeDeleteConfirmation() {
    if (isDeletingTransaction) {
      return
    }

    setTransactionPendingDeleteId(null)
  }

  async function confirmDeleteTransaction() {
    if (selectedAccountId === null || transactionPendingDeleteId === null || isDeletingTransaction) {
      return
    }

    const transactionId = transactionPendingDeleteId

    try {
      await showPromise(deleteTransaction({ accountId: selectedAccountId, transactionId }), {
        loading: 'Deleting transaction...',
        success: 'Transaction deleted successfully',
        error: 'Could not delete transaction',
      })

      if (selectedTransactionId === transactionId) {
        closeTransactionModal()
      }

      setTransactionPendingDeleteId(null)
      await reload()
    } catch {
      // The promise toast displays the failure state and the list remains unchanged.
    }
  }

  if (selectedAccountId === null) {
    return (
      <section className={styles.page}>
        <div className={styles.emptyState}>
          <h1 className={styles.emptyStateTitle}>Create an account first</h1>
          <p className={styles.emptyStateText}>
            An account needs to be created before transactions can be viewed.
          </p>
          <Link className={styles.emptyStateLink} to="/accounts">
            Go to accounts
          </Link>
        </div>
      </section>
    )
  }

  return (
    <section className={styles.page}>
      <div className={styles.header}>
        <CreateNewButton onClick={openCreateModal} />
      </div>

      <div className={styles.card}>
        <div className={styles.cardHeader}>
          <h1 className={styles.title}>Transaction Summary</h1>
        </div>

        <div className={styles.tableWrapper}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th scope="col">Transaction Date</th>
                <th scope="col">Amount</th>
                <th scope="col">Payment Method</th>
                <th scope="col">Transaction Type</th>
                <th scope="col">Category</th>
                <th className={styles.actionsHeader} scope="col">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {isLoading && (
                <tr>
                  <td className={styles.stateCell} colSpan={6}>
                    Loading transactions...
                  </td>
                </tr>
              )}

              {!isLoading && Boolean(error) && (
                <tr>
                  <td className={styles.stateCell} colSpan={6}>
                    Could not load transactions.
                  </td>
                </tr>
              )}

              {!isLoading && !error && transactions.length === 0 && (
                <tr>
                  <td className={styles.stateCell} colSpan={6}>
                    No transactions found.
                  </td>
                </tr>
              )}

              {!isLoading &&
                !error &&
                transactions.map((transaction) => (
                  <TransactionRow
                    key={transaction.id}
                    isDeleting={isDeletingTransaction}
                    onDeleteRequest={openDeleteConfirmation}
                    onEdit={openEditModal}
                    onClick={openTransactionModal}
                    transaction={transaction}
                  />
                ))}
            </tbody>
          </table>
        </div>

        <nav aria-label="Transactions pagination" className={styles.pagination}>
          <button
            className={styles.pageItem}
            disabled={!hasPrevPage || isLoading}
            onClick={goToPreviousPage}
            type="button"
          >
            &lt;
          </button>
          {paginationItems.map((item, index) => {
            if (item === 'ellipsis') {
              return (
                <span className={styles.pageItem} key={`ellipsis-${index}`}>
                  ...
                </span>
              )
            }

            return (
              <button
                className={`${styles.pageItem} ${item === currentPage ? styles.pageItemActive : ''}`.trim()}
                disabled={isLoading}
                key={item}
                onClick={() => goToPage(item)}
                type="button"
              >
                {item}
              </button>
            )
          })}
          <button
            className={styles.pageItem}
            disabled={!hasNextPage || isLoading}
            onClick={goToNextPage}
            type="button"
          >
            &gt;
          </button>
        </nav>
      </div>

      <Modal
        cancelText="Close"
        isOpen={selectedTransactionId !== null}
        onClose={closeTransactionModal}
        title="Transaction Details"
      >
        {selectedAccountId !== null && selectedTransactionId !== null && (
          <TransactionDetails
            accountId={selectedAccountId}
            isDeleting={isDeletingTransaction}
            onDeleteRequest={openDeleteConfirmation}
            onEdit={openEditModalFromDetails}
            transactionId={selectedTransactionId}
          />
        )}
      </Modal>

      {editingTransactionId !== null && (
        <UpdateTransactionModal
          accountId={selectedAccountId}
          isOpen={editingTransactionId !== null}
          key={editingTransactionId}
          onClose={closeEditModal}
          onUpdated={reload}
          transactionId={editingTransactionId}
        />
      )}

      <CreateTransactionModal
        accountId={selectedAccountId}
        isOpen={isCreateModalOpen}
        onClose={closeCreateModal}
        onCreated={reload}
      />

      <DeleteTransactionConfirmationModal
        isDeleting={isDeletingTransaction}
        isOpen={transactionPendingDeleteId !== null}
        onCancel={closeDeleteConfirmation}
        onConfirm={confirmDeleteTransaction}
      />
    </section>
  )
}

export default TransactionsPage
