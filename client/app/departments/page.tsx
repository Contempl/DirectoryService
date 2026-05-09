'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';



const departmentSchema = z.object({
    name: z.string().min(1, 'Department name is required').max(255),
    location: z.string().min(1, 'Location is required').max(255),
    description: z.string().optional(),
});

type DepartmentFormData = z.infer<typeof departmentSchema>;

const createDepartment = async (data: DepartmentFormData) => {
    const response = await fetch('/api/departments', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Failed to create department');
    return response.json();
};

export default function DepartmentsPage() {
    const queryClient = useQueryClient();
    const [isOpen, setIsOpen] = useState(false);

    const { register, handleSubmit, formState: { errors }, reset } = useForm<DepartmentFormData>({
        resolver: zodResolver(departmentSchema),
    });

    const mutation = useMutation({
        mutationFn: createDepartment,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['departments'] });
            reset();
            setIsOpen(false);
        },
    });

    const onSubmit = (data: DepartmentFormData) => mutation.mutate(data);

    return (
        <div className="p-6">
            <button onClick={() => setIsOpen(true)} className="mb-4 px-4 py-2 bg-blue-600 text-white rounded">
                Create Department
            </button>

            {isOpen && (
                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 border p-4 rounded">
                    <div>
                        <label className="block mb-1">Name</label>
                        <input {...register('name')} className="w-full border px-2 py-1 rounded" />
                        {errors.name && <p className="text-red-500 text-sm">{errors.name.message}</p>}
                    </div>

                    <div>
                        <label className="block mb-1">Location</label>
                        <input {...register('location')} className="w-full border px-2 py-1 rounded" />
                        {errors.location && <p className="text-red-500 text-sm">{errors.location.message}</p>}
                    </div>

                    <div>
                        <label className="block mb-1">Description</label>
                        <textarea {...register('description')} className="w-full border px-2 py-1 rounded" />
                    </div>

                    <div className="flex gap-2">
                        <button type="submit" disabled={mutation.isPending} className="px-4 py-2 bg-green-600 text-white rounded">
                            {mutation.isPending ? 'Saving...' : 'Save'}
                        </button>
                        <button type="button" onClick={() => setIsOpen(false)} className="px-4 py-2 bg-gray-400 text-white rounded">
                            Cancel
                        </button>
                    </div>
                </form>
            )}
        </div>
    );
}