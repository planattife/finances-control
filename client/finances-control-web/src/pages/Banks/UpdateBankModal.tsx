import ActionButton from '../../components/ActionButton'
import UpdateBankForm, {
  type UpdateBankFormData,
} from '../../components/forms/UpdateBankForm/UpdateBankForm'
import Modal from '../../components/Modal'
import { useBankDetails } from '../../features/banks/useBankDetails'
import { useChangeBankStatus } from '../../features/banks/useChangeBankStatus'
import { useUpdateBank } from '../../features/banks/useUpdateBank'
import { showPromise } from '../../shared/notifications/toastService'
import styles from './BanksPage.module.css'

type UpdateBankModalProps = {
  bankId: number
  isOpen: boolean
  onClose: () => void
  onUpdated: () => Promise<void> | void
}

function UpdateBankModal({ bankId, isOpen, onClose, onUpdated }: UpdateBankModalProps) {
  const {
    bankDetails,
    error: bankDetailsError,
    isLoading: isBankDetailsLoading,
    reload: reloadBankDetails,
  } = useBankDetails(bankId)
  const { changeBankStatus, isLoading: isChangingBankStatus } = useChangeBankStatus()
  const { updateBank, isLoading: isUpdating } = useUpdateBank()
  const isBusy = isBankDetailsLoading || isUpdating || isChangingBankStatus
  const isActive = bankDetails?.status.toLowerCase() === 'active'
  const nextStatus = isActive ? 'inactive' : 'active'
  const nextStatusActionLabel = nextStatus === 'active' ? 'Activating' : 'Inactivating'
  const nextStatusSuccessLabel = nextStatus === 'active' ? 'activated' : 'inactivated'
  const statusActionText = isChangingBankStatus
    ? `${isActive ? 'Inactivating' : 'Activating'}...`
    : isActive ? 'Inactivate' : 'Activate'

  async function handleSubmit(data: UpdateBankFormData) {
    if (isBusy) {
      return
    }

    try {
      await showPromise(
        updateBank({
          bankId,
          payload: {
            name: data.name,
            accountType: data.accountType,
          },
        }),
        {
          loading: 'Updating bank...',
          success: 'Bank updated successfully',
          error: 'Could not update bank',
        },
      )
      await onUpdated()
      onClose()
    } catch {
      // The promise toast displays the failure state and the modal remains open.
    }
  }

  function handleClose() {
    if (isUpdating || isChangingBankStatus) {
      return
    }

    onClose()
  }

  async function handleStatusChange() {
    if (isBusy) {
      return
    }

    try {
      await showPromise(changeBankStatus({ bankId, nextStatus }), {
        loading: `${nextStatusActionLabel} bank...`,
        success: `Bank ${nextStatusSuccessLabel} successfully`,
        error: 'Could not change bank status',
      })
      await reloadBankDetails()
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
      title="Edit Bank"
    >
      {isBankDetailsLoading && (
        <p className={styles.stateCell}>Loading bank details...</p>
      )}

      {!isBankDetailsLoading && (bankDetailsError || !bankDetails) && (
        <p className={styles.stateCell}>Could not load bank details.</p>
      )}

      {!isBankDetailsLoading && !bankDetailsError && bankDetails && (
        <UpdateBankForm
          defaultValues={{ name: bankDetails.name, accountType: bankDetails.accountType }}
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

export default UpdateBankModal
