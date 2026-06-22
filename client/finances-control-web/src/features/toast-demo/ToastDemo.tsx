import ActionButton from '../../components/ActionButton'
import {
  showError,
  showInfo,
  showPromise,
  showSuccess,
  showWarning,
} from '../../shared/notifications/toastService'
import styles from './ToastDemo.module.css'

type FakeSavedItem = {
  name: string
}

const fakeSuccessfulRequest = async (): Promise<FakeSavedItem> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve({ name: 'Test item' })
    }, 2000)
  })
}

const fakeFailedRequest = async (): Promise<void> => {
  return new Promise((_, reject) => {
    setTimeout(() => {
      reject(new Error('Dummy request failed'))
    }, 2000)
  })
}

function ToastDemo() {
  const handlePromiseSuccess = async () => {
    await showPromise(fakeSuccessfulRequest(), {
      loading: 'Saving...',
      success: (data) => `${data.name} saved successfully`,
      error: 'Could not save',
    })
  }

  const handlePromiseError = async () => {
    try {
      await showPromise(fakeFailedRequest(), {
        loading: 'Saving...',
        success: 'Saved successfully',
        error: (error) => error instanceof Error ? error.message : 'Unexpected error',
      })
    } catch {
      // Demo-only: the toast service rethrows errors so real callers can handle them.
    }
  }

  return (
    <section className={styles.demo}>
      <div className={styles.header}>
        <h1 className={styles.title}>Toast demo</h1>
        <p className={styles.description}>
          Temporary development surface for validating reusable notifications.
        </p>
      </div>

      <div className={styles.actions}>
        <ActionButton variant="primary" onClick={() => showSuccess('Saved successfully')}>
          Success toast
        </ActionButton>
        <ActionButton onClick={() => showError('Could not save', 'Please try again.')}>
          Error toast
        </ActionButton>
        <ActionButton onClick={() => showInfo('Heads up', 'This is an informational toast.')}>
          Info toast
        </ActionButton>
        <ActionButton onClick={() => showWarning('Review required', 'Check the details before continuing.')}>
          Warning toast
        </ActionButton>
        <ActionButton variant="primary" onClick={handlePromiseSuccess}>
          Promise success toast
        </ActionButton>
        <ActionButton dangerHover onClick={handlePromiseError}>
          Promise error toast
        </ActionButton>
      </div>
    </section>
  )
}

export default ToastDemo
