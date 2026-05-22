import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Activity } from 'lucide-react'
import { authService } from '@/services/feedlot.service'
import { useAuthStore } from '@/stores/auth.store'
import { Button, Input, Label } from '@/components/ui'
import { cn } from '@/utils'

const schema = z.object({
  email: z.string().email('Email inválido'),
  password: z.string().min(1, 'La contraseña es requerida'),
})

type LoginForm = z.infer<typeof schema>

export default function LoginPage() {
  const navigate = useNavigate()
  const { login } = useAuthStore()
  const [error, setError] = useState<string>()

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<LoginForm>({
    resolver: zodResolver(schema),
    defaultValues: { email: 'admin@feedlot.com', password: 'Admin123!' },
  })

  const onSubmit = async (data: LoginForm) => {
    try {
      setError(undefined)
      const response = await authService.login(data.email, data.password)
      login(response)
      navigate('/')
    } catch (err: any) {
      setError(err.response?.data?.error ?? 'Credenciales inválidas')
    }
  }

  return (
    <div className="min-h-screen bg-background flex items-center justify-center p-4">
      <div className="w-full max-w-sm animate-slide-up">
        {/* Logo */}
        <div className="flex flex-col items-center mb-8">
          <div className="w-12 h-12 rounded-2xl bg-primary/10 flex items-center justify-center mb-4">
            <Activity className="w-6 h-6 text-primary" />
          </div>
          <h1 className="text-xl font-bold">SmartFeedLot</h1>
          <p className="text-sm text-muted-foreground mt-1">Plataforma de analítica ganadera</p>
        </div>

        {/* Form */}
        <div className="rounded-xl border border-border bg-card p-6 space-y-4">
          <div className="space-y-1.5">
            <h2 className="text-sm font-semibold">Iniciar sesión</h2>
            <p className="text-xs text-muted-foreground">Ingresa tus credenciales para continuar</p>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                autoComplete="email"
                {...register('email')}
                className={cn(errors.email && 'border-destructive')}
              />
              {errors.email && (
                <p className="text-xs text-destructive">{errors.email.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="password">Contraseña</Label>
              <Input
                id="password"
                type="password"
                autoComplete="current-password"
                {...register('password')}
                className={cn(errors.password && 'border-destructive')}
              />
              {errors.password && (
                <p className="text-xs text-destructive">{errors.password.message}</p>
              )}
            </div>

            {error && (
              <div className="rounded-md bg-destructive/10 border border-destructive/20 px-3 py-2">
                <p className="text-xs text-destructive">{error}</p>
              </div>
            )}

            <Button type="submit" className="w-full" loading={isSubmitting}>
              Iniciar sesión
            </Button>
          </form>
        </div>

        <p className="text-center text-xs text-muted-foreground mt-4">
          SmartFeedLot v1.0 — Gestión inteligente de feedlot bovino
        </p>
      </div>
    </div>
  )
}
