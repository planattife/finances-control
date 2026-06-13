const SELECTED_ACCOUNT_STORAGE_KEY = 'finances-control:selected-account'

export function getStoredSelectedAccountId(): number | null {
  const storedValue = localStorage.getItem(SELECTED_ACCOUNT_STORAGE_KEY)

  if (!storedValue) {
    return null
  }

  const accountId = Number(storedValue)

  return Number.isInteger(accountId) && accountId > 0 ? accountId : null
}

export function storeSelectedAccountId(accountId: number) {
  localStorage.setItem(SELECTED_ACCOUNT_STORAGE_KEY, String(accountId))
}

export function clearStoredSelectedAccountId() {
  localStorage.removeItem(SELECTED_ACCOUNT_STORAGE_KEY)
}
