import { useEffect, useMemo, useRef, useState } from 'react'
import { Pencil } from 'lucide-react'
import CreateNewButton from '../../components/CreateNewButton'
import IconButton from '../../components/IconButton'
import Modal from '../../components/Modal'
import { usePaginatedCategories } from '../../features/categories/usePaginatedCategories'
import CategoryDetails from './CategoryDetails'
import CreateCategoryModal from './CreateCategoryModal'
import UpdateCategoryModal from './UpdateCategoryModal'
import styles from './CategoriesPage.module.css'

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

function CategoriesPage() {
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [selectedPage, setSelectedPage] = useState(1)
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(null)
  const [editingCategoryId, setEditingCategoryId] = useState<number | null>(null)
  const alertedErrorsRef = useRef('')
  const {
    categories,
    currentPage,
    error,
    errors,
    hasNextPage,
    hasPrevPage,
    isLoading,
    reload,
    totalPages,
  } = usePaginatedCategories({ pageNumber: selectedPage, pageSize })
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

  function openCategoryModal(categoryId: number) {
    setSelectedCategoryId(categoryId)
  }

  function closeCategoryModal() {
    setSelectedCategoryId(null)
  }

  function openEditModal(categoryId: number) {
    setEditingCategoryId(categoryId)
  }

  function openEditModalFromDetails(categoryId: number) {
    closeCategoryModal()
    openEditModal(categoryId)
  }

  function closeEditModal() {
    setEditingCategoryId(null)
  }

  return (
    <section className={styles.page}>
      <div className={styles.header}>
        <CreateNewButton onClick={openCreateModal} />
      </div>

      <div className={styles.card}>
        <div className={styles.cardHeader}>
          <h1 className={styles.title}>Category Summary</h1>
        </div>

        <div className={styles.tableWrapper}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th scope="col">Name</th>
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
                  <td className={styles.stateCell} colSpan={3}>
                    Loading categories...
                  </td>
                </tr>
              )}

              {!isLoading && Boolean(error) && (
                <tr>
                  <td className={styles.stateCell} colSpan={3}>
                    Could not load categories.
                  </td>
                </tr>
              )}

              {!isLoading && !error && categories.length === 0 && (
                <tr>
                  <td className={styles.stateCell} colSpan={3}>
                    No categories found.
                  </td>
                </tr>
              )}

              {!isLoading && !error && categories.map((category) => (
                <tr key={category.id}>
                  <td>
                    <button
                      className={styles.categoryNameButton}
                      onClick={() => openCategoryModal(category.id)}
                      type="button"
                    >
                      {category.name}
                    </button>
                  </td>
                  <td className={styles.statusCell}>{category.isActive ? 'Active' : 'Inactive'}</td>
                  <td className={styles.actionsCell}>
                    <div className={styles.actions}>
                      <IconButton
                        disabled={category.isDefault}
                        label={`Edit ${category.name}`}
                        onClick={() => openEditModal(category.id)}
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

        <nav aria-label="Categories pagination" className={styles.pagination}>
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
        isOpen={selectedCategoryId !== null}
        onClose={closeCategoryModal}
        title="Category Details"
      >
        {selectedCategoryId !== null && (
          <CategoryDetails
            categoryId={selectedCategoryId}
            onEdit={openEditModalFromDetails}
            onStatusChanged={reload}
          />
        )}
      </Modal>

      <CreateCategoryModal
        isOpen={isCreateModalOpen}
        onClose={closeCreateModal}
        onCreated={reload}
      />

      {editingCategoryId !== null && (
        <UpdateCategoryModal
          categoryId={editingCategoryId}
          isOpen={editingCategoryId !== null}
          key={editingCategoryId}
          onClose={closeEditModal}
          onUpdated={reload}
        />
      )}
    </section>
  )
}

export default CategoriesPage
