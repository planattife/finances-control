import { useEffect, type ReactNode } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import {
  getCreditCardDayLimit,
  getCreditCardDayValidationMessage,
  isValidCreditCardDay,
} from '../../../features/cards/creditCardDayValidation'
import { CardType, type CardType as CardTypeValue } from '../../../types/card'
import ActionButton from '../../ActionButton'
import styles from './UpdateCardForm.module.css'

const updateCardFormSchema = z
  .object({
    cardType: z.coerce
      .number({ error: 'Card type is required.' })
      .pipe(z.union([z.literal(CardType.Debit), z.literal(CardType.Credit)])),
    cardName: z
      .string()
      .trim()
      .min(1, 'Card name is required.')
      .min(2, 'Card name must be at least 2 characters.')
      .max(50, 'Card name must be at most 50 characters.'),
    closingDay: z.coerce.number().optional(),
    dueDateDay: z.coerce.number().optional(),
  })
  .superRefine((data, context) => {
    if (data.cardType !== CardType.Credit) {
      return
    }

    if (!isValidCreditCardDay(data.closingDay)) {
      context.addIssue({
        code: 'custom',
        message: getCreditCardDayValidationMessage('Closing day'),
        path: ['closingDay'],
      })
    }

    if (!isValidCreditCardDay(data.dueDateDay)) {
      context.addIssue({
        code: 'custom',
        message: getCreditCardDayValidationMessage('Due date day'),
        path: ['dueDateDay'],
      })
    }
  })

type UpdateCardFormInput = z.input<typeof updateCardFormSchema>

export type UpdateCardFormData = z.output<typeof updateCardFormSchema>

type UpdateCardFormProps = {
  cardType: CardTypeValue
  defaultValues: UpdateCardFormData
  isLoading?: boolean
  onCancel: () => void
  onSubmit: (data: UpdateCardFormData) => Promise<void> | void
  statusAction?: ReactNode
}

function getInputClassName(hasError: boolean) {
  return `${styles.textInput} ${hasError ? styles.textInputError : ''}`.trim()
}

function UpdateCardForm({
  cardType,
  defaultValues,
  isLoading = false,
  onCancel,
  onSubmit,
  statusAction,
}: UpdateCardFormProps) {
  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
    reset,
  } = useForm<UpdateCardFormInput, unknown, UpdateCardFormData>({
    resolver: zodResolver(updateCardFormSchema),
    defaultValues,
  })
  const isCreditCard = cardType === CardType.Credit
  const isBusy = isLoading || isSubmitting
  const creditCardDayLimit = getCreditCardDayLimit()

  useEffect(() => {
    reset(defaultValues)
  }, [defaultValues, reset])

  async function handleFormSubmit(data: UpdateCardFormData) {
    await onSubmit(data)
  }

  return (
    <div className={styles.wrapper}>
      <form className={styles.form} onSubmit={handleSubmit(handleFormSubmit)} noValidate>
        <input type="hidden" {...register('cardType')} />

        <label className={styles.field}>
          <span className={styles.fieldLabel}>Card Name</span>
          <input
            aria-describedby={errors.cardName ? 'update-card-name-error' : undefined}
            aria-invalid={Boolean(errors.cardName)}
            className={getInputClassName(Boolean(errors.cardName))}
            disabled={isBusy}
            maxLength={50}
            minLength={2}
            type="text"
            {...register('cardName')}
          />
          {errors.cardName?.message && (
            <p className={styles.errorMessage} id="update-card-name-error">
              {errors.cardName.message}
            </p>
          )}
        </label>

        {isCreditCard && (
          <div className={styles.inlineFields}>
            <label className={styles.field}>
              <span className={styles.fieldLabel}>Closing Day</span>
              <input
                aria-describedby={errors.closingDay ? 'update-card-closing-day-error' : undefined}
                aria-invalid={Boolean(errors.closingDay)}
                className={getInputClassName(Boolean(errors.closingDay))}
                disabled={isBusy}
                max={creditCardDayLimit}
                min={1}
                type="number"
                {...register('closingDay')}
              />
              {errors.closingDay?.message && (
                <p className={styles.errorMessage} id="update-card-closing-day-error">
                  {errors.closingDay.message}
                </p>
              )}
            </label>

            <label className={styles.field}>
              <span className={styles.fieldLabel}>Due Date Day</span>
              <input
                aria-describedby={errors.dueDateDay ? 'update-card-due-date-day-error' : undefined}
                aria-invalid={Boolean(errors.dueDateDay)}
                className={getInputClassName(Boolean(errors.dueDateDay))}
                disabled={isBusy}
                max={creditCardDayLimit}
                min={1}
                type="number"
                {...register('dueDateDay')}
              />
              {errors.dueDateDay?.message && (
                <p className={styles.errorMessage} id="update-card-due-date-day-error">
                  {errors.dueDateDay.message}
                </p>
              )}
            </label>
          </div>
        )}

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

export default UpdateCardForm
