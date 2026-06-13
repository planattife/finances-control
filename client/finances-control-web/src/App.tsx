import { Navigate, Route, Routes } from 'react-router-dom'
import ProtectedLayout from './components/layout/ProtectedLayout'
import ProtectedRoute from './components/ProtectedRoute'
import LoginPage from './pages/Login/LoginPage'
import PlaceholderPage from './pages/Placeholder/PlaceholderPage'

function App() {
  return (
    <Routes>
      <Route path="/" element={<LoginPage />} />
      <Route
        element={
          <ProtectedRoute>
            <ProtectedLayout />
          </ProtectedRoute>
        }
      >
        <Route
          path="/accounts"
          element={
            <PlaceholderPage
              description="Account management will be available here."
              title="Accounts"
            />
          }
        />
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
  )
}

export default App
