"use client"

import { SidebarProvider } from "@/shared/components/ui/sidebar";
import { QueryClientProvider } from "@tanstack/react-query";
import { AppSidebar } from "../sidebar/app-sidebar";
import { queryClient } from "@/shared/api/query-client";
import Header from "../header/header";



export default function Layout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <QueryClientProvider client={queryClient}>
      <SidebarProvider>
        <div className="flex h-screen w-full">
          <AppSidebar />
          <div className="flex-1 flex flex-col min-w-0">
            <Header />
            <main className="flex-1 overflow-auto p-10">{children}</main>
          </div>
        </div>
      </SidebarProvider>
    </QueryClientProvider>
  )};