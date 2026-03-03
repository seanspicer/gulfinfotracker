import { useTranslation } from "react-i18next";
import { useSources, useTriggerPoll } from "../hooks/useSources";

export default function AdminSourcesPage() {
  const { t } = useTranslation();
  const { data: sources, isLoading, isError } = useSources();
  const pollMutation = useTriggerPoll();

  if (isLoading) return <p className="text-gray-500">{t("loading")}</p>;
  if (isError) return <p className="text-red-600">{t("error")}</p>;

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold text-gray-900">{t("sourceHealth")}</h1>

      <div className="overflow-x-auto">
        <table className="w-full text-left text-sm">
          <thead className="border-b bg-gray-50 text-xs uppercase text-gray-600">
            <tr>
              <th className="px-4 py-3">{t("pluginId")}</th>
              <th className="px-4 py-3">{t("displayName")}</th>
              <th className="px-4 py-3">{t("lastPolled")}</th>
              <th className="px-4 py-3">{t("articles24h")}</th>
              <th className="px-4 py-3">{t("lastError")}</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody>
            {sources?.map((s) => (
              <tr key={s.pluginId} className="border-b">
                <td className="px-4 py-3 font-mono text-xs">{s.pluginId}</td>
                <td className="px-4 py-3">{s.displayName}</td>
                <td className="px-4 py-3">
                  {s.lastPolledAt
                    ? new Date(s.lastPolledAt).toLocaleString()
                    : t("never")}
                </td>
                <td className="px-4 py-3">{s.articlesLast24h}</td>
                <td className="px-4 py-3 text-red-600">
                  {s.lastError ?? t("none")}
                </td>
                <td className="px-4 py-3">
                  <button
                    onClick={() => pollMutation.mutate(s.pluginId)}
                    disabled={pollMutation.isPending}
                    className="rounded bg-sky-600 px-3 py-1 text-xs text-white hover:bg-sky-700 disabled:opacity-50"
                  >
                    {pollMutation.isPending ? t("polling") : t("poll")}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
