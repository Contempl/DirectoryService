import {Avatar, AvatarFallback, AvatarImage} from "@/components/ui/avatar"
import { SidebarTrigger } from "../ui/sidebar";
import { routes } from "@/shared/routes";
import { Link } from "lucide-react";

export default function Header() {
  return (
    <header className="sticky top-0 z-50 w-full bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60">
      <div className="flex h-16 items-center justify-between px-4">
        <div className="flex items-center gap-2">
          <SidebarTrigger className="md:hidden mb-4" />
          <div className="flex items-center gap-2 font-semibold text-xl">
            <div className="h-8 w-8 bg-red-600 rounded-full flex items-center justify-center text-white font-bold">
              FS
            </div>
            <Link href={routes.home}>
              <span>Fullstack</span>
            </Link>
          </div>
        </div>

        {/* Правая часть - Аватарка */}
        <div className="flex items-center gap-4">
          <Avatar className="h-9 w-9 cursor-pointer hover:opacity-80 transition-opacity">
            <AvatarImage src="https://github.com/shadcn.png" alt="User" />
            <AvatarFallback>ST</AvatarFallback>
          </Avatar>
        </div>
      </div>
    </header>
  );
}
