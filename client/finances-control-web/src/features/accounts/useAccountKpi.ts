import { useEffect, useState } from 'react'
import type { AccountKpi, AccountKpiResponse } from '../../types/account'
import { getAccountKpi } from './accountsApi'

export function useAccountKpi(accountId: number) {
  const [accountKpi, setAccountKpi] = useState<AccountKpi | null>(null)
  const [response, setResponse] = useState<AccountKpiResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<unknown>(null)

  useEffect(() => {
    let isActive = true

    async function loadAccountKpi() {
      setIsLoading(true)
      setError(null)

      try {
        const nextResponse = await getAccountKpi(accountId)

        if (!isActive) {
          return
        }

        setAccountKpi(nextResponse.data)
        setResponse(nextResponse)
      } catch (loadError) {
        if (!isActive) {
          return
        }

        setAccountKpi(null)
        setResponse(null)
        setError(loadError)
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadAccountKpi()

    return () => {
      isActive = false
    }
  }, [accountId])

  return {
    accountKpi,
    error,
    errors: response?.errors ?? [],
    isLoading,
  }
}
