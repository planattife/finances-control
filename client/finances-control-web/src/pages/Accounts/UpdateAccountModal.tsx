import ActionButton from '../../components/ActionButton'
import UpdateAccountForm, {
  type UpdateAccountFormData,
} from '../../components/forms/UpdateAccountForm/UpdateAccountForm'
import Modal from '../../components/Modal'
import { useAccountDetails } from '../../features/accounts/useAccountDetails'
import { useChangeAccountStatus } from '../../features/accounts/useChangeAccountStatus'
import { useUpdateAccount } from '../../features/accounts/useUpdateAccount'
import { showPromise } from '../../shared/notifications/toastService'
import styles from './AccountsPage.module.css'

type UpdateAccountModalProps = {
  accountId: number
  isOpen: boolean
  onClose: () => void
  onUpdated: () => Promise<void> | void
}

function UpdateAccountModal({ accountId, isOpen, onClose, onUpdated }: UpdateAccountModalProps) {
  const {
    accountDetails,
    error: accountDetailsError,
    isLoading: isAccountDetailsLoading,
    reload: reloadAccountDetails,
  } = useAccountDetails(accountId)
  const { changeAccountStatus, isLoading: isChangingAccountStatus } = useChangeAccountStatus()
  const { updateAccount, isLoading: isUpdating } = useUpdateAccount()
  const isBusy = isAccountDetailsLoading || isUpdating || isChangingAccountStatus
  const isActive = accountDetails?.status.toLowerCase() === 'active'
  const nextStatus = isActive ? 'inactive' : 'active'
  const nextStatusActionLabel = nextStatus === 'active' ? 'Activating' : 'Inactivating'
  const nextStatusSuccessLabel = nextStatus === 'active' ? 'activated' : 'inactivated'
  const statusActionText = isChangingAccountStatus
    ? `${isActive ? 'Inactivating' : 'Activating'}...`
    : isActive ? 'Inactivate' : 'Activate'

  async function handleSubmit(data: UpdateAccountFormData) {
    if (isBusy) {
      return
    }

    try {
      await showPromise(
        updateAccount({
          accountId,
          payload: { name: data.name },
        }),
        {
          loading: 'Updating account...',
          success: 'Account updated successfully',
          error: 'Could not update account',
        },
      )
      await onUpdated()
      onClose()
    } catch {
      // The promise toast displays the failure state and the modal remains open.
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
      await showPromise(changeAccountStatus({ accountId, nextStatus }), {
        loading: `${nextStatusActionLabel} account...`,
        success: `Account ${nextStatusSuccessLabel} successfully`,
        error: 'Could not change account status',
      })
      await reloadAccountDetails()
      await onUpdated()
    } catch {
      // The promise toast displays the failure state and the modal remains open.
    }
  }

  return (
    <Modal
      hideFooter
      isOpen={isOpen}
      onClose={handleClose}
      title="Edit Account"
    >
      {isAccountDetailsLoading && (
        <p className={styles.stateCell}>Loading account details...</p>
      )}

      {!isAccountDetailsLoading && (accountDetailsError || !accountDetails) && (
        <p className={styles.stateCell}>Could not load account details.</p>
      )}

      {!isAccountDetailsLoading && !accountDetailsError && accountDetails && (
        <UpdateAccountForm
          defaultValues={{ name: accountDetails.name }}
          isLoading={isBusy}
          onCancel={handleClose}
          onSubmit={handleSubmit}
          statusAction={(
            <ActionButton
              dangerHover={isActive}
              disabled={isBusy}
              onClick={handleStatusChange}
            >
              {statusActionText}
            </ActionButton>
          )}
        />
      )}
    </Modal>
  )
}

export default UpdateAccountModal
