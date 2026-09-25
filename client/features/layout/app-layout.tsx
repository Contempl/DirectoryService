"use client"

import { useEffect } from "react";
import { SidebarProvider } from "@/shared/components/ui/sidebar";
import { QueryClientProvider } from "@tanstack/react-query";
import { usePathname, useRouter } from "next/navigation";
import { AppSidebar } from "../sidebar/app-sidebar";
import { queryClient } from "@/shared/api/query-client";
import Header from "../header/header";
import { Toaster } from "sonner";
import { AuthBootstrap } from "../auth/ui/auth-bootstrap";
import { useAuthStore } from "../auth/model/auth-store";
import { Spinner } from "@/shared/components/ui/spinner";



export default function Layout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const pathname = usePathname();
  const router = useRouter();
  const authStatus = useAuthStore((state) => state.status);
  const isLoginPage = pathname === "/login";

  useEffect(() => {
    if (!isLoginPage && authStatus === "anonymous") {
      router.replace("/login");
    }
  }, [authStatus, isLoginPage, router]);

  return (
    <QueryClientProvider client={queryClient}>
      <AuthBootstrap />
      {isLoginPage ? (
        children
      ) : authStatus === "authenticated" ? (
        <SidebarProvider>
          <div className="flex h-screen w-full">
            <AppSidebar />
            <div className="flex-1 flex flex-col min-w-0">
              <Header />
              <main className="flex-1 overflow-auto p-10">{children}</main>
            </div>
          </div>
        </SidebarProvider>
      ) : (
        <main className="flex min-h-screen items-center justify-center">
          <Spinner className="size-8" />
        </main>
      )}
      <Toaster position="top-center" duration={3000} richColors/>
    </QueryClientProvider>
  )};
