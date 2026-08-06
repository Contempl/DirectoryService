"use client";

import { useQuery } from "@tanstack/react-query";
import { Building2, Loader2, MapPin, Search, UserRound } from "lucide-react";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useRef, useState } from "react";
import { useDebounce } from "use-debounce";

import { searchQueryOptions } from "@/entities/search/api";
import type { SearchResultDto, SearchResultType } from "@/entities/search/types";
import { Button } from "@/shared/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogTitle } from "@/shared/components/ui/dialog";
import { Input } from "@/shared/components/ui/input";
import { routes } from "@/shared/routes";

const groups: Array<{ type: SearchResultType; label: string; icon: typeof MapPin }> = [
  { type: "location", label: "Локации", icon: MapPin },
  { type: "department", label: "Подразделения", icon: Building2 },
  { type: "position", label: "Позиции", icon: UserRound },
];

const sectionRoutes: Record<SearchResultType, string> = {
  location: routes.locations,
  department: routes.departments,
  position: routes.positions,
};

function resultHref(result: SearchResultDto) {
  return result.href ?? `${sectionRoutes[result.type]}?focus=${result.id}`;
}

export function GlobalSearch() {
  const router = useRouter();
  const inputRef = useRef<HTMLInputElement>(null);
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");
  const [activeIndex, setActiveIndex] = useState(0);
  const [debouncedQuery] = useDebounce(query.trim(), 300);
  const canSearch = debouncedQuery.length >= 2;
  const { data = [], isFetching, isError } = useQuery(searchQueryOptions(debouncedQuery));

  const groupedResults = useMemo(
    () => groups.map((group) => ({ ...group, results: data.filter((item) => item.type === group.type) })),
    [data],
  );
  const flatResults = useMemo(() => groupedResults.flatMap((group) => group.results), [groupedResults]);

  useEffect(() => {
    const handleShortcut = (event: KeyboardEvent) => {
      if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        setOpen(true);
      }
    };
    document.addEventListener("keydown", handleShortcut);
    return () => document.removeEventListener("keydown", handleShortcut);
  }, []);

  useEffect(() => {
    if (open) requestAnimationFrame(() => inputRef.current?.focus());
  }, [open]);

  const navigate = (result: SearchResultDto) => {
    setOpen(false);
    setQuery("");
    router.push(resultHref(result));
  };

  const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (flatResults.length === 0) return;
    if (event.key === "ArrowDown") {
      event.preventDefault();
      setActiveIndex((current) => (current + 1) % flatResults.length);
    } else if (event.key === "ArrowUp") {
      event.preventDefault();
      setActiveIndex((current) => (current - 1 + flatResults.length) % flatResults.length);
    } else if (event.key === "Enter") {
      event.preventDefault();
      navigate(flatResults[activeIndex]);
    }
  };

  let status: React.ReactNode = null;
  if (query.trim().length < 2) status = "Введите минимум 2 символа";
  else if (isFetching) status = <span className="inline-flex items-center gap-2"><Loader2 className="size-4 animate-spin" /> Поиск…</span>;
  else if (isError) status = "Не удалось выполнить поиск";
  else if (canSearch && flatResults.length === 0) status = "Ничего не найдено";

  return (
    <>
      <Button variant="outline" className="h-9 w-9 justify-center px-0 text-muted-foreground sm:w-72 sm:justify-between sm:px-3" onClick={() => setOpen(true)} aria-label="Открыть глобальный поиск">
        <span className="flex items-center gap-2"><Search className="size-4" /><span className="hidden sm:inline">Поиск…</span></span>
        <kbd className="hidden rounded border bg-muted px-1.5 py-0.5 text-xs sm:inline">Ctrl K</kbd>
      </Button>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="top-[20%] block max-h-[70vh] translate-y-0 overflow-hidden p-0 sm:max-w-2xl" showCloseButton={false}>
          <DialogTitle className="sr-only">Глобальный поиск</DialogTitle>
          <DialogDescription className="sr-only">Поиск по локациям, подразделениям и позициям</DialogDescription>
          <div className="flex items-center border-b px-4">
            <Search className="mr-2 size-5 shrink-0 text-muted-foreground" />
            <Input ref={inputRef} value={query} onChange={(event) => { setQuery(event.target.value); setActiveIndex(0); }} onKeyDown={handleKeyDown} placeholder="Найти локацию, подразделение или позицию…" aria-label="Поисковый запрос" aria-controls="global-search-results" aria-activedescendant={flatResults[activeIndex] ? `search-result-${flatResults[activeIndex].type}-${flatResults[activeIndex].id}` : undefined} className="h-14 border-0 px-0 shadow-none focus-visible:ring-0" />
            <kbd className="rounded border bg-muted px-1.5 py-0.5 text-xs">Esc</kbd>
          </div>

          <div id="global-search-results" role="listbox" className="max-h-[calc(70vh-3.5rem)] overflow-y-auto p-2">
            {status ? <div className="py-10 text-center text-sm text-muted-foreground">{status}</div> : groupedResults.map((group) => group.results.length > 0 ? (
              <section key={group.type} className="mb-2 last:mb-0">
                <h3 className="px-2 py-1.5 text-xs font-medium text-muted-foreground">{group.label}</h3>
                {group.results.map((result) => {
                  const index = flatResults.indexOf(result);
                  const Icon = group.icon;
                  const active = index === activeIndex;
                  return (
                    <button id={`search-result-${result.type}-${result.id}`} key={`${result.type}-${result.id}`} type="button" role="option" aria-selected={active} onMouseMove={() => setActiveIndex(index)} onClick={() => navigate(result)} className={`flex w-full items-center gap-3 rounded-md px-3 py-2 text-left ${active ? "bg-accent text-accent-foreground" : ""}`}>
                      <Icon className="size-4 shrink-0 text-muted-foreground" />
                      <span className="min-w-0"><span className="block truncate text-sm font-medium">{result.title}</span><span className="block truncate text-xs text-muted-foreground">{result.subtitle}</span></span>
                    </button>
                  );
                })}
              </section>
            ) : null)}
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}
