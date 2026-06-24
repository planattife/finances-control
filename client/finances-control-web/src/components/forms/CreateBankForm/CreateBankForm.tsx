import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import ActionButton from '../../ActionButton'
import { bankAccountTypeOptions, BankAccountType } from '../../../types/bank'
import styles from './CreateBankForm.module.css'

const createBankFormSchema = z.object({
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

type CreateBankFormInput = z.input<typeof createBankFormSchema>

export type CreateBankFormData = z.output<typeof createBankFormSchema>

type CreateBankFormProps = {
  isLoading?: boolean
  onCancel: () => void
  onSubmit: (data: CreateBankFormData) => Promise<void> | void
}

function CreateBankForm({ isLoading = false, onCancel, onSubmit }: CreateBankFormProps) {
  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
  } = useForm<CreateBankFormInput, unknown, CreateBankFormData>({
    resolver: zodResolver(createBankFormSchema),
    defaultValues: {
      name: '',
      accountType: BankAccountType.Savings,
    },
  })

  const isBusy = isLoading || isSubmitting
  const nameError = errors.name?.message
  const accountTypeError = errors.accountType?.message
  const nameInputClassName = `${styles.textInput} ${nameError ? styles.textInputError : ''}`.trim()
  const selectClassName = `${styles.textInput} ${accountTypeError ? styles.textInputError : ''}`.trim()

  async function handleFormSubmit(data: CreateBankFormData) {
    await onSubmit(data)
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit(handleFormSubmit)} noValidate>
      <label className={styles.field}>
        <span className={styles.fieldLabel}>Bank Name</span>
        <input
          aria-describedby={nameError ? 'create-bank-name-error' : undefined}
          aria-invalid={Boolean(nameError)}
          className={nameInputClassName}
          disabled={isBusy}
          maxLength={50}
          minLength={2}
          type="text"
          {...register('name')}
        />
        {nameError && (
          <p className={styles.errorMessage} id="create-bank-name-error">
            {nameError}
          </p>
        )}
      </label>

      <label className={styles.field}>
        <span className={styles.fieldLabel}>Account Type</span>
        <select
          aria-describedby={accountTypeError ? 'create-bank-account-type-error' : undefined}
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
          <p className={styles.errorMessage} id="create-bank-account-type-error">
            {accountTypeError}
          </p>
        )}
      </label>

      <div className={styles.actions}>
        <ActionButton disabled={isBusy} onClick={onCancel} type="button">
          Cancel
        </ActionButton>
        <ActionButton disabled={isBusy} type="submit" variant="primary">
          {isBusy ? 'Creating...' : 'Create'}
        </ActionButton>
      </div>
    </form>
  )
}

export default CreateBankForm
