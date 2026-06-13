import type { ChangeEvent, FC } from 'react'
import type { AxiosError } from 'axios'
import { useNavigate } from 'react-router-dom'
import { useLogout } from '../../features/auth/useLogout'
import { useAuth } from '../../features/auth/useAuth'
import type { ApiErrorResponse } from '../../types/auth'
import styles from './Topbar.module.css'

type TopbarProps = Record<string, never>

const Topbar: FC<TopbarProps> = () => {
  const navigate = useNavigate()
  const logoutMutation = useLogout()
  const { clearUser, user } = useAuth()

  function handleAccountChange(event: ChangeEvent<HTMLSelectElement>) {
    console.info('Selected account placeholder:', event.target.value)
  }

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
    <header className={styles.topbar}>
      <label className={styles.accountSelector}>
        <span className={styles.accountSelectorLabel}>Account</span>
        <select className={styles.accountSelect} defaultValue="all" onChange={handleAccountChange}>
          <option value="all">All accounts</option>
          <option value="checking">Checking account</option>
          <option value="savings">Savings account</option>
        </select>
      </label>

      <div className={styles.userActions}>
        <span className={styles.username}>{user?.email ?? 'User'}</span>
        <button
          aria-label="Sign out"
          className={styles.logoutButton}
          disabled={logoutMutation.isLoading}
          onClick={handleLogout}
          type="button"
        >
          {logoutMutation.isLoading ? 'Signing out' : 'Logout'}
        </button>
      </div>
    </header>
  )
}

export default Topbar
