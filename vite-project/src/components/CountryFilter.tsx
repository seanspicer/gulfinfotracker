import { useTranslation } from "react-i18next";
import { COUNTRIES } from "../types/api";

interface Props {
  value: string;
  onChange: (country: string) => void;
}

export default function CountryFilter({ value, onChange }: Props) {
  const { t } = useTranslation();

  return (
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className="rounded border border-gray-300 px-3 py-2 text-sm"
      aria-label="country filter"
    >
      <option value="">{t("allCountries")}</option>
      {COUNTRIES.map((c) => (
        <option key={c} value={c}>
          {c}
        </option>
      ))}
    </select>
  );
}
