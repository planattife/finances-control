import { Toaster } from 'sonner'

function AppToaster() {
  return (
    <Toaster
      position="top-right"
      richColors
      closeButton
      duration={4000}
    />
  )
}

export default AppToaster
