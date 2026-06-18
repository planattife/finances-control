import { type KeyboardEvent, type MouseEvent, type ReactNode, useEffect, useId, useRef } from 'react'
import { createPortal } from 'react-dom'
import { X } from 'lucide-react'
import ActionButtons from './ActionButtons'
import styles from './Modal.module.css'

interface ModalProps {
  isOpen: boolean
  onClose: () => void
  title: string
  children: ReactNode
  isFormModal?: boolean
  onSubmit?: () => void
  submitText?: string
  cancelText?: string
}

const focusableSelector = [
  'a[href]',
  'button:not([disabled])',
  'textarea:not([disabled])',
  'input:not([disabled])',
  'select:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
].join(',')

function getFocusableElements(container: HTMLDivElement) {
  return Array.from(container.querySelectorAll<HTMLElement>(focusableSelector)).filter(
    (element) => !element.hasAttribute('disabled') && element.getAttribute('aria-hidden') !== 'true',
  )
}

function Modal({
  isOpen,
  onClose,
  title,
  children,
  isFormModal = false,
  onSubmit,
  submitText = 'Submit',
  cancelText = 'Cancel',
}: ModalProps) {
  const titleId = useId()
  const modalRef = useRef<HTMLDivElement>(null)
  const previousActiveElementRef = useRef<Element | null>(null)

  useEffect(() => {
    if (!isOpen) {
      return
    }

    previousActiveElementRef.current = document.activeElement

    window.setTimeout(() => {
      const modal = modalRef.current

      if (!modal) {
        return
      }

      const firstFocusableElement = getFocusableElements(modal)[0]
      firstFocusableElement?.focus()

      if (!firstFocusableElement) {
        modal.focus()
      }
    }, 0)

    return () => {
      const previousActiveElement = previousActiveElementRef.current

      if (previousActiveElement instanceof HTMLElement) {
        previousActiveElement.focus()
      }
    }
  }, [isOpen])

  if (!isOpen) {
    return null
  }

  function handleBackdropClick(event: MouseEvent<HTMLDivElement>) {
    if (event.target === event.currentTarget) {
      onClose()
    }
  }

  function handleKeyDown(event: KeyboardEvent<HTMLDivElement>) {
    if (event.key === 'Escape') {
      event.stopPropagation()
      onClose()
      return
    }

    if (event.key !== 'Tab' || !modalRef.current) {
      return
    }

    const focusableElements = getFocusableElements(modalRef.current)

    if (focusableElements.length === 0) {
      event.preventDefault()
      modalRef.current.focus()
      return
    }

    const firstFocusableElement = focusableElements[0]
    const lastFocusableElement = focusableElements[focusableElements.length - 1]

    if (event.shiftKey && document.activeElement === firstFocusableElement) {
      event.preventDefault()
      lastFocusableElement.focus()
      return
    }

    if (!event.shiftKey && document.activeElement === lastFocusableElement) {
      event.preventDefault()
      firstFocusableElement.focus()
    }
  }

  return createPortal(
    <div className={styles.backdrop} onClick={handleBackdropClick}>
      <div
        aria-labelledby={titleId}
        aria-modal="true"
        className={styles.modal}
        onKeyDown={handleKeyDown}
        ref={modalRef}
        role="dialog"
        tabIndex={-1}
      >
        <header className={styles.header}>
          <h2 className={styles.title} id={titleId}>
            {title}
          </h2>
          <button aria-label="Close modal" className={styles.closeButton} onClick={onClose} type="button">
            <span className={styles.closeIcon}>
              <X aria-hidden="true" />
            </span>
          </button>
        </header>

        <div className={styles.body}>{children}</div>

        <footer className={styles.footer}>
          <ActionButtons
            cancelText={cancelText}
            isFormModal={isFormModal}
            onClose={onClose}
            onSubmit={onSubmit}
            submitText={submitText}
          />
        </footer>
      </div>
    </div>,
    document.body,
  )
}

export type { ModalProps }
export default Modal
