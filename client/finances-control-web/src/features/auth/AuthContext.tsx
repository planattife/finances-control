import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { getCurrentUser } from './authApi'
import type { CurrentUser } from '../../types/auth'

type AuthContextValue = {
  user: CurrentUser | null
  isCheckingAuth: boolean
  isAuthenticated: boolean
  refreshCurrentUser: () => Promise<CurrentUser>
  clearUser: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

type AuthProviderProps = {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<CurrentUser | null>(null)
  const [isCheckingAuth, setIsCheckingAuth] = useState(true)

  const refreshCurrentUser = useCallback(async () => {
    const currentUser = await getCurrentUser()
    setUser(currentUser)
    return currentUser
  }, [])

  const clearUser = useCallback(() => {
    setUser(null)
  }, [])

  useEffect(() => {
    let isMounted = true

    async function checkCurrentUser() {
      try {
        const currentUser = await getCurrentUser()

        if (isMounted) {
          setUser(currentUser)
        }
      } catch {
        if (isMounted) {
          setUser(null)
        }
      } finally {
        if (isMounted) {
          setIsCheckingAuth(false)
        }
      }
    }

    checkCurrentUser()

    return () => {
      isMounted = false
    }
  }, [])

  const value = useMemo(
    () => ({
      user,
      isCheckingAuth,
      isAuthenticated: user !== null,
      refreshCurrentUser,
      clearUser,
    }),
    [clearUser, isCheckingAuth, refreshCurrentUser, user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider.')
  }

  return context
}
