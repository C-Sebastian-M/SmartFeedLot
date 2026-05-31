import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './app/App'
import './index.css'
// Aplica el tema guardado antes del primer render para evitar flash
import './stores/theme.store'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)
