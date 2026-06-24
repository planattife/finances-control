import { useEffect, type ReactNode } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import ActionButton from '../../ActionButton'
import styles from './UpdateCategoryForm.module.css'

const updateCategoryFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Category name is required.')
    .min(2, 'Category name must be at least 2 characters.')
    .max(50, 'Category name must be at most 50 characters.'),
})

type UpdateCategoryFormInput = z.input<typeof updateCategoryFormSchema>

export type UpdateCategoryFormData = z.output<typeof updateCategoryFormSchema>

type UpdateCategoryFormProps = {
  defaultValues: UpdateCategoryFormData
  isLoading?: boolean
  onCancel: () => void
  onSubmit: (data: UpdateCategoryFormData) => Promise<void> | void
  statusAction?: ReactNode
}

function UpdateCategoryForm({
  defaultValues,
  isLoading = false,
  onCancel,
  onSubmit,
  statusAction,
}: UpdateCategoryFormProps) {
  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
    reset,
  } = useForm<UpdateCategoryFormInput, unknown, UpdateCategoryFormData>({
    resolver: zodResolver(updateCategoryFormSchema),
    defaultValues,
  })

  useEffect(() => {
    reset(defaultValues)
  }, [defaultValues, reset])

  const isBusy = isLoading || isSubmitting
  const nameError = errors.name?.message
  const nameInputClassName = `${styles.textInput} ${nameError ? styles.textInputError : ''}`.trim()

  async function handleFormSubmit(data: UpdateCategoryFormData) {
    await onSubmit(data)
  }

  return (
    <div className={styles.wrapper}>
      <form className={styles.form} onSubmit={handleSubmit(handleFormSubmit)} noValidate>
        <label className={styles.field}>
          <span className={styles.fieldLabel}>Category Name</span>
          <input
            aria-describedby={nameError ? 'update-category-name-error' : undefined}
            aria-invalid={Boolean(nameError)}
            className={nameInputClassName}
            disabled={isBusy}
            maxLength={50}
            minLength={2}
            type="text"
            {...register('name')}
          />
          {nameError && (
            <p className={styles.errorMessage} id="update-category-name-error">
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

export default UpdateCategoryForm
