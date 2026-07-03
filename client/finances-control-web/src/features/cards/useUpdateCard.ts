import { useCallback, useState } from 'react'
import { CardType } from '../../types/card'
import {
  updateCardName,
  updateCreditCardDates,
  type UpdateCreditCardDatesRequest,
} from './cardsApi'

export type UpdateDebitCardRequest = {
  accountId: number
  cardId: number
  cardType: typeof CardType.Debit
  cardName: string
}

export type UpdateCreditCardRequest = {
  accountId: number
  cardId: number
  cardType: typeof CardType.Credit
  cardName: string
} & UpdateCreditCardDatesRequest

export type UpdateCardRequest = UpdateDebitCardRequest | UpdateCreditCardRequest

export function useUpdateCard() {
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<unknown>(null)

  const updateCard = useCallback(async (payload: UpdateCardRequest) => {
    setIsLoading(true)
    setError(null)

    try {
      if (payload.cardType === CardType.Credit) {
        await Promise.all([
          updateCardName(payload.accountId, payload.cardId, payload.cardName),
          updateCreditCardDates(payload.accountId, payload.cardId, {
            closingDay: payload.closingDay,
            dueDateDay: payload.dueDateDay,
          }),
        ])
        return
      }

      await updateCardName(payload.accountId, payload.cardId, payload.cardName)
    } catch (updateError) {
      setError(updateError)
      throw updateError
    } finally {
      setIsLoading(false)
    }
  }, [])

  return {
    error,
    isLoading,
    updateCard,
  }
}
