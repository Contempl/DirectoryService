import { useUpdateLocation } from "./use-update-location"
import { LocationDto } from "@/entities/locations/types"
import z from "zod"
import { zodResolver } from "@hookform/resolvers/zod"
import { Dialog, 
  DialogContent, 
  DialogDescription,
  DialogFooter, 
  DialogHeader, 
  DialogTitle } from "@/shared/components/ui/dialog"
import { Button } from "@/shared/components/ui/button"
import { FormError } from "../create-location-dialog"
import { Label } from "@/shared/components/ui/label"
import { Input } from "@/shared/components/ui/input"
import { useForm } from "react-hook-form"


const UpdateLocationSchema = z.object({
      name: z.string().min(2).max(100),
      city: z.string().min(2).max(100),
      street: z.string().min(2).max(100),
      house: z.string().max(100),
      apartment: z.string().max(100).optional(),
      timezone: z.string() 
    });

    type UpdateLocationData = z.infer<typeof UpdateLocationSchema>;

type Props = {
  location: LocationDto
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function EditLocationDialog({ location, open, onOpenChange }: Props) {
const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<UpdateLocationData>({
    defaultValues: {
      name: location.name,
      city: location.address.city,
      street: location.address.street,
      house: location.address.house,
      apartment: location.address.apartment ?? "",
      timezone: location.timezone,
    },
    resolver: zodResolver(UpdateLocationSchema),
  });


  const { updateLocation, isPending, error, isError } = useUpdateLocation()

  const onSubmit = (data: UpdateLocationData) => {
    updateLocation(
      { id: location.id, data },
      {
        onSuccess: () => {
          onOpenChange(false)
        },
      }
    )
  }

  const getErrorMessage = (): string => {
    if (isError) {
      return error ? error.message : "Неизвестная ошибка";
    }

    return "";
  };


   return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Изменение локации</DialogTitle>
          <DialogDescription>
            Измените данные локации
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="grid gap-4 py-4">

            <div className="grid gap-2">
              <Label htmlFor="name">Название локации</Label>
              <Input
                id="name"
                placeholder="Введите название"
                className={`w-full ${
                  errors.name
                    ? "border-destructive focus-visible:ring-destructive"
                    : ""
                }`}
                {...register("name")}
              />
              <FormError message={errors.name?.message} />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="city">Город</Label>
              <Input
                id="city"
                {...register("city")}
              />
              <FormError message={errors.city?.message} />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="street">Улица</Label>
              <Input
                id="street"
                {...register("street")}
              />
              <FormError message={errors.street?.message} />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="house">Дом</Label>
              <Input
                id="house"
                {...register("house")}
              />
              <FormError message={errors.house?.message} />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="apartment">Квартира</Label>
              <Input
                id="apartment"
                {...register("apartment")}
              />
              <FormError message={errors.apartment?.message} />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="timezone">Timezone</Label>
              <Input
                id="timezone"
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
                Сохранить
              </Button>

              {error && (
                <div className="text-red-500">
                  {getErrorMessage()}
                </div>
              )}
            </DialogFooter>

          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
}