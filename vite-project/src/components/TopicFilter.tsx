import { useTranslation } from "react-i18next";
import { TOPICS } from "../types/api";

interface Props {
  value: string;
  onChange: (topic: string) => void;
}

export default function TopicFilter({ value, onChange }: Props) {
  const { t, i18n } = useTranslation();
  const lang = i18n.language as "en" | "ar";

  return (
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className="rounded border border-gray-300 px-3 py-2 text-sm"
      aria-label="topic filter"
    >
      <option value="">{t("allTopics")}</option>
      {Object.entries(TOPICS).map(([id, labels]) => (
        <option key={id} value={id}>
          {labels[lang] ?? labels.en}
        </option>
      ))}
    </select>
  );
}
