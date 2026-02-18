"use client";

import { routes } from "@/shared/routes";
import { usePathname } from "next/navigation";
import { Home, MonitorCog, Briefcase, Waypoints } from "lucide-react"; 
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarTrigger,
} from "../ui/sidebar";
import Link from "next/link";

const menuItems = [
  { href: routes.home, label: "Home", icon: Home },
  { href: routes.departments, label: "Departments", icon: MonitorCog },
  { href: routes.locations, label: "Locations", icon: Waypoints },
  { href: routes.positions, label: "Positions", icon: Briefcase },
];

export function AppSidebar() {
  const pathname = usePathname();

  return (
    <Sidebar collapsible="icon" suppressHydrationWarning>
      <SidebarHeader className="pl-2">
        <SidebarTrigger />
      </SidebarHeader>
      <SidebarContent className="py-4">
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu className="space-y-1">
              {menuItems.map((item) => {
                const isActive =
                  pathname === item.href ||
                  pathname.startsWith(item.href + "/");

                return (
                  <SidebarMenuItem key={item.href}>
                    <SidebarMenuButton
                      asChild
                      isActive={isActive}
                      tooltip={item.label}
                      className="hover:bg-gray-700 transition-colors data-[state=expanded]:justify-start data-[state=collapsed]:justify-center"
                    >
                      <Link
                        href={item.href}
                        className="flex items-center gap-3 w-full"
                      >
                        <item.icon className="h-5 w-5 shrink-0" />
                        <span className="ml-3 text-sm font-medium group-data-[collapsible=icon]:hidden">
                          {item.label}
                        </span>
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
    </Sidebar>
  );
}