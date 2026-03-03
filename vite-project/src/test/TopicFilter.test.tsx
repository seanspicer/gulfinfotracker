import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import TopicFilter from "../components/TopicFilter";
import "../i18n/i18n";

describe("TopicFilter", () => {
  it("renders all topics plus 'All Topics' option", () => {
    render(<TopicFilter value="" onChange={vi.fn()} />);
    const select = screen.getByRole("combobox", { name: /topic filter/i });
    expect(select).toBeInTheDocument();
    // 5 topics + 1 "All Topics"
    expect(select.querySelectorAll("option")).toHaveLength(6);
  });

  it("has 'All Topics' as first option", () => {
    render(<TopicFilter value="" onChange={vi.fn()} />);
    const options = screen.getAllByRole("option");
    expect(options[0]).toHaveTextContent("All Topics");
  });
});
