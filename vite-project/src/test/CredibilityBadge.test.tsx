import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import CredibilityBadge from "../components/CredibilityBadge";
import "../i18n/i18n";

describe("CredibilityBadge", () => {
  it("renders 'Not scored' when score is null", () => {
    render(<CredibilityBadge score={null} />);
    expect(screen.getByText("Not scored")).toBeInTheDocument();
  });

  it("renders score number when provided", () => {
    render(<CredibilityBadge score={85} />);
    expect(screen.getByText("85")).toBeInTheDocument();
  });

  it("applies green styling for score >= 70", () => {
    render(<CredibilityBadge score={75} />);
    const badge = screen.getByText("75");
    expect(badge.className).toContain("bg-green-100");
  });

  it("applies yellow styling for score 40-69", () => {
    render(<CredibilityBadge score={55} />);
    const badge = screen.getByText("55");
    expect(badge.className).toContain("bg-yellow-100");
  });

  it("applies red styling for score < 40", () => {
    render(<CredibilityBadge score={20} />);
    const badge = screen.getByText("20");
    expect(badge.className).toContain("bg-red-100");
  });
});
