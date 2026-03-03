import { useTranslation } from "react-i18next";

interface Props {
  score: number | null;
}

function scoreColor(score: number): string {
  if (score >= 70) return "bg-green-100 text-green-800";
  if (score >= 40) return "bg-yellow-100 text-yellow-800";
  return "bg-red-100 text-red-800";
}

export default function CredibilityBadge({ score }: Props) {
  const { t } = useTranslation();

  if (score === null) {
    return (
      <span className="inline-block rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-500">
        {t("notScored")}
      </span>
    );
  }

  return (
    <span
      className={`inline-block rounded-full px-2 py-0.5 text-xs font-semibold ${scoreColor(score)}`}
      title={`${t("credibility")}: ${score}/100`}
    >
      {score}
    </span>
  );
}
