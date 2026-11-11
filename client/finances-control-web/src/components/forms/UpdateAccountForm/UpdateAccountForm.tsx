import { useEffect, type ReactNode } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import ActionButton from '../../ActionButton'
import styles from './UpdateAccountForm.module.css'

const updateAccountFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Account name is required.')
    .min(2, 'Account name must be at least 2 characters.')
    .max(50, 'Account name must be at most 50 characters.'),
})

export type UpdateAccountFormData = z.infer<typeof updateAccountFormSchema>

type UpdateAccountFormProps = {
  defaultValues: UpdateAccountFormData
  isLoading?: boolean
  onCancel: () => void
  onSubmit: (data: UpdateAccountFormData) => Promise<void> | void
  statusAction?: ReactNode
}

function UpdateAccountForm({
  defaultValues,
  isLoading = false,
  onCancel,
  onSubmit,
  statusAction,
}: UpdateAccountFormProps) {
  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
    reset,
  } = useForm<UpdateAccountFormData>({
    resolver: zodResolver(updateAccountFormSchema),
    defaultValues,
  })

  useEffect(() => {
    reset(defaultValues)
  }, [defaultValues, reset])

  const isBusy = isLoading || isSubmitting
  const nameError = errors.name?.message
  const nameInputClassName = `${styles.textInput} ${nameError ? styles.textInputError : ''}`.trim()

  async function handleFormSubmit(data: UpdateAccountFormData) {
    await onSubmit(data)
  }

  return (
    <div className={styles.wrapper}>
      <form className={styles.form} onSubmit={handleSubmit(handleFormSubmit)} noValidate>
        <label className={styles.field}>
          <span className={styles.fieldLabel}>Account Name</span>
          <input
            aria-describedby={nameError ? 'update-account-name-error' : undefined}
            aria-invalid={Boolean(nameError)}
            className={nameInputClassName}
            disabled={isBusy}
            maxLength={50}
            minLength={2}
            type="text"
            {...register('name')}
          />
          {nameError && (
            <p className={styles.errorMessage} id="update-account-name-error">
              {nameError}
            </p>
          )}
        </label>

        {statusAction && <div className={styles.statusAction}>{statusAction}</div>}

        <div className={styles.actions}>
          <ActionButton disabled={isBusy} onClick={onCancel} type="button">
            Cancel
          </ActionButton>
          <ActionButton disabled={isBusy} type="submit" variant="primary">
            {isBusy ? 'Updating...' : 'Update'}
          </ActionButton>
        </div>
      </form>
    </div>
  )
}

export default UpdateAccountForm
