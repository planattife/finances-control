import { useCallback, useState } from 'react'
import { activateAccount, deactivateAccount } from './accountsApi'

export type AccountStatusChange = {
  accountId: number
  nextStatus: 'active' | 'inactive'
}

export function useChangeAccountStatus() {
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<unknown>(null)

  const changeAccountStatus = useCallback(async ({ accountId, nextStatus }: AccountStatusChange) => {
    setIsLoading(true)
    setError(null)

    try {
      if (nextStatus === 'active') {
        await activateAccount(accountId)
      } else {
        await deactivateAccount(accountId)
      }
    } catch (changeError) {
      setError(changeError)
      throw changeError
    } finally {
      setIsLoading(false)
    }
  }, [])

  return {
    changeAccountStatus,
    error,
    isLoading,
  }
}
