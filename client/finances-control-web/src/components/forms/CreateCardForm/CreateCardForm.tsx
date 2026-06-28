import { zodResolver } from '@hookform/resolvers/zod'
import { useMemo, type ChangeEvent } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { z } from 'zod'
import ActionButton from '../../ActionButton'
import { usePaginatedBanks } from '../../../features/banks/usePaginatedBanks'
import {
  CardNetwork,
  cardNetworkOptions,
  CardType,
  cardTypeOptions,
} from '../../../types/card'
import styles from './CreateCardForm.module.css'

const LOAD_MORE_BANKS_VALUE = '__load_more_banks__'

const createCardFormSchema = z
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
    cardNumber: z
      .string()
      .trim()
      .min(1, 'Card number is required.')
      .min(4, 'Card number must be at least 4 characters.')
      .max(19, 'Card number must be at most 19 characters.'),
    expirationDate: z
      .string()
      .min(1, 'Expiration date is required.')
      .transform((value) => new Date(`${value}T00:00:00.000Z`).toISOString()),
    cardNetwork: z.coerce.number({ error: 'Card network is required.' }).pipe(
      z.union([
        z.literal(CardNetwork.Visa),
        z.literal(CardNetwork.Mastercard),
        z.literal(CardNetwork.Elo),
        z.literal(CardNetwork.AmericanExpress),
        z.literal(CardNetwork.Hipercard),
      ]),
    ),
    bankId: z.coerce.number({ error: 'Bank is required.' }).int().positive('Bank is required.'),
    closingDay: z.coerce.number().optional(),
    dueDateDay: z.coerce.number().optional(),
  })
  .superRefine((data, context) => {
    if (data.cardType !== CardType.Credit) {
      return
    }

    const closingDay = data.closingDay
    const dueDateDay = data.dueDateDay

    if (!Number.isInteger(closingDay) || closingDay === undefined || closingDay < 1 || closingDay > 30) {
      context.addIssue({
        code: 'custom',
        message: 'Closing day must be a number from 1 to 30.',
        path: ['closingDay'],
      })
    }

    if (!Number.isInteger(dueDateDay) || dueDateDay === undefined || dueDateDay < 1 || dueDateDay > 30) {
      context.addIssue({
        code: 'custom',
        message: 'Due date day must be a number from 1 to 30.',
        path: ['dueDateDay'],
      })
    }
  })

type CreateCardFormInput = z.input<typeof createCardFormSchema>

export type CreateCardFormData = z.output<typeof createCardFormSchema>

type CreateCardFormProps = {
  isLoading?: boolean
  onCancel: () => void
  onSubmit: (data: CreateCardFormData) => Promise<void> | void
}

function getInputClassName(hasError: boolean) {
  return `${styles.textInput} ${hasError ? styles.textInputError : ''}`.trim()
}

