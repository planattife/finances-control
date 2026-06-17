import { useEffect, useState } from 'react'
import type { AccountDetails, AccountDetailsResponse } from '../../types/account'
import { getAccountDetails } from './accountsApi'

export function useAccountDetails(accountId: number) {
  const [accountDetails, setAccountDetails] = useState<AccountDetails | null>(null)
  const [response, setResponse] = useState<AccountDetailsResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<unknown>(null)

  useEffect(() => {
    let isActive = true

    async function loadAccountDetails() {
      setIsLoading(true)
      setError(null)

      try {
        const nextResponse = await getAccountDetails(accountId)

        if (!isActive) {
          return
        }

        setAccountDetails(nextResponse.data)
        setResponse(nextResponse)
      } catch (loadError) {
        if (!isActive) {
          return
        }

        setAccountDetails(null)
        setResponse(null)
        setError(loadError)
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadAccountDetails()

    return () => {
      isActive = false
    }
  }, [accountId])

  return {
    accountDetails,
    error,
    errors: response?.errors ?? [],
    isLoading,
  }
}
