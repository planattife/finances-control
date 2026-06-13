import { useEffect, useRef, useState, type ChangeEvent, type FC } from 'react'
import {
  getStoredSelectedAccountId,
  storeSelectedAccountId,
} from '../../features/accounts/accountSelectionStorage'
import { usePaginatedAccounts } from '../../features/accounts/usePaginatedAccounts'
import type { Account } from '../../types/account'
import styles from './AccountDropdown.module.css'

type AccountDropdownProps = {
  onAccountChange?: (account: Account) => void
}

const LOAD_MORE_VALUE = '__load_more_accounts__'

const AccountDropdown: FC<AccountDropdownProps> = ({ onAccountChange }) => {
  const { accounts, error, hasNextPage, isLoading, isLoadingMore, loadNextPage, loadUntilAccount } =
    usePaginatedAccounts({
      pageSize: 5,
      initialPage: 1,
    })
  const [selectedAccountId, setSelectedAccountId] = useState<number | null>(null)
  const hasAttemptedRestoreRef = useRef(false)
  const selectedAccount = accounts.find((account) => account.id === selectedAccountId) ?? accounts[0]

  useEffect(() => {
    if (isLoading || error || accounts.length === 0) {
      return
    }

    if (hasAttemptedRestoreRef.current) {
      return
    }

    hasAttemptedRestoreRef.current = true

    async function restoreSelectedAccount() {
      const storedAccountId = getStoredSelectedAccountId()

      if (!storedAccountId) {
        const firstAccount = accounts[0]

        setSelectedAccountId(firstAccount.id)
        storeSelectedAccountId(firstAccount.id)
        onAccountChange?.(firstAccount)
        return
      }

      const { accounts: availableAccounts, matchedAccount } =
        await loadUntilAccount(storedAccountId)

      const accountToSelect = matchedAccount ?? availableAccounts[0]

      setSelectedAccountId(accountToSelect.id)
      storeSelectedAccountId(accountToSelect.id)
      onAccountChange?.(accountToSelect)
    }

    void restoreSelectedAccount()
  }, [accounts, error, isLoading, loadUntilAccount, onAccountChange])

  async function handleAccountChange(event: ChangeEvent<HTMLSelectElement>) {
    const selectedValue = event.target.value

    if (selectedValue === LOAD_MORE_VALUE) {
      await loadNextPage()
      return
    }

    const accountId = Number(selectedValue)
    const selectedAccount = accounts.find((account) => account.id === accountId)

    if (!selectedAccount) {
      return
    }

    setSelectedAccountId(selectedAccount.id)
    storeSelectedAccountId(selectedAccount.id)
    onAccountChange?.(selectedAccount)
  }

  const selectValue = selectedAccount ? String(selectedAccount.id) : ''
  const isDisabled = isLoading || Boolean(error) || accounts.length === 0

  return (
    <label className={styles.accountSelector}>
      <span className={styles.accountSelectorLabel}>Account</span>
      <select
        className={styles.accountSelect}
        disabled={isDisabled}
        onChange={handleAccountChange}
        value={selectValue}
      >
        {isLoading && <option value="">Loading accounts...</option>}
        {!isLoading && Boolean(error) && <option value="">Could not load accounts</option>}
        {!isLoading && !error && accounts.length === 0 && <option value="">No accounts found</option>}
        {accounts.map((account) => (
          <option key={account.id} value={account.id}>
            {account.name}
          </option>
        ))}
        {hasNextPage && !error && accounts.length > 0 && (
          <option value={LOAD_MORE_VALUE}>
            {isLoadingMore ? 'Loading more accounts...' : 'Load more accounts...'}
          </option>
        )}
      </select>
    </label>
  )
}

export default AccountDropdown