function CreateCardForm({ isLoading = false, onCancel, onSubmit }: CreateCardFormProps) {
  const {
    banks,
    error: banksError,
    hasNextPage,
    isLoading: isLoadingBanks,
    isLoadingMore,
    loadNextPage,
  } = usePaginatedBanks({
    pageSize: 5,
    initialPage: 1,
  })
  const {
    formState: { errors, isSubmitting },
    control,
    handleSubmit,
    register,
    setValue,
  } = useForm<CreateCardFormInput, unknown, CreateCardFormData>({
    resolver: zodResolver(createCardFormSchema),
    defaultValues: {
      cardType: CardType.Debit,
      cardName: '',
      cardNumber: '',
      expirationDate: '',
      cardNetwork: CardNetwork.Visa,
      bankId: 0,
      closingDay: undefined,
      dueDateDay: undefined,
    },
  })
  const selectedCardType = useWatch({ control, name: 'cardType' })
  const isCreditCard = Number(selectedCardType) === CardType.Credit
  const isBusy = isLoading || isSubmitting
  const isBankSelectDisabled =
    isBusy || isLoadingBanks || isLoadingMore || Boolean(banksError) || banks.length === 0
  const bankOptions = useMemo(() => banks, [banks])

  async function handleBankChange(event: ChangeEvent<HTMLSelectElement>) {
    const selectedValue = event.target.value

    if (selectedValue === LOAD_MORE_BANKS_VALUE) {
      await loadNextPage()
      return
    }

    setValue('bankId', Number(selectedValue), { shouldDirty: true, shouldValidate: true })
  }

  async function handleFormSubmit(data: CreateCardFormData) {
    await onSubmit(data)
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit(handleFormSubmit)} noValidate>
      <label className={styles.field}>
        <span className={styles.fieldLabel}>Card Type</span>
        <select
          aria-describedby={errors.cardType ? 'create-card-type-error' : undefined}
          aria-invalid={Boolean(errors.cardType)}
          className={getInputClassName(Boolean(errors.cardType))}
          disabled={isBusy}
          {...register('cardType')}
        >
          {cardTypeOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
        {errors.cardType?.message && (
          <p className={styles.errorMessage} id="create-card-type-error">
            {errors.cardType.message}
          </p>
        )}
      </label>

      <label className={styles.field}>
        <span className={styles.fieldLabel}>Card Name</span>
        <input
          aria-describedby={errors.cardName ? 'create-card-name-error' : undefined}
          aria-invalid={Boolean(errors.cardName)}
          className={getInputClassName(Boolean(errors.cardName))}
          disabled={isBusy}
          maxLength={50}
          minLength={2}
          type="text"
          {...register('cardName')}
        />
        {errors.cardName?.message && (
          <p className={styles.errorMessage} id="create-card-name-error">
            {errors.cardName.message}
          </p>
        )}
      </label>

      <label className={styles.field}>
        <span className={styles.fieldLabel}>Card Number</span>
        <input
          aria-describedby={errors.cardNumber ? 'create-card-number-error' : undefined}
          aria-invalid={Boolean(errors.cardNumber)}
          className={getInputClassName(Boolean(errors.cardNumber))}
          disabled={isBusy}
          maxLength={19}
          minLength={4}
          type="text"
          {...register('cardNumber')}
        />
        {errors.cardNumber?.message && (
          <p className={styles.errorMessage} id="create-card-number-error">
            {errors.cardNumber.message}
          </p>
        )}
      </label>

      <label className={styles.field}>
        <span className={styles.fieldLabel}>Expiration Date</span>
        <input
          aria-describedby={errors.expirationDate ? 'create-card-expiration-date-error' : undefined}
          aria-invalid={Boolean(errors.expirationDate)}
          className={getInputClassName(Boolean(errors.expirationDate))}
          disabled={isBusy}
          type="date"
          {...register('expirationDate')}
        />
        {errors.expirationDate?.message && (
          <p className={styles.errorMessage} id="create-card-expiration-date-error">
            {errors.expirationDate.message}
          </p>
        )}
      </label>

      <label className={styles.field}>
        <span className={styles.fieldLabel}>Card Network</span>
        <select
          aria-describedby={errors.cardNetwork ? 'create-card-network-error' : undefined}
          aria-invalid={Boolean(errors.cardNetwork)}
          className={getInputClassName(Boolean(errors.cardNetwork))}
          disabled={isBusy}
          {...register('cardNetwork')}
        >
          {cardNetworkOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
        {errors.cardNetwork?.message && (
          <p className={styles.errorMessage} id="create-card-network-error">
            {errors.cardNetwork.message}
          </p>
        )}
      </label>

      <label className={styles.field}>
        <span className={styles.fieldLabel}>Bank</span>
        <select
          aria-describedby={errors.bankId ? 'create-card-bank-error' : undefined}
          aria-invalid={Boolean(errors.bankId)}
          className={getInputClassName(Boolean(errors.bankId))}
          disabled={isBankSelectDisabled}
          {...register('bankId', { onChange: handleBankChange })}
        >
          {isLoadingBanks && <option value="">Loading banks...</option>}
          {!isLoadingBanks && Boolean(banksError) && <option value="">Could not load banks</option>}
          {!isLoadingBanks && !banksError && bankOptions.length === 0 && (
            <option value="">No banks found</option>
          )}
          {bankOptions.length > 0 && <option value="">Select a bank</option>}
          {bankOptions.map((bank) => (
            <option key={bank.id} value={bank.id}>
              {bank.name}
            </option>
          ))}
          {hasNextPage && !banksError && bankOptions.length > 0 && (
            <option value={LOAD_MORE_BANKS_VALUE}>
              {isLoadingMore ? 'Loading more banks...' : 'Load more banks...'}
            </option>
          )}
        </select>
        {errors.bankId?.message && (
          <p className={styles.errorMessage} id="create-card-bank-error">
            {errors.bankId.message}
          </p>
        )}
      </label>

      {isCreditCard && (
        <div className={styles.inlineFields}>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>Closing Day</span>
            <input
              aria-describedby={errors.closingDay ? 'create-card-closing-day-error' : undefined}
              aria-invalid={Boolean(errors.closingDay)}
              className={getInputClassName(Boolean(errors.closingDay))}
              disabled={isBusy}
              max={30}
              min={1}
              type="number"
              {...register('closingDay')}
            />
            {errors.closingDay?.message && (
              <p className={styles.errorMessage} id="create-card-closing-day-error">
                {errors.closingDay.message}
              </p>
            )}
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>Due Date Day</span>
            <input
              aria-describedby={errors.dueDateDay ? 'create-card-due-date-day-error' : undefined}
              aria-invalid={Boolean(errors.dueDateDay)}
              className={getInputClassName(Boolean(errors.dueDateDay))}
              disabled={isBusy}
              max={30}
              min={1}
              type="number"
              {...register('dueDateDay')}
            />
            {errors.dueDateDay?.message && (
              <p className={styles.errorMessage} id="create-card-due-date-day-error">
                {errors.dueDateDay.message}
              </p>
            )}
          </label>
        </div>
      )}

      <div className={styles.actions}>
        <ActionButton disabled={isBusy} onClick={onCancel} type="button">
          Cancel
        </ActionButton>
        <ActionButton disabled={isBusy || isBankSelectDisabled} type="submit" variant="primary">
          {isBusy ? 'Creating...' : 'Create'}
        </ActionButton>
      </div>
    </form>
  )
}

export default CreateCardForm
