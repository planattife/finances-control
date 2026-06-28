import { useCallback, useState } from 'react'
import { CardType } from '../../types/card'
import {
  createCreditCard,
  createDebitCard,
  type CreateCreditCardRequest,
  type CreateDebitCardRequest,
} from './cardsApi'

export type CreateCardRequest =
  | ({ cardType: typeof CardType.Debit } & CreateDebitCardRequest)
  | ({ cardType: typeof CardType.Credit } & CreateCreditCardRequest)

export function useCreateCard(accountId: number) {
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<unknown>(null)

  const createCard = useCallback(
    async (payload: CreateCardRequest) => {
      setIsLoading(true)
      setError(null)

      try {
        if (payload.cardType === CardType.Credit) {
          await createCreditCard(accountId, {
            cardNumber: payload.cardNumber,
            expirationDate: payload.expirationDate,
            cardNetwork: payload.cardNetwork,
            bankId: payload.bankId,
            cardName: payload.cardName,
            closingDay: payload.closingDay,
            dueDateDay: payload.dueDateDay,
          })
          return
        }

        await createDebitCard(accountId, {
          cardNumber: payload.cardNumber,
          expirationDate: payload.expirationDate,
          cardNetwork: payload.cardNetwork,
          bankId: payload.bankId,
          cardName: payload.cardName,
        })
      } catch (createError) {
        setError(createError)
        throw createError
      } finally {
        setIsLoading(false)
      }
    },
    [accountId],
  )

  return {
    createCard,
    error,
    isLoading,
  }
}
