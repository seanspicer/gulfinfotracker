import { useTranslation } from "react-i18next";

interface Props {
  page: number;
  pageSize: number;
  total: number;
  onPageChange: (page: number) => void;
}

export default function Pagination({ page, pageSize, total, onPageChange }: Props) {
  const { t } = useTranslation();
  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  if (totalPages <= 1) return null;

  return (
    <div className="mt-6 flex items-center justify-center gap-4 text-sm">
      <button
        onClick={() => onPageChange(page - 1)}
        disabled={page <= 1}
        className="rounded border px-3 py-1 disabled:opacity-40"
      >
        {t("prev")}
      </button>
      <span>
        {t("page")} {page} {t("of")} {totalPages}
      </span>
      <button
        onClick={() => onPageChange(page + 1)}
        disabled={page >= totalPages}
        className="rounded border px-3 py-1 disabled:opacity-40"
      >
        {t("next")}
      </button>
    </div>
  );
}
