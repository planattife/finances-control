import { useCallback, useState } from 'react'
import { activateCard, deactivateCard } from './cardsApi'

export type CardStatusChange = {
  accountId: number
  cardId: number
  nextStatus: 'active' | 'inactive'
}

export function useChangeCardStatus() {
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<unknown>(null)

  const changeCardStatus = useCallback(
    async ({ accountId, cardId, nextStatus }: CardStatusChange) => {
      setIsLoading(true)
      setError(null)

      try {
        if (nextStatus === 'active') {
          await activateCard(accountId, cardId)
        } else {
          await deactivateCard(accountId, cardId)
        }
      } catch (changeError) {
        setError(changeError)
        throw changeError
      } finally {
        setIsLoading(false)
      }
    },
    [],
  )

  return {
    changeCardStatus,
    error,
    isLoading,
  }
}
