import { useEffect, type ReactNode } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import ActionButton from '../../ActionButton'
import { bankAccountTypeOptions, BankAccountType } from '../../../types/bank'
import styles from './UpdateBankForm.module.css'

const updateBankFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Bank name is required.')
    .min(2, 'Bank name must be at least 2 characters.')
    .max(50, 'Bank name must be at most 50 characters.'),
  accountType: z.coerce
    .number({ error: 'Account type is required.' })
    .pipe(z.union([z.literal(BankAccountType.Savings), z.literal(BankAccountType.Check)])),
})

type UpdateBankFormInput = z.input<typeof updateBankFormSchema>

export type UpdateBankFormData = z.output<typeof updateBankFormSchema>

type UpdateBankFormProps = {
  defaultValues: UpdateBankFormData
  isLoading?: boolean
  onCancel: () => void
  onSubmit: (data: UpdateBankFormData) => Promise<void> | void
  statusAction?: ReactNode
}

function UpdateBankForm({
  defaultValues,
  isLoading = false,
  onCancel,
  onSubmit,
  statusAction,
}: UpdateBankFormProps) {
  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
    reset,
  } = useForm<UpdateBankFormInput, unknown, UpdateBankFormData>({
    resolver: zodResolver(updateBankFormSchema),
    defaultValues,
  })

  useEffect(() => {
    reset(defaultValues)
  }, [defaultValues, reset])

  const isBusy = isLoading || isSubmitting
  const nameError = errors.name?.message
  const accountTypeError = errors.accountType?.message
  const nameInputClassName = `${styles.textInput} ${nameError ? styles.textInputError : ''}`.trim()
  const selectClassName = `${styles.textInput} ${accountTypeError ? styles.textInputError : ''}`.trim()

  async function handleFormSubmit(data: UpdateBankFormData) {
    await onSubmit(data)
  }

  return (
    <div className={styles.wrapper}>
      <form className={styles.form} onSubmit={handleSubmit(handleFormSubmit)} noValidate>
        <label className={styles.field}>
          <span className={styles.fieldLabel}>Bank Name</span>
          <input
            aria-describedby={nameError ? 'update-bank-name-error' : undefined}
            aria-invalid={Boolean(nameError)}
            className={nameInputClassName}
            disabled={isBusy}
            maxLength={50}
            minLength={2}
            type="text"
            {...register('name')}
          />
          {nameError && (
            <p className={styles.errorMessage} id="update-bank-name-error">
              {nameError}
            </p>
          )}
        </label>

        <label className={styles.field}>
          <span className={styles.fieldLabel}>Account Type</span>
          <select
            aria-describedby={accountTypeError ? 'update-bank-account-type-error' : undefined}
            aria-invalid={Boolean(accountTypeError)}
            className={selectClassName}
            disabled={isBusy}
            {...register('accountType')}
          >
            {bankAccountTypeOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
          {accountTypeError && (
            <p className={styles.errorMessage} id="update-bank-account-type-error">
              {accountTypeError}
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

export default UpdateBankForm
