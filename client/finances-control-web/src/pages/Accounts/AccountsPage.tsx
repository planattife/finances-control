import { useEffect, useMemo, useRef, useState } from 'react'
import CreateNewButton from '../../components/CreateNewButton'
import IconButton from '../../components/IconButton'
import Modal from '../../components/Modal'
import { usePaginatedAccounts } from '../../features/accounts/usePaginatedAccounts'
import CreateAccountModal from './CreateAccountModal'
import AccountDetails from './AccountDetails'
import styles from './AccountsPage.module.css'

const pageSize = 5

function EditIcon() {
  return (
    <svg aria-hidden="true" fill="none" viewBox="0 0 24 24">
      <path d="M14.5 5.5h-8a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2v-8" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
      <path d="m13 13.5 6.2-6.2a1.8 1.8 0 0 0-2.5-2.5L10.5 11 10 14l3-.5Z" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
    </svg>
  )
}

function DeleteIcon() {
  return (
    <svg aria-hidden="true" fill="none" viewBox="0 0 24 24">
      <path d="M5 7h14" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
      <path d="M10 11v6" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
      <path d="M14 11v6" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
      <path d="M8 7l1-3h6l1 3" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
      <path d="M7 7l1 13h8l1-13" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
    </svg>
  )
}

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

function AccountsPage() {
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [selectedAccountId, setSelectedAccountId] = useState<number | null>(null)
  const [selectedPage, setSelectedPage] = useState(1)
  const alertedErrorsRef = useRef('')
  const {
    accounts,
    currentPage,
    error,
    errors,
    hasNextPage,
    hasPrevPage,
    isLoading,
    reload,
    totalPages,
  } = usePaginatedAccounts({ pageNumber: selectedPage, pageSize })
  const paginationItems = useMemo(
    () => getPaginationItems(currentPage, totalPages),
    [currentPage, totalPages],
  )

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

  function openAccountModal(accountId: number) {
    setSelectedAccountId(accountId)
  }

  function closeAccountModal() {
    setSelectedAccountId(null)
  }

  return (
    <section className={styles.page}>
      <div className={styles.header}>
        <CreateNewButton onClick={openCreateModal} />
      </div>

      <div className={styles.card}>
        <div className={styles.cardHeader}>
          <h1 className={styles.title}>Account Summary</h1>
        </div>

        <div className={styles.tableWrapper}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th scope="col">Name</th>
                <th className={styles.activeCell} scope="col">
                  IsActive
                </th>
                <th className={styles.actionsHeader} scope="col">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {isLoading && (
                <tr>
                  <td className={styles.stateCell} colSpan={3}>
                    Loading accounts...
                  </td>
                </tr>
              )}

              {!isLoading && Boolean(error) && (
                <tr>
                  <td className={styles.stateCell} colSpan={3}>
                    Could not load accounts.
                  </td>
                </tr>
              )}

              {!isLoading && !error && accounts.length === 0 && (
                <tr>
                  <td className={styles.stateCell} colSpan={3}>
                    No accounts found.
                  </td>
                </tr>
              )}

              {!isLoading && !error && accounts.map((account) => (
                <tr key={account.id}>
                  <td>
                    <button
                      className={styles.accountNameButton}
                      onClick={() => openAccountModal(account.id)}
                      type="button"
                    >
                      {account.name}
                    </button>
                  </td>
                  <td className={styles.activeCell}>
                    <input
                      aria-label={`${account.name} active status`}
                      checked={account.isActive}
                      className={styles.checkbox}
                      readOnly
                      type="checkbox"
                    />
                  </td>
                  <td className={styles.actionsCell}>
                    <div className={styles.actions}>
                      <IconButton label={`Edit ${account.name}`}>
                        <EditIcon />
                      </IconButton>
                      <IconButton label={`Delete ${account.name}`}>
                        <DeleteIcon />
                      </IconButton>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <nav aria-label="Accounts pagination" className={styles.pagination}>
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

      <CreateAccountModal
        isOpen={isCreateModalOpen}
        onClose={closeCreateModal}
        onCreated={reload}
      />

      <Modal
        cancelText="Close"
        isOpen={selectedAccountId !== null}
        onClose={closeAccountModal}
        title="Account Details"
      >
        {selectedAccountId !== null && <AccountDetails accountId={selectedAccountId} />}
      </Modal>
    </section>
  )
}

export default AccountsPage
