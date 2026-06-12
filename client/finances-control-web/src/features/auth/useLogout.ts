import { useState } from 'react'
import { logout } from './authApi'

export function useLogout() {
  const [isLoading, setIsLoading] = useState(false)

  async function mutateAsync() {
    setIsLoading(true)

    try {
      await logout()
    } finally {
      setIsLoading(false)
    }
  }

  return {
    isLoading,
    mutateAsync,
  }
}
