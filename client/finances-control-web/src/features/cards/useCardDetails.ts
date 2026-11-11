import { useCallback, useEffect, useState } from 'react'
import type {
  CardDetails,
  CardDetailsResponse,
  CardType,
  CreditCardDetails,
  CreditCardDetailsResponse,
} from '../../types/card'
import { CardType as CardTypeValue } from '../../types/card'
import { getCreditCardDetails, getDebitCardDetails } from './cardsApi'

type CardDetailsResult = CardDetails | CreditCardDetails
type CardDetailsResponseResult = CardDetailsResponse | CreditCardDetailsResponse

export function useCardDetails(accountId: number, cardId: number, cardType: CardType) {
  const [cardDetails, setCardDetails] = useState<CardDetailsResult | null>(null)
  const [response, setResponse] = useState<CardDetailsResponseResult | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<unknown>(null)

  const loadCardDetails = useCallback(async () => {
    if (cardType === CardTypeValue.Credit) {
      return getCreditCardDetails(accountId, cardId)
    }

    return getDebitCardDetails(accountId, cardId)
  }, [accountId, cardId, cardType])

  const reload = useCallback(async () => {
    setIsLoading(true)
    setError(null)

    try {
      const nextResponse = await loadCardDetails()

      setCardDetails(nextResponse.data)
      setResponse(nextResponse)
    } catch (loadError) {
      setCardDetails(null)
      setResponse(null)
      setError(loadError)
    } finally {
      setIsLoading(false)
    }
  }, [loadCardDetails])

  useEffect(() => {
    let isActive = true

    async function loadDetails() {
      setIsLoading(true)
      setError(null)

      try {
        const nextResponse = await loadCardDetails()

        if (!isActive) {
          return
        }

        setCardDetails(nextResponse.data)
        setResponse(nextResponse)
      } catch (loadError) {
        if (!isActive) {
          return
        }

        setCardDetails(null)
        setResponse(null)
        setError(loadError)
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadDetails()

    return () => {
      isActive = false
    }
  }, [loadCardDetails])

  return {
    cardDetails,
    error,
    errors: response?.errors ?? [],
    isLoading,
    reload,
  }
}
