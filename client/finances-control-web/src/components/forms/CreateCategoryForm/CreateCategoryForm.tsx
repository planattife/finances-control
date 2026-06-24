import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import ActionButton from '../../ActionButton'
import styles from './CreateCategoryForm.module.css'

const createCategoryFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Category name is required.')
    .min(2, 'Category name must be at least 2 characters.')
    .max(50, 'Category name must be at most 50 characters.'),
})

type CreateCategoryFormInput = z.input<typeof createCategoryFormSchema>

export type CreateCategoryFormData = z.output<typeof createCategoryFormSchema>

type CreateCategoryFormProps = {
  isLoading?: boolean
  onCancel: () => void
  onSubmit: (data: CreateCategoryFormData) => Promise<void> | void
}

function CreateCategoryForm({ isLoading = false, onCancel, onSubmit }: CreateCategoryFormProps) {
  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
  } = useForm<CreateCategoryFormInput, unknown, CreateCategoryFormData>({
    resolver: zodResolver(createCategoryFormSchema),
    defaultValues: {
      name: '',
    },
  })

  const isBusy = isLoading || isSubmitting
  const nameError = errors.name?.message
  const nameInputClassName = `${styles.textInput} ${nameError ? styles.textInputError : ''}`.trim()

  async function handleFormSubmit(data: CreateCategoryFormData) {
    await onSubmit(data)
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit(handleFormSubmit)} noValidate>
      <label className={styles.field}>
        <span className={styles.fieldLabel}>Category Name</span>
        <input
          aria-describedby={nameError ? 'create-category-name-error' : undefined}
          aria-invalid={Boolean(nameError)}
          className={nameInputClassName}
          disabled={isBusy}
          maxLength={50}
          minLength={2}
          type="text"
          {...register('name')}
        />
        {nameError && (
          <p className={styles.errorMessage} id="create-category-name-error">
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

export default CreateCategoryForm
