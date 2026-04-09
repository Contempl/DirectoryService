import { DepartmentTree } from "@/features/departments/tree/department-tree";

export default function DepartmentsPage() {
  return (
    <main className="p-6">
      <h2 className="text-2xl font-semibold mb-4">Подразделения</h2>
      <DepartmentTree />
    </main>
  );
}