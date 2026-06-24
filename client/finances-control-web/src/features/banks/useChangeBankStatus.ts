import { useCallback, useState } from 'react'
import { activateBank, deactivateBank } from './banksApi'

export type BankStatusChange = {
  bankId: number
  nextStatus: 'active' | 'inactive'
}

export function useChangeBankStatus() {
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<unknown>(null)

  const changeBankStatus = useCallback(async ({ bankId, nextStatus }: BankStatusChange) => {
    setIsLoading(true)
    setError(null)

    try {
      if (nextStatus === 'active') {
        await activateBank(bankId)
      } else {
        await deactivateBank(bankId)
      }
    } catch (changeError) {
      setError(changeError)
      throw changeError
    } finally {
      setIsLoading(false)
    }
  }, [])

  return {
    changeBankStatus,
    error,
    isLoading,
  }
}
