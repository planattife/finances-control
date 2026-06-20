import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import ActionButton from '../../ActionButton'
import styles from './CreateAccountForm.module.css'

const createAccountFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Account name is required.')
    .min(2, 'Account name must be at least 2 characters.')
    .max(50, 'Account name must be at most 50 characters.'),
})

export type CreateAccountFormData = z.infer<typeof createAccountFormSchema>

type CreateAccountFormProps = {
  isLoading?: boolean
  onCancel: () => void
  onSubmit: (data: CreateAccountFormData) => Promise<void> | void
}

function CreateAccountForm({ isLoading = false, onCancel, onSubmit }: CreateAccountFormProps) {
  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
  } = useForm<CreateAccountFormData>({
    resolver: zodResolver(createAccountFormSchema),
    defaultValues: {
      name: '',
    },
  })

  const isBusy = isLoading || isSubmitting
  const nameError = errors.name?.message
  const nameInputClassName = `${styles.textInput} ${nameError ? styles.textInputError : ''}`.trim()

  async function handleFormSubmit(data: CreateAccountFormData) {
    await onSubmit(data)
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit(handleFormSubmit)} noValidate>
      <label className={styles.field}>
        <span className={styles.fieldLabel}>Account Name</span>
        <input
          aria-describedby={nameError ? 'create-account-name-error' : undefined}
          aria-invalid={Boolean(nameError)}
          className={nameInputClassName}
          disabled={isBusy}
          maxLength={50}
          minLength={2}
          type="text"
          {...register('name')}
        />
        {nameError && (
          <p className={styles.errorMessage} id="create-account-name-error">
            {nameError}
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

export default CreateAccountForm
