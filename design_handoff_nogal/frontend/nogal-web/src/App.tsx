import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './auth/AuthContext'
import { RequireAuth } from './auth/RequireAuth'
import { AdminLayout } from './layouts/AdminLayout'
import { StoreLayout } from './layouts/StoreLayout'
import { LoginPage } from './pages/admin/LoginPage'
import { ResumenPage } from './pages/admin/ResumenPage'
import { PedidosPage } from './pages/admin/PedidosPage'
import { ProductosPage } from './pages/admin/ProductosPage'
import { ProduccionPage } from './pages/admin/ProduccionPage'
import { HomePlaceholder } from './pages/store/HomePlaceholder'
import { CatalogoPage } from './pages/store/CatalogoPage'
import { ProductoPage } from './pages/store/ProductoPage'

export function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route element={<StoreLayout />}>
            <Route path="/" element={<HomePlaceholder />} />
            <Route path="/catalogo" element={<CatalogoPage />} />
            <Route path="/producto/:slug" element={<ProductoPage />} />
          </Route>

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

          <Route element={<StoreLayout />}>
            <Route path="*" element={<HomePlaceholder />} />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
