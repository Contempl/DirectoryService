import { Button } from "@/shared/components/ui/button";
import { Dialog, 
  DialogContent, 
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle } 
  from "@/shared/components/ui/dialog"
import { Input } from "@/shared/components/ui/input";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Label } from "@radix-ui/react-label"
import { useCreateLocation } from "./model/use-create-location";
import z from "zod";


const createLocationSchema = z.object({
  name: z
    .string()
    .min(1, "Название обязательно")
    .min(2, "Название должно содержать минимум 2 символа")
    .max(200, "Название не должно превышать 200 символов"),

  city: z
    .string()
    .min(1, "Город обязателен")
    .min(2, "Минимум 2 символа"),

  street: z
    .string()
    .min(1, "Улица обязательна"),

  house: z
    .string()
    .min(1, "Номер дома обязателен"),

  apartment: z.string().optional(),

  timezone: z
    .string()
    .min(1, "Таймзона обязательна"),
});

type CreateLocationData = z.infer<typeof createLocationSchema>;

type Props = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function CreateLocationDialog({ open, onOpenChange }: Props) {
  const initialData: CreateLocationData = {
    name: "",
    city: "",
    street: "",
    house: "",
    apartment: "",
    timezone: "",
  };

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<CreateLocationData>({
    defaultValues: initialData,
    resolver: zodResolver(createLocationSchema),
  });

  const { createLocation, isPending, error } = useCreateLocation();

  const onSubmit = (data: CreateLocationData) => {
    createLocation(data, {
      onSuccess: () => {
        reset(initialData);
        onOpenChange(false);
      },
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[520px]">
        <DialogHeader>
          <DialogTitle>Создание локации</DialogTitle>
          <DialogDescription>
            Заполните форму для создания новой локации
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="grid gap-4 py-4">

            {/* Название */}
            <div className="grid gap-2">
              <Label htmlFor="name">Название</Label>
              <Input
                id="name"
                placeholder="Например: Центральный офис"
                className={
                  errors.name
                    ? "border-destructive focus-visible:ring-destructive"
                    : ""
                }
                {...register("name")}
              />
              <FormError message={errors.name?.message} />
            </div>

            {/* Город */}
            <div className="grid gap-2">
              <Label htmlFor="city">Город</Label>
              <Input
                id="city"
                placeholder="Москва"
                className={
                  errors.city
                    ? "border-destructive focus-visible:ring-destructive"
                    : ""
                }
                {...register("city")}
              />
              <FormError message={errors.city?.message} />
            </div>

            {/* Улица */}
            <div className="grid gap-2">
              <Label htmlFor="street">Улица</Label>
              <Input
                id="street"
                placeholder="Ленина"
                className={
                  errors.street
                    ? "border-destructive focus-visible:ring-destructive"
                    : ""
                }
                {...register("street")}
              />
              <FormError message={errors.street?.message} />
            </div>

            {/* Дом + квартира */}
            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="house">Дом</Label>
                <Input
                  id="house"
                  placeholder="10"
                  className={
                    errors.house
                      ? "border-destructive focus-visible:ring-destructive"
                      : ""
                  }
                  {...register("house")}
                />
                <FormError message={errors.house?.message} />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="apartment">Квартира</Label>
                <Input
                  id="apartment"
                  placeholder="25 (необязательно)"
                  {...register("apartment")}
                />
              </div>
            </div>

            {/* Таймзона */}
            <div className="grid gap-2">
              <Label htmlFor="timezone">Часовой пояс</Label>
              <Input
                id="timezone"
                placeholder="Europe/Moscow"
                className={
                  errors.timezone
                    ? "border-destructive focus-visible:ring-destructive"
                    : ""
                }
                {...register("timezone")}
              />
              <FormError message={errors.timezone?.message} />
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
              >
                Отмена
              </Button>

              <Button type="submit" disabled={isPending}>
                {isPending ? "Создание..." : "Создать локацию"}
              </Button>
            </DialogFooter>

            {error && (
              <div className="text-red-500 text-sm">
                {error.message}
              </div>
            )}
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

type FormErrorProps = {
  message?: string
}

export const FormError = ({ message }: FormErrorProps) => {
  if (!message) return null

  return (
    <p className="text-sm text-red-500 mt-1">
      {message}
    </p>
  )
}