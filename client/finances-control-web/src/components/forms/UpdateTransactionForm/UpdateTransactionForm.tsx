import { useEffect, type ChangeEvent } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import type { Category } from '../../../types/category'
import ActionButton from '../../ActionButton'
import styles from './UpdateTransactionForm.module.css'

const LOAD_MORE_VALUE = '__load_more_categories__'

const updateTransactionFormSchema = z.object({
  amount: z.coerce
    .number({ error: 'Amount is required.' })
    .min(0, 'Amount cannot be less than 0.'),
  categoryId: z.coerce.number({ error: 'Category is required.' }).min(1, 'Category is required.'),
  description: z.string().max(500, 'Description must be at most 500 characters.'),
})

type UpdateTransactionFormInput = z.input<typeof updateTransactionFormSchema>

export type UpdateTransactionFormData = z.output<typeof updateTransactionFormSchema>

type UpdateTransactionFormProps = {
  categories: Category[]
  defaultValues: UpdateTransactionFormData
  hasMoreCategories: boolean
  isLoading?: boolean
  isLoadingCategories?: boolean
  isLoadingMoreCategories?: boolean
  onCancel: () => void
  onLoadMoreCategories: () => Promise<void> | void
  onSubmit: (data: UpdateTransactionFormData) => Promise<void> | void
}

function getInputClassName(baseClassName: string, errorClassName: string, hasError: boolean) {
  return `${baseClassName} ${hasError ? errorClassName : ''}`.trim()
}

function UpdateTransactionForm({
  categories,
  defaultValues,
  hasMoreCategories,
  isLoading = false,
  isLoadingCategories = false,
  isLoadingMoreCategories = false,
  onCancel,
  onLoadMoreCategories,
  onSubmit,
}: UpdateTransactionFormProps) {
  const {
    control,
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
    reset,
  } = useForm<UpdateTransactionFormInput, unknown, UpdateTransactionFormData>({
    resolver: zodResolver(updateTransactionFormSchema),
    defaultValues,
  })
  const isBusy = isLoading || isSubmitting
  const isCategorySelectDisabled = isBusy || isLoadingCategories || categories.length === 0

  useEffect(() => {
    reset(defaultValues)
  }, [defaultValues, reset])

  async function handleFormSubmit(data: UpdateTransactionFormData) {
    await onSubmit(data)
  }

  return (
    <div className={styles.wrapper}>
      <form className={styles.form} onSubmit={handleSubmit(handleFormSubmit)} noValidate>
        <label className={styles.field}>
          <span className={styles.fieldLabel}>Amount</span>
          <input
            aria-describedby={errors.amount ? 'update-transaction-amount-error' : undefined}
            aria-invalid={Boolean(errors.amount)}
            className={getInputClassName(
              styles.textInput,
              styles.textInputError,
              Boolean(errors.amount),
            )}
            disabled={isBusy}
            min={0}
            step="0.01"
            type="number"
            {...register('amount')}
          />
          {errors.amount?.message && (
            <p className={styles.errorMessage} id="update-transaction-amount-error">
              {errors.amount.message}
            </p>
          )}
        </label>

        <label className={styles.field}>
          <span className={styles.fieldLabel}>Category</span>
          <Controller
            control={control}
            name="categoryId"
            render={({ field }) => (
              <select
                aria-describedby={errors.categoryId ? 'update-transaction-category-error' : undefined}
                aria-invalid={Boolean(errors.categoryId)}
                className={getInputClassName(
                  styles.selectInput,
                  styles.selectInputError,
                  Boolean(errors.categoryId),
                )}
                disabled={isCategorySelectDisabled}
                onChange={async (event: ChangeEvent<HTMLSelectElement>) => {
                  if (event.target.value === LOAD_MORE_VALUE) {
                    await onLoadMoreCategories()
                    return
                  }

                  field.onChange(Number(event.target.value))
                }}
                value={String(field.value)}
              >
                {isLoadingCategories && <option value="">Loading categories...</option>}
                {!isLoadingCategories && categories.length === 0 && (
                  <option value="">No categories found</option>
                )}
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
                {hasMoreCategories && categories.length > 0 && (
                  <option value={LOAD_MORE_VALUE}>
                    {isLoadingMoreCategories ? 'Loading more categories...' : 'Load more categories...'}
                  </option>
                )}
              </select>
            )}
          />
          {errors.categoryId?.message && (
            <p className={styles.errorMessage} id="update-transaction-category-error">
              {errors.categoryId.message}
            </p>
          )}
        </label>

        <label className={styles.field}>
          <span className={styles.fieldLabel}>Description</span>
          <textarea
            aria-describedby={errors.description ? 'update-transaction-description-error' : undefined}
            aria-invalid={Boolean(errors.description)}
            className={getInputClassName(
              styles.textareaInput,
              styles.textareaInputError,
              Boolean(errors.description),
            )}
            disabled={isBusy}
            maxLength={500}
            {...register('description')}
          />
          {errors.description?.message && (
            <p className={styles.errorMessage} id="update-transaction-description-error">
              {errors.description.message}
            </p>
          )}
        </label>

        <div className={styles.actions}>
          <ActionButton disabled={isBusy} onClick={onCancel} type="button">
            Cancel
          </ActionButton>
          <ActionButton disabled={isBusy || isLoadingCategories} type="submit" variant="primary">
            {isBusy ? 'Updating...' : 'Update'}
          </ActionButton>
        </div>
      </form>
    </div>
  )
}

export default UpdateTransactionForm
