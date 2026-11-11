import { useEffect, useMemo, useRef, useState } from 'react'
import { Pencil } from 'lucide-react'
import CreateNewButton from '../../components/CreateNewButton'
import IconButton from '../../components/IconButton'
import Modal from '../../components/Modal'
import { usePaginatedBanks } from '../../features/banks/usePaginatedBanks'
import { getBankAccountTypeLabel } from '../../types/bank'
import BankDetails from './BankDetails'
import CreateBankModal from './CreateBankModal'
import UpdateBankModal from './UpdateBankModal'
import styles from './BanksPage.module.css'

const pageSize = 10

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

function BanksPage() {
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [selectedBankId, setSelectedBankId] = useState<number | null>(null)
  const [editingBankId, setEditingBankId] = useState<number | null>(null)
  const [selectedPage, setSelectedPage] = useState(1)
  const alertedErrorsRef = useRef('')
  const {
    banks,
    currentPage,
    error,
    errors,
    hasNextPage,
    hasPrevPage,
    isLoading,
    reload,
    totalPages,
  } = usePaginatedBanks({ pageNumber: selectedPage, pageSize })
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

  function openBankModal(bankId: number) {
    setSelectedBankId(bankId)
  }

  function closeBankModal() {
    setSelectedBankId(null)
  }

  function openEditModalFromDetails(bankId: number) {
    closeBankModal()
    openEditModal(bankId)
  }

  function openEditModal(bankId: number) {
    setEditingBankId(bankId)
  }

  function closeEditModal() {
    setEditingBankId(null)
  }

  return (
    <section className={styles.page}>
      <div className={styles.header}>
        <CreateNewButton onClick={openCreateModal} />
      </div>

      <div className={styles.card}>
        <div className={styles.cardHeader}>
          <h1 className={styles.title}>Bank Summary</h1>
        </div>

        <div className={styles.tableWrapper}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th scope="col">Name</th>
                <th scope="col">Account Type</th>
                <th className={styles.statusCell} scope="col">
                  Status
                </th>
                <th className={styles.actionsHeader} scope="col">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {isLoading && (
                <tr>
                  <td className={styles.stateCell} colSpan={4}>
                    Loading banks...
                  </td>
                </tr>
              )}

              {!isLoading && Boolean(error) && (
                <tr>
                  <td className={styles.stateCell} colSpan={4}>
                    Could not load banks.
                  </td>
                </tr>
              )}

              {!isLoading && !error && banks.length === 0 && (
                <tr>
                  <td className={styles.stateCell} colSpan={4}>
                    No banks found.
                  </td>
                </tr>
              )}

              {!isLoading && !error && banks.map((bank) => (
                <tr key={bank.id}>
                  <td>
                    <button
                      className={styles.bankNameButton}
                      onClick={() => openBankModal(bank.id)}
                      type="button"
                    >
                      {bank.name}
                    </button>
                  </td>
                  <td>{getBankAccountTypeLabel(bank.accountType)}</td>
                  <td className={styles.statusCell}>{bank.isActive ? 'Active' : 'Inactive'}</td>
                  <td className={styles.actionsCell}>
                    <div className={styles.actions}>
                      <IconButton
                        label={`Edit ${bank.name}`}
                        onClick={() => openEditModal(bank.id)}
                      >
                        <Pencil aria-hidden="true" />
                      </IconButton>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <nav aria-label="Banks pagination" className={styles.pagination}>
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

      <CreateBankModal
        isOpen={isCreateModalOpen}
        onClose={closeCreateModal}
        onCreated={reload}
      />

      {editingBankId !== null && (
        <UpdateBankModal
          bankId={editingBankId}
          isOpen={editingBankId !== null}
          key={editingBankId}
          onClose={closeEditModal}
          onUpdated={reload}
        />
      )}

      <Modal
        cancelText="Close"
        isOpen={selectedBankId !== null}
        onClose={closeBankModal}
        title="Bank Details"
      >
        {selectedBankId !== null && (
          <BankDetails
            bankId={selectedBankId}
            onEdit={openEditModalFromDetails}
            onStatusChanged={reload}
          />
        )}
      </Modal>
    </section>
  )
}

export default BanksPage
