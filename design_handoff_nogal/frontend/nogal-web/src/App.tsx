import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './auth/AuthContext'
import { RequireAuth } from './auth/RequireAuth'
import { AdminLayout } from './layouts/AdminLayout'
import { LoginPage } from './pages/admin/LoginPage'
import { ResumenPage } from './pages/admin/ResumenPage'
import { PedidosPage } from './pages/admin/PedidosPage'
import { ProductosPage } from './pages/admin/ProductosPage'
import { ProduccionPage } from './pages/admin/ProduccionPage'
import { HomePlaceholder } from './pages/store/HomePlaceholder'

export function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/" element={<HomePlaceholder />} />
          <Route path="/admin/login" element={<LoginPage />} />

          <Route
            path="/admin"
            element={
              <RequireAuth>
                <AdminLayout />
              </RequireAuth>
            }
          >
            <Route index element={<ResumenPage />} />
            <Route path="pedidos" element={<PedidosPage />} />
            <Route path="productos" element={<ProductosPage />} />
            <Route path="produccion" element={<ProduccionPage />} />
          </Route>

          <Route path="*" element={<HomePlaceholder />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
