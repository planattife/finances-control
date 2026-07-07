import { useEffect, type ChangeEvent } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { z } from 'zod'
import type { Bank } from '../../../types/bank'
import { CardType, type Card } from '../../../types/card'
import type { Category } from '../../../types/category'
import {
  PaymentMethodCode,
  paymentMethodOptions,
  requiresBank,
  requiresCard,
  TransactionTypeCode,
  transactionTypeOptions,
} from '../../../types/transaction'
import ActionButton from '../../ActionButton'
import styles from './CreateTransactionForm.module.css'

const LOAD_MORE_CATEGORIES_VALUE = '__load_more_categories__'
const LOAD_MORE_BANKS_VALUE = '__load_more_banks__'
const LOAD_MORE_CARDS_VALUE = '__load_more_cards__'

function nullableNumber(value: unknown) {
  if (value === '' || value === null || value === undefined) {
    return null
  }

  return Number(value)
}

const createTransactionFormSchema = z
  .object({
    amount: z.coerce
      .number({ error: 'Amount is required.' })
      .min(0, 'Amount cannot be less than 0.'),
    paymentMethod: z.coerce
      .number({ error: 'Payment method is required.' })
      .pipe(z.union([
        z.literal(PaymentMethodCode.Credit),
        z.literal(PaymentMethodCode.Debit),
        z.literal(PaymentMethodCode.Pix),
        z.literal(PaymentMethodCode.BankSlip),
        z.literal(PaymentMethodCode.Cash),
        z.literal(PaymentMethodCode.BankTransfer),
      ])),
    transactionType: z.coerce
      .number({ error: 'Transaction type is required.' })
      .pipe(z.union([z.literal(TransactionTypeCode.Expense), z.literal(TransactionTypeCode.Income)])),
    categoryId: z.coerce.number({ error: 'Category is required.' }).min(1, 'Category is required.'),
    transactionDate: z.string().min(1, 'Transaction date is required.'),
    description: z.string().max(500, 'Description must be at most 500 characters.'),
    cardId: z.preprocess(nullableNumber, z.number().nullable()),
    bankId: z.preprocess(nullableNumber, z.number().nullable()),
  })
  .superRefine((data, context) => {
    if (requiresCard(data.paymentMethod) && (!data.cardId || data.cardId <= 0)) {
      context.addIssue({
        code: 'custom',
        message: 'Card is required for this payment method.',
        path: ['cardId'],
      })
    }

    if (requiresBank(data.paymentMethod) && (!data.bankId || data.bankId <= 0)) {
      context.addIssue({
        code: 'custom',
        message: 'Bank is required for this payment method.',
        path: ['bankId'],
      })
    }
  })

type CreateTransactionFormInput = z.input<typeof createTransactionFormSchema>

export type CreateTransactionFormData = z.output<typeof createTransactionFormSchema>

type CreateTransactionFormProps = {
  banks: Bank[]
  cards: Card[]
  categories: Category[]
  defaultValues: CreateTransactionFormData
  hasMoreBanks: boolean
  hasMoreCards: boolean
  hasMoreCategories: boolean
  isLoading?: boolean
  isLoadingBanks?: boolean
  isLoadingCards?: boolean
  isLoadingCategories?: boolean
  isLoadingMoreBanks?: boolean
  isLoadingMoreCards?: boolean
  isLoadingMoreCategories?: boolean
  onCancel: () => void
  onLoadMoreBanks: () => Promise<void> | void
  onLoadMoreCards: () => Promise<void> | void
  onLoadMoreCategories: () => Promise<void> | void
  onSubmit: (data: CreateTransactionFormData) => Promise<void> | void
}

function getInputClassName(baseClassName: string, errorClassName: string, hasError: boolean) {
  return `${baseClassName} ${hasError ? errorClassName : ''}`.trim()
}

function isActiveCard(card: Card) {
  return card.status.toLowerCase() === 'active'
}

function getCardLabel(card: Card) {
  return card.cardName ?? card.cardNumber
}

function normalizePaymentMethod(value: unknown) {
  return Number(value) as PaymentMethodCode
}

