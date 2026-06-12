import type { AxiosError } from 'axios'
import { useNavigate } from 'react-router-dom'
import Button from '../../components/Button'
import { useAuth } from '../../features/auth/AuthContext'
import { useLogout } from '../../features/auth/useLogout'
import type { ApiErrorResponse } from '../../types/auth'

function HomePage() {
  const navigate = useNavigate()
  const logoutMutation = useLogout()
  const { clearUser } = useAuth()

  async function handleLogout() {
    try {
      await logoutMutation.mutateAsync()
      clearUser()
      navigate('/')
    } catch (error) {
      const logoutError = error as AxiosError<ApiErrorResponse>
      alert(logoutError.response?.data?.detail ?? logoutError.message ?? 'Logout failed')
    }
  }

  return (
    <main>
      <h1>home</h1>
      <Button onClick={handleLogout} disabled={logoutMutation.isLoading}>
        {logoutMutation.isLoading ? 'Signing Out...' : 'Sign Out'}
      </Button>
    </main>
  )
}

export default HomePage
