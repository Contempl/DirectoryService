import { DepartmentTree } from "@/features/departments/tree/department-tree";
import { DepartmentPositionsList } from "@/features/department-positions/department-positions-list";

export default function DepartmentsPage() {
  return (
    <main className="space-y-6 p-6">
      <h1 className="text-2xl font-semibold">Оргструктура</h1>

      <div className="grid gap-6 lg:grid-cols-[minmax(320px,2fr)_minmax(320px,3fr)]">
        <section className="rounded-lg border p-4">
          <h2 className="mb-4 text-lg font-semibold">Подразделения</h2>
          <DepartmentTree />
        </section>

        <section className="rounded-lg border p-4">
          <h2 className="mb-4 text-lg font-semibold">Позиции</h2>
          <DepartmentPositionsList />
        </section>
      </div>
    </main>
  );
}
