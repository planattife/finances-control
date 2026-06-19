import { useState } from 'react'
import ActionButton from '../../components/ActionButton'
import Modal from '../../components/Modal'
import { useAccountDetails } from '../../features/accounts/useAccountDetails'
import { useChangeAccountStatus } from '../../features/accounts/useChangeAccountStatus'
import { useUpdateAccount } from '../../features/accounts/useUpdateAccount'
import styles from './AccountsPage.module.css'

type UpdateAccountModalProps = {
  accountId: number
  isOpen: boolean
  onClose: () => void
  onUpdated: () => Promise<void> | void
}

function UpdateAccountModal({ accountId, isOpen, onClose, onUpdated }: UpdateAccountModalProps) {
  const [editedName, setEditedName] = useState<string | null>(null)
  const {
    accountDetails,
    error: accountDetailsError,
    isLoading: isAccountDetailsLoading,
    reload: reloadAccountDetails,
  } = useAccountDetails(accountId)
  const { changeAccountStatus, isLoading: isChangingAccountStatus } = useChangeAccountStatus()
  const { updateAccount, isLoading: isUpdating } = useUpdateAccount()
  const isBusy = isAccountDetailsLoading || isUpdating || isChangingAccountStatus
  const name = editedName ?? accountDetails?.name ?? ''
  const trimmedName = name.trim()
  const isActive = accountDetails?.status.toLowerCase() === 'active'
  const nextStatus = isActive ? 'inactive' : 'active'
  const nextStatusLabel = nextStatus === 'active' ? 'Active' : 'Inactive'
  const statusActionText = isChangingAccountStatus
    ? `${isActive ? 'Inactivating' : 'Activating'}...`
    : isActive ? 'Inactivate' : 'Activate'

  async function handleSubmit() {
    if (trimmedName.length === 0 || isBusy) {
      return
    }

    try {
      await updateAccount({
        accountId,
        payload: { name: trimmedName },
      })
      await onUpdated()
      onClose()
      setEditedName(null)
      alert('Account was updated.')
    } catch (error) {
      const updateError = error as Error
      alert(updateError.message || 'Could not update account.')
    }
  }

  function handleClose() {
    if (isUpdating || isChangingAccountStatus) {
      return
    }

    onClose()
  }

  async function handleStatusChange() {
    if (isBusy) {
      return
    }

    try {
      await changeAccountStatus({ accountId, nextStatus })
      await reloadAccountDetails()
      await onUpdated()
      alert(`The Account Status was changed to ${nextStatusLabel}`)
    } catch (error) {
      const statusError = error as Error
      alert(statusError.message || 'Could not change account status.')
    }
  }

  return (
    <Modal
      cancelText="Cancel"
      isFormModal
      isOpen={isOpen}
      onClose={handleClose}
      onSubmit={handleSubmit}
      submitText={isUpdating ? 'Updating...' : 'Update'}
      title="Edit Account"
    >
      {isAccountDetailsLoading && (
        <p className={styles.stateCell}>Loading account details...</p>
      )}

      {!isAccountDetailsLoading && (accountDetailsError || !accountDetails) && (
        <p className={styles.stateCell}>Could not load account details.</p>
      )}

      {!isAccountDetailsLoading && !accountDetailsError && accountDetails && (
        <div className={styles.createForm}>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>Account Name</span>
            <input
              className={styles.textInput}
              disabled={isBusy}
              maxLength={50}
              minLength={2}
              name="name"
              onChange={(event) => setEditedName(event.target.value)}
              type="text"
              value={name}
            />
          </label>
          <ActionButton
            className={styles.statusAction}
            dangerHover={isActive}
            disabled={isBusy}
            onClick={handleStatusChange}
          >
            {statusActionText}
          </ActionButton>
        </div>
      )}
    </Modal>
  )
}

export default UpdateAccountModal
