import { useEffect, useMemo, useRef, useState } from 'react'
import { Pencil } from 'lucide-react'
import { Link } from 'react-router-dom'
import CreateNewButton from '../../components/CreateNewButton'
import IconButton from '../../components/IconButton'
import Modal from '../../components/Modal'
import {
  getStoredSelectedAccountId,
  SELECTED_ACCOUNT_CHANGED_EVENT,
} from '../../features/accounts/accountSelectionStorage'
import { usePaginatedCards } from '../../features/cards/usePaginatedCards'
import { getCardNetworkLabel, getCardTypeLabel, type CardType } from '../../types/card'
import CardDetails from './CardDetails'
import CreateCardModal from './CreateCardModal'
import UpdateCardModal from './UpdateCardModal'
import styles from './CardsPage.module.css'

const pageSize = 10

type SelectedCard = {
  id: number
  cardType: CardType
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

function CardsPage() {
  const [selectedAccountId, setSelectedAccountId] = useState<number | null>(() =>
    getStoredSelectedAccountId(),
  )
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [editingCard, setEditingCard] = useState<SelectedCard | null>(null)
  const [selectedCard, setSelectedCard] = useState<SelectedCard | null>(null)
  const [selectedPage, setSelectedPage] = useState(1)
  const alertedErrorsRef = useRef('')
  const {
    cards,
    currentPage,
    error,
    errors,
    hasNextPage,
    hasPrevPage,
    isLoading,
    reload,
    totalPages,
  } = usePaginatedCards({ accountId: selectedAccountId, pageNumber: selectedPage, pageSize })
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
      setEditingCard(null)
      setSelectedCard(null)
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

  function openCardModal(card: SelectedCard) {
    setSelectedCard(card)
  }

  function closeCardModal() {
    setSelectedCard(null)
  }

  function openEditModal(card: SelectedCard) {
    setEditingCard(card)
  }

  function openEditModalFromDetails(cardId: number, cardType: CardType) {
    closeCardModal()
    openEditModal({ id: cardId, cardType })
  }

  function closeEditModal() {
    setEditingCard(null)
  }

  if (selectedAccountId === null) {
    return (
      <section className={styles.page}>
        <div className={styles.emptyState}>
          <h1 className={styles.emptyStateTitle}>Create an account first</h1>
          <p className={styles.emptyStateText}>
            An account needs to be created before cards can be managed.
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
          <h1 className={styles.title}>Card Summary</h1>
        </div>

        <div className={styles.tableWrapper}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th scope="col">Card Name</th>
                <th scope="col">Card Number</th>
                <th scope="col">Type</th>
                <th scope="col">Network</th>
                <th scope="col">Bank</th>
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
                  <td className={styles.stateCell} colSpan={7}>
                    Loading cards...
                  </td>
                </tr>
              )}

              {!isLoading && Boolean(error) && (
                <tr>
                  <td className={styles.stateCell} colSpan={7}>
                    Could not load cards.
                  </td>
                </tr>
              )}

              {!isLoading && !error && cards.length === 0 && (
                <tr>
                  <td className={styles.stateCell} colSpan={7}>
                    No cards found.
                  </td>
                </tr>
              )}

              {!isLoading && !error && cards.map((card) => (
                <tr key={card.id}>
                  <td>
                    <button
                      className={styles.cardNameButton}
                      onClick={() => openCardModal({ id: card.id, cardType: card.cardType })}
                      type="button"
                    >
                      {card.cardName ?? 'Unnamed card'}
                    </button>
                  </td>
                  <td>{card.cardNumber}</td>
                  <td>{getCardTypeLabel(card.cardType)}</td>
                  <td>{getCardNetworkLabel(card.cardNetwork)}</td>
                  <td>{card.bank.name}</td>
                  <td className={styles.statusCell}>
                    <input
                      aria-label={`${card.cardName ?? 'Unnamed card'} active status`}
                      checked={card.status.toLowerCase() === 'active'}
                      className={styles.checkbox}
                      readOnly
                      type="checkbox"
                    />
                  </td>
                  <td className={styles.actionsCell}>
                    <div className={styles.actions}>
                      <IconButton
                        label={`Edit ${card.cardName ?? 'card'}`}
                        onClick={() => openEditModal({ id: card.id, cardType: card.cardType })}
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

        <nav aria-label="Cards pagination" className={styles.pagination}>
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

      <CreateCardModal
        accountId={selectedAccountId}
        isOpen={isCreateModalOpen}
        onClose={closeCreateModal}
        onCreated={reload}
      />

      {editingCard !== null && (
        <UpdateCardModal
          accountId={selectedAccountId}
          cardId={editingCard.id}
          cardType={editingCard.cardType}
          isOpen={editingCard !== null}
          key={`${editingCard.cardType}-${editingCard.id}`}
          onClose={closeEditModal}
          onUpdated={reload}
        />
      )}

      <Modal
        cancelText="Close"
        isOpen={selectedCard !== null}
        onClose={closeCardModal}
        title="Card Details"
      >
        {selectedCard !== null && (
          <CardDetails
            accountId={selectedAccountId}
            cardId={selectedCard.id}
            cardType={selectedCard.cardType}
            onEdit={openEditModalFromDetails}
            onStatusChanged={reload}
          />
        )}
      </Modal>
    </section>
  )
}

export default CardsPage
