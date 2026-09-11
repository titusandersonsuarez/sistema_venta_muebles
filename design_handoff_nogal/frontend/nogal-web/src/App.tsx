import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './auth/AuthContext'
import { RequireAuth } from './auth/RequireAuth'
import { CartProvider } from './cart/CartContext'
import { AdminLayout } from './layouts/AdminLayout'
import { StoreLayout } from './layouts/StoreLayout'
import { LoginPage } from './pages/admin/LoginPage'
import { ResumenPage } from './pages/admin/ResumenPage'
import { PedidosPage } from './pages/admin/PedidosPage'
import { ProductosPage } from './pages/admin/ProductosPage'
import { ProduccionPage } from './pages/admin/ProduccionPage'
import { HomePage } from './pages/store/HomePage'
import { CatalogoPage } from './pages/store/CatalogoPage'
import { ProductoPage } from './pages/store/ProductoPage'
import { CarritoPage } from './pages/store/CarritoPage'
import { GraciasPage } from './pages/store/GraciasPage'
import { PagoDemoPage } from './pages/store/PagoDemoPage'

export function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <CartProvider>
        <Routes>
          <Route element={<StoreLayout />}>
            <Route path="/" element={<HomePage />} />
            <Route path="/catalogo" element={<CatalogoPage />} />
            <Route path="/producto/:slug" element={<ProductoPage />} />
            <Route path="/carrito" element={<CarritoPage />} />
            <Route path="/carrito/pago-demo/:codigo" element={<PagoDemoPage />} />
            <Route path="/carrito/gracias/:codigo" element={<GraciasPage />} />
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
            <Route path="*" element={<HomePage />} />
          </Route>
        </Routes>
        </CartProvider>
      </AuthProvider>
    </BrowserRouter>
  )
}
