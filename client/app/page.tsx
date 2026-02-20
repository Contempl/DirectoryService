import { Button } from "@/components/ui/button";
import { Search } from "lucide-react";

export default function Home() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center p-6">
      <h1 className="text-4xl font-bold mb-6">Directory Service</h1>
      <div className="flex items-center gap-4">
        <Button variant="default">
          <Search className="mr-2 h-4 w-4" />
          Найти силы учить фронтенд
        </Button>
        <Button variant="outline">Кнопки тут всякие. Просто тестирую.</Button>
      </div>
    </main>
  );
}