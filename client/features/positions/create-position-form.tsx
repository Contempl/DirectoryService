"use client"

import { DepartmentShortDto } from "@/entities/departments/types";
import { DepartmentsPicker } from "@/features/departments/departments-picker";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Textarea } from "@/shared/components/ui/textarea";
import { useState } from "react";
import { useCreatePosition } from "./models/use-create-position";

type CreatePositionFormProps = {
  onSuccess: () => void;
};

export function CreatePositionForm({ onSuccess }: CreatePositionFormProps) {
  const { createPosition, isPending } = useCreatePosition();

  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [selectedDepartments, setSelectedDepartments] = useState<DepartmentShortDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    setError(null);

    if (!name.trim()) {
      setError("Название обязательно");
      return;
    }

    if (selectedDepartments.length === 0) {
      setError("Выберите хотя бы одно подразделение");
      return;
    }

    const uniqueIds = new Set(selectedDepartments.map((d) => d.id));
    if (uniqueIds.size !== selectedDepartments.length) {
      setError("Дублирующиеся подразделения недопустимы");
      return;
    }

    try {
      await createPosition({
        name: name.trim(),
        description: description.trim() || null,
        departmentIds: [...uniqueIds],
      });
      onSuccess();
    } catch {
      setError("Ошибка при создании позиции");
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

      <div className="flex flex-col gap-1">
        <label className="text-sm font-medium">Подразделения *</label>
        <DepartmentsPicker
          selectedDepartments={selectedDepartments}
          onChange={setSelectedDepartments}
          multiselect
        />
      </div>

      {error && <p className="text-sm text-red-500">{error}</p>}

      <Button onClick={handleSubmit} disabled={isPending}>
        {isPending ? "Создание..." : "Создать"}
      </Button>
    </div>
  );
}