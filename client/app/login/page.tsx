"use client";

import { Suspense, useState, type FormEvent } from "react";
import { useSearchParams } from "next/navigation";
import { AxiosError } from "axios";
import { continueSession, loginSession } from "@/features/auth/model/auth-session";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";

export default function LoginPage() {
  return (
    <Suspense fallback={null}>
      <LoginForm />
    </Suspense>
  );
}

function LoginForm() {
  const searchParams = useSearchParams();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await loginSession({ email, password }, searchParams.get("returnUrl"));
    } catch (requestError: unknown) {
      setError(
        requestError instanceof AxiosError && requestError.response?.status === 400
          ? "Неверный email или пароль."
          : "Не удалось выполнить вход. Попробуйте ещё раз.",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleContinue() {
    setError(null);
    setIsSubmitting(true);
    try {
      await continueSession();
    } catch {
      setError("Не удалось продолжить вход. Попробуйте ещё раз.");
      setIsSubmitting(false);
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center p-6">
      <form
        className="w-full max-w-sm space-y-5 rounded-xl border bg-card p-6 shadow-lg"
        onSubmit={handleSubmit}
      >
        <div className="space-y-1">
          <h1 className="text-2xl font-semibold">Вход</h1>
          <p className="text-sm text-muted-foreground">
            Войдите в Directory Service
          </p>
        </div>

        <div className="space-y-2">
          <Label htmlFor="email">Email</Label>
          <Input
            id="email"
            type="email"
            autoComplete="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="password">Пароль</Label>
          <Input
            id="password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
          />
        </div>

        {error && <p className="text-sm text-destructive">{error}</p>}

        <Button className="w-full" type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Входим…" : "Войти"}
        </Button>
        <Button
          className="w-full"
          type="button"
          variant="outline"
          disabled={isSubmitting}
          onClick={() => void handleContinue()}
        >
          Продолжить без повторного ввода пароля
        </Button>
      </form>
    </main>
  );
}
