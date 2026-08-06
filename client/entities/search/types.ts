export const searchResultTypes = ["location", "department", "position"] as const;

export type SearchResultType = (typeof searchResultTypes)[number];

export type SearchResultDto = {
  type: SearchResultType;
  id: string;
  title: string;
  subtitle: string;
  href?: string;
};
