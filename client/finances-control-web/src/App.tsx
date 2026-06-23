import { Navigate, Route, Routes } from 'react-router-dom'
import ProtectedLayout from './components/layout/ProtectedLayout'
import ProtectedRoute from './components/ProtectedRoute'
import AccountsPage from './pages/Accounts/AccountsPage'
import LoginPage from './pages/Login/LoginPage'
import PlaceholderPage from './pages/Placeholder/PlaceholderPage'
import AppToaster from './shared/notifications/AppToaster'

function App() {
  return (
    <>
      <Routes>
        <Route path="/" element={<LoginPage />} />
        <Route
          element={
            <ProtectedRoute>
              <ProtectedLayout />
            </ProtectedRoute>
          }
        >
          <Route path="/accounts" element={<AccountsPage />} />
          <Route
            path="/banks"
            element={<PlaceholderPage description="Bank management will be available here." title="Banks" />}
          />
          <Route
            path="/cards"
            element={<PlaceholderPage description="Card management will be available here." title="Cards" />}
          />
          <Route
            path="/transactions"
            element={
              <PlaceholderPage
                description="Transaction management will be available here."
                title="Transactions"
              />
            }
          />
          <Route path="/home" element={<Navigate to="/accounts" replace />} />
        </Route>
        <Route
          path="*"
          element={
            <Navigate to="/" replace />
          }
        />
      </Routes>
      <AppToaster />
    </>
  )
}

export default App
