import { useEffect, useMemo, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import CreateNewButton from '../../components/CreateNewButton'
import {
  getStoredSelectedAccountId,
  SELECTED_ACCOUNT_CHANGED_EVENT,
} from '../../features/accounts/accountSelectionStorage'
import { usePaginatedTransactions } from '../../features/transactions/usePaginatedTransactions'
import {
  getPaymentMethodLabel,
  getTransactionTypeLabel,
  type Transaction,
} from '../../types/transaction'
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

function TransactionRow({ transaction }: { transaction: Transaction }) {
  const transactionTypeClassName = [
    styles.transactionTypeBadge,
    transaction.transactionType === 'Expense' ? styles.transactionTypeExpense : '',
    transaction.transactionType === 'Income' ? styles.transactionTypeIncome : '',
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <tr>
      <td>{formatTransactionDate(transaction.transactionDate)}</td>
      <td className={styles.amountCell}>{currencyFormatter.format(transaction.amount)}</td>
      <td>{getPaymentMethodLabel(transaction.paymentMethod)}</td>
      <td>
        <span className={transactionTypeClassName}>
          {getTransactionTypeLabel(transaction.transactionType)}
        </span>
      </td>
      <td>{transaction.category?.categoryName ?? 'Unknown'}</td>
    </tr>
  )
}

function TransactionsPage() {
  const [selectedAccountId, setSelectedAccountId] = useState<number | null>(() =>
    getStoredSelectedAccountId(),
  )
  const [selectedPage, setSelectedPage] = useState(1)
  const alertedErrorsRef = useRef('')
  const {
    currentPage,
    error,
    errors,
    hasNextPage,
    hasPrevPage,
    isLoading,
    totalPages,
    transactions,
  } = usePaginatedTransactions({
    accountId: selectedAccountId,
    pageNumber: selectedPage,
    pageSize,
  })
  const paginationItems = useMemo(
    () => getPaginationItems(currentPage, totalPages),
    [currentPage, totalPages],
  )

  useEffect(() => {
    function handleSelectedAccountChange(event: Event) {
      const selectedAccountEvent = event as CustomEvent<{ accountId: number | null }>

      setSelectedAccountId(selectedAccountEvent.detail.accountId)
      setSelectedPage(1)
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
        <CreateNewButton disabled />
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
              </tr>
            </thead>
            <tbody>
              {isLoading && (
                <tr>
                  <td className={styles.stateCell} colSpan={5}>
                    Loading transactions...
                  </td>
                </tr>
              )}

              {!isLoading && Boolean(error) && (
                <tr>
                  <td className={styles.stateCell} colSpan={5}>
                    Could not load transactions.
                  </td>
                </tr>
              )}

              {!isLoading && !error && transactions.length === 0 && (
                <tr>
                  <td className={styles.stateCell} colSpan={5}>
                    No transactions found.
                  </td>
                </tr>
              )}

              {!isLoading &&
                !error &&
                transactions.map((transaction) => (
                  <TransactionRow key={transaction.id} transaction={transaction} />
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
    </section>
  )
}

export default TransactionsPage
