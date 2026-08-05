import { departmentsApi, departmentsQueryOptions } from "@/entities/departments/api";
import { isEnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useTreeStore } from "./use-tree-store";

type ToggleActivityVariables = { departmentId: string; isActive: boolean };
type ToggleContext = {
  snapshots: Array<[readonly unknown[], unknown]>;
  previousActivity: boolean;
};

function updateActivityInData(value: unknown, departmentId: string, isActive: boolean): unknown {
  if (Array.isArray(value)) {
    return value.map((item) => updateActivityInData(item, departmentId, isActive));
  }
  if (value && typeof value === "object") {
    const record = value as Record<string, unknown>;
    const updated = Object.fromEntries(
      Object.entries(record).map(([key, child]) => [
        key,
        updateActivityInData(child, departmentId, isActive),
      ])
    );
    if (record.id === departmentId && typeof record.isActive === "boolean") {
      updated.isActive = isActive;
    }
    return updated;
  }
  return value;
}

const activityErrorMessages: Record<string, string> = {
  "department.activity.active_descendants":
    "Нельзя деактивировать подразделение, пока у него есть активные дочерние подразделения.",
  "department.activity.deleted": "Нельзя изменить активность удалённого подразделения.",
};

export function useToggleDepartmentActivity(currentActivity: boolean) {
  const queryClient = useQueryClient();
  return useMutation<string | null, unknown, ToggleActivityVariables, ToggleContext>({
    mutationFn: ({ departmentId, isActive }) =>
      departmentsApi.toggleActivity(departmentId, isActive),
    onMutate: async ({ departmentId, isActive }) => {
      await queryClient.cancelQueries({ queryKey: [departmentsQueryOptions.baseKey] });
      const snapshots = queryClient.getQueriesData({
        queryKey: [departmentsQueryOptions.baseKey],
      });
      queryClient.setQueriesData(
        { queryKey: [departmentsQueryOptions.baseKey] },
        (data) => updateActivityInData(data, departmentId, isActive)
      );
      useTreeStore.getState().updateActivity(departmentId, isActive);
      return { snapshots, previousActivity: currentActivity };
    },
    onError: (error, variables, context) => {
      context?.snapshots.forEach(([queryKey, data]) => queryClient.setQueryData(queryKey, data));
      useTreeStore.getState().updateActivity(
        variables.departmentId,
        context?.previousActivity ?? !variables.isActive
      );
      const apiMessage = isEnvelopeError(error) ? error.messages[0] : null;
      toast.error(
        apiMessage
          ? activityErrorMessages[apiMessage.code] ?? apiMessage.message
          : "Не удалось изменить активность подразделения. Изменение отменено."
      );
    },
    onSettled: () =>
      queryClient.invalidateQueries({ queryKey: [departmentsQueryOptions.baseKey] }),
  });
}