function CreateTransactionForm({
  banks,
  cards,
  categories,
  defaultValues,
  hasMoreBanks,
  hasMoreCards,
  hasMoreCategories,
  isLoading = false,
  isLoadingBanks = false,
  isLoadingCards = false,
  isLoadingCategories = false,
  isLoadingMoreBanks = false,
  isLoadingMoreCards = false,
  isLoadingMoreCategories = false,
  onCancel,
  onLoadMoreBanks,
  onLoadMoreCards,
  onLoadMoreCategories,
  onSubmit,
}: CreateTransactionFormProps) {
  const {
    control,
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
    reset,
    setValue,
  } = useForm<CreateTransactionFormInput, unknown, CreateTransactionFormData>({
    resolver: zodResolver(createTransactionFormSchema),
    defaultValues,
  })
  const selectedPaymentMethodValue = useWatch({ control, name: 'paymentMethod' })
  const selectedPaymentMethod = normalizePaymentMethod(selectedPaymentMethodValue)
  const shouldShowCard = requiresCard(selectedPaymentMethod)
  const shouldShowBank = requiresBank(selectedPaymentMethod)
  const isBusy = isLoading || isSubmitting
  const isCategorySelectDisabled = isBusy || isLoadingCategories || (categories.length === 0 && !hasMoreCategories)
  const isBankSelectDisabled = isBusy || isLoadingBanks || (banks.length === 0 && !hasMoreBanks)
  const filteredCards = cards.filter((card) => {
    if (!isActiveCard(card)) {
      return false
    }

    if (selectedPaymentMethod === PaymentMethodCode.Credit) {
      return card.cardType === CardType.Credit
    }

    if (selectedPaymentMethod === PaymentMethodCode.Debit) {
      return card.cardType === CardType.Debit
    }

    return false
  })
  const isCardSelectDisabled = isBusy || isLoadingCards || (filteredCards.length === 0 && !hasMoreCards)

  useEffect(() => {
    reset(defaultValues)
  }, [defaultValues, reset])

  useEffect(() => {
    if (!shouldShowCard) {
      setValue('cardId', null)
    }

    if (!shouldShowBank) {
      setValue('bankId', null)
    }
  }, [setValue, shouldShowBank, shouldShowCard])

  async function handleFormSubmit(data: CreateTransactionFormData) {
    await onSubmit(data)
  }

  return (
    <div className={styles.wrapper}>
      <form className={styles.form} onSubmit={handleSubmit(handleFormSubmit)} noValidate>
        <div className={styles.inlineFields}>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>Amount</span>
            <input
              aria-describedby={errors.amount ? 'create-transaction-amount-error' : undefined}
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
              <p className={styles.errorMessage} id="create-transaction-amount-error">
                {errors.amount.message}
              </p>
            )}
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>Transaction Date</span>
            <input
              aria-describedby={errors.transactionDate ? 'create-transaction-date-error' : undefined}
              aria-invalid={Boolean(errors.transactionDate)}
              className={getInputClassName(
                styles.textInput,
                styles.textInputError,
                Boolean(errors.transactionDate),
              )}
              disabled={isBusy}
              type="date"
              {...register('transactionDate')}
            />
            {errors.transactionDate?.message && (
              <p className={styles.errorMessage} id="create-transaction-date-error">
                {errors.transactionDate.message}
              </p>
            )}
          </label>
        </div>

        <div className={styles.inlineFields}>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>Payment Method</span>
            <select
              aria-describedby={errors.paymentMethod ? 'create-transaction-payment-method-error' : undefined}
              aria-invalid={Boolean(errors.paymentMethod)}
              className={getInputClassName(
                styles.selectInput,
                styles.selectInputError,
                Boolean(errors.paymentMethod),
              )}
              disabled={isBusy}
              {...register('paymentMethod')}
            >
              {paymentMethodOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
            {errors.paymentMethod?.message && (
              <p className={styles.errorMessage} id="create-transaction-payment-method-error">
                {errors.paymentMethod.message}
              </p>
            )}
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>Transaction Type</span>
            <select
              aria-describedby={errors.transactionType ? 'create-transaction-type-error' : undefined}
              aria-invalid={Boolean(errors.transactionType)}
              className={getInputClassName(
                styles.selectInput,
                styles.selectInputError,
                Boolean(errors.transactionType),
              )}
              disabled={isBusy}
              {...register('transactionType')}
            >
              {transactionTypeOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
            {errors.transactionType?.message && (
              <p className={styles.errorMessage} id="create-transaction-type-error">
                {errors.transactionType.message}
              </p>
            )}
          </label>
        </div>

        <label className={styles.field}>
          <span className={styles.fieldLabel}>Category</span>
          <Controller
            control={control}
            name="categoryId"
            render={({ field }) => (
              <select
                aria-describedby={errors.categoryId ? 'create-transaction-category-error' : undefined}
                aria-invalid={Boolean(errors.categoryId)}
                className={getInputClassName(
                  styles.selectInput,
                  styles.selectInputError,
                  Boolean(errors.categoryId),
                )}
                disabled={isCategorySelectDisabled}
                onChange={async (event: ChangeEvent<HTMLSelectElement>) => {
                  if (event.target.value === LOAD_MORE_CATEGORIES_VALUE) {
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
                  <option value={LOAD_MORE_CATEGORIES_VALUE}>
                    {isLoadingMoreCategories ? 'Loading more categories...' : 'Load more categories...'}
                  </option>
                )}
              </select>
            )}
          />
          {errors.categoryId?.message && (
            <p className={styles.errorMessage} id="create-transaction-category-error">
              {errors.categoryId.message}
            </p>
          )}
        </label>

        {shouldShowCard && (
          <label className={styles.field}>
            <span className={styles.fieldLabel}>Card</span>
            <Controller
              control={control}
              name="cardId"
              render={({ field }) => (
                <select
                  aria-describedby={errors.cardId ? 'create-transaction-card-error' : undefined}
                  aria-invalid={Boolean(errors.cardId)}
                  className={getInputClassName(
                    styles.selectInput,
                    styles.selectInputError,
                    Boolean(errors.cardId),
                  )}
                  disabled={isCardSelectDisabled}
                  onChange={async (event: ChangeEvent<HTMLSelectElement>) => {
                    if (event.target.value === LOAD_MORE_CARDS_VALUE) {
                      await onLoadMoreCards()
                      return
                    }

                    field.onChange(Number(event.target.value))
                  }}
                  value={field.value === null ? '' : String(field.value)}
                >
                  {isLoadingCards && <option value="">Loading cards...</option>}
                  {!isLoadingCards && filteredCards.length === 0 && (
                    <option value="">No cards found</option>
                  )}
                  <option value="">Select a card</option>
                  {filteredCards.map((card) => (
                    <option key={card.id} value={card.id}>
                      {getCardLabel(card)}
                    </option>
                  ))}
                  {hasMoreCards && (
                    <option value={LOAD_MORE_CARDS_VALUE}>
                      {isLoadingMoreCards ? 'Loading more cards...' : 'Load more cards...'}
                    </option>
                  )}
                </select>
              )}
            />
            {errors.cardId?.message && (
              <p className={styles.errorMessage} id="create-transaction-card-error">
                {errors.cardId.message}
              </p>
            )}
          </label>
        )}

        {shouldShowBank && (
          <label className={styles.field}>
            <span className={styles.fieldLabel}>Bank</span>
            <Controller
              control={control}
              name="bankId"
              render={({ field }) => (
                <select
                  aria-describedby={errors.bankId ? 'create-transaction-bank-error' : undefined}
                  aria-invalid={Boolean(errors.bankId)}
                  className={getInputClassName(
                    styles.selectInput,
                    styles.selectInputError,
                    Boolean(errors.bankId),
                  )}
                  disabled={isBankSelectDisabled}
                  onChange={async (event: ChangeEvent<HTMLSelectElement>) => {
                    if (event.target.value === LOAD_MORE_BANKS_VALUE) {
                      await onLoadMoreBanks()
                      return
                    }

                    field.onChange(Number(event.target.value))
                  }}
                  value={field.value === null ? '' : String(field.value)}
                >
                  {isLoadingBanks && <option value="">Loading banks...</option>}
                  {!isLoadingBanks && banks.length === 0 && <option value="">No banks found</option>}
                  <option value="">Select a bank</option>
                  {banks.map((bank) => (
                    <option key={bank.id} value={bank.id}>
                      {bank.name}
                    </option>
                  ))}
                  {hasMoreBanks && banks.length > 0 && (
                    <option value={LOAD_MORE_BANKS_VALUE}>
                      {isLoadingMoreBanks ? 'Loading more banks...' : 'Load more banks...'}
                    </option>
                  )}
                </select>
              )}
            />
            {errors.bankId?.message && (
              <p className={styles.errorMessage} id="create-transaction-bank-error">
                {errors.bankId.message}
              </p>
            )}
          </label>
        )}

        <label className={styles.field}>
          <span className={styles.fieldLabel}>Description</span>
          <textarea
            aria-describedby={errors.description ? 'create-transaction-description-error' : undefined}
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
            <p className={styles.errorMessage} id="create-transaction-description-error">
              {errors.description.message}
            </p>
          )}
        </label>

        <div className={styles.actions}>
          <ActionButton disabled={isBusy} onClick={onCancel} type="button">
            Cancel
          </ActionButton>
          <ActionButton
            disabled={isBusy || isLoadingCategories}
            type="submit"
            variant="primary"
          >
            {isBusy ? 'Creating...' : 'Create'}
          </ActionButton>
        </div>
      </form>
    </div>
  )
}

export default CreateTransactionForm
