"use client"

import { PositionDto } from "@/entities/positions/types";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Textarea } from "@/shared/components/ui/textarea";
import { useState } from "react";
import { useUpdatePosition } from "./models/use-update-position";

type EditPositionFormProps = {
  position: PositionDto;
  onSuccess: () => void;
};

export function EditPositionForm({ position, onSuccess }: EditPositionFormProps) {
  const { updatePosition, isPending } = useUpdatePosition();

  const [name, setName] = useState(position.name);
  const [description, setDescription] = useState(position.description ?? "");
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    setError(null);

    if (!name.trim()) {
      setError("Название обязательно");
      return;
    }

    try {
      await updatePosition({
        id: position.id,
        request: {
          name: name.trim(),
          description: description.trim() || null,
        },
      });
      onSuccess();
    } catch {
      setError("Ошибка при обновлении позиции");
    }
  };

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-col gap-1">
        <label className="text-sm font-medium">Название *</label>
        <Input
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Введите название..."
        />
      </div>

      <div className="flex flex-col gap-1">
        <label className="text-sm font-medium">Описание</label>
        <Textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Введите описание..."
        />
      </div>

      {error && <p className="text-sm text-red-500">{error}</p>}

      <Button onClick={handleSubmit} disabled={isPending}>
        {isPending ? "Сохранение..." : "Сохранить"}
      </Button>
    </div>
  );
}