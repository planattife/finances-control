const SELECTED_ACCOUNT_STORAGE_KEY = 'finances-control:selected-account'
export const SELECTED_ACCOUNT_CHANGED_EVENT = 'finances-control:selected-account-changed'

function dispatchSelectedAccountChanged(accountId: number | null) {
  window.dispatchEvent(
    new CustomEvent<{ accountId: number | null }>(SELECTED_ACCOUNT_CHANGED_EVENT, {
      detail: { accountId },
    }),
  )
}

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
  dispatchSelectedAccountChanged(accountId)
}

export function clearStoredSelectedAccountId() {
  localStorage.removeItem(SELECTED_ACCOUNT_STORAGE_KEY)
  dispatchSelectedAccountChanged(null)
}
