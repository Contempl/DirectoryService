"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { X } from "lucide-react";
import { toast } from "sonner";
import { RoleGate } from "@/features/auth/ui/role-gate";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Spinner } from "@/shared/components/ui/spinner";
import { systemRoles, usersApi, type SystemRole } from "../api/users-api";

const pageSize = 10;

export function UsersPage() {
  const [page, setPage] = useState(1);
  const queryClient = useQueryClient();
  const queryKey = ["auth-users", page, pageSize];
  const usersQuery = useQuery({
    queryKey,
    queryFn: () => usersApi.getUsers(page, pageSize),
  });

  const refreshUsers = () =>
    queryClient.invalidateQueries({ queryKey: ["auth-users"] });

  const assignRole = useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: SystemRole }) =>
      usersApi.assignRole(userId, role),
    onSuccess: refreshUsers,
    onError: () => toast.error("Could not assign the role."),
  });

  const removeRole = useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: string }) =>
      usersApi.removeRole(userId, role),
    onSuccess: refreshUsers,
    onError: () => toast.error("Could not remove the role. The last Admin role is protected."),
  });

  return (
    <RoleGate
      roles={["Admin"]}
      fallback={<p className="text-destructive">You do not have access to this page.</p>}
    >
      <section className="space-y-6">
        <div>
          <h1 className="text-2xl font-semibold">Users</h1>
          <p className="text-sm text-muted-foreground">
            Assign and remove application roles.
          </p>
        </div>

        {usersQuery.isPending ? (
          <Spinner />
        ) : usersQuery.isError ? (
          <p className="text-destructive">
            {usersQuery.error instanceof Error
              ? usersQuery.error.message
              : "Failed to load users."}
          </p>
        ) : (
          <>
            <div className="overflow-hidden rounded-lg border">
              {usersQuery.data.items.length === 0 ? (
                <p className="p-6 text-sm text-muted-foreground">No users found.</p>
              ) : usersQuery.data.items.map((user) => {
                const availableRoles = systemRoles.filter(
                  (role) => !user.roles.some(
                    (assignedRole) => assignedRole.toLowerCase() === role.toLowerCase(),
                  ),
                );

                return (
                  <div
                    key={user.id}
                    className="grid gap-4 border-b p-4 last:border-b-0 md:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_12rem] md:items-center"
                  >
                    <div className="min-w-0">
                      <p className="truncate font-medium">
                        {[user.firstName, user.lastName].filter(Boolean).join(" ") || user.email}
                      </p>
                      <p className="truncate text-sm text-muted-foreground">{user.email}</p>
                    </div>

                    <div className="flex flex-wrap gap-2">
                      {user.roles.length === 0 ? (
                        <span className="text-sm text-muted-foreground">No roles</span>
                      ) : user.roles.map((role) => (
                        <Badge key={role} variant="secondary" className="gap-1">
                          {role}
                          <button
                            type="button"
                            aria-label={`Remove ${role} role`}
                            disabled={removeRole.isPending}
                            onClick={() => removeRole.mutate({ userId: user.id, role })}
                          >
                            <X className="size-3" />
                          </button>
                        </Badge>
                      ))}
                    </div>

                    <Select
                      key={user.roles.slice().sort().join("|")}
                      disabled={availableRoles.length === 0 || assignRole.isPending}
                      onValueChange={(role) =>
                        assignRole.mutate({ userId: user.id, role: role as SystemRole })
                      }
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Add role" />
                      </SelectTrigger>
                      <SelectContent>
                        {availableRoles.map((role) => (
                          <SelectItem key={role} value={role}>{role}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                );
              })}
            </div>

            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">
                {usersQuery.data.totalCount} users
              </p>
              <div className="flex items-center gap-2">
                <Button
                  variant="outline"
                  disabled={page === 1}
                  onClick={() => setPage((current) => current - 1)}
                >
                  Previous
                </Button>
                <span className="text-sm">
                  Page {usersQuery.data.pageNumber} of {Math.max(usersQuery.data.totalPages, 1)}
                </span>
                <Button
                  variant="outline"
                  disabled={page >= usersQuery.data.totalPages}
                  onClick={() => setPage((current) => current + 1)}
                >
                  Next
                </Button>
              </div>
            </div>
          </>
        )}
      </section>
    </RoleGate>
  );
}
