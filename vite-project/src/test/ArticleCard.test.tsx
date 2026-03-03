import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { BrowserRouter } from "react-router-dom";
import ArticleCard from "../components/ArticleCard";
import type { ArticleListItem } from "../types/api";
import "../i18n/i18n";

const mockArticle: ArticleListItem = {
  id: "abc-123",
  pluginId: "uae-gov",
  headlineEn: "UAE Signs New Trade Deal",
  headlineAr: null,
  summaryEn: "A summary of the article content.",
  summaryAr: null,
  sourceUrl: "https://example.com/article",
  publishedAt: "2026-03-01T12:00:00Z",
  country: "UAE",
  credibilityScore: 82,
  fullText: true,
  translated: false,
  topicIds: ["T2"],
};

describe("ArticleCard", () => {
  it("renders headline", () => {
    render(
      <BrowserRouter>
        <ArticleCard article={mockArticle} />
      </BrowserRouter>,
    );
    expect(screen.getByText("UAE Signs New Trade Deal")).toBeInTheDocument();
  });

  it("renders summary", () => {
    render(
      <BrowserRouter>
        <ArticleCard article={mockArticle} />
      </BrowserRouter>,
    );
    expect(screen.getByText("A summary of the article content.")).toBeInTheDocument();
  });

  it("renders country badge", () => {
    render(
      <BrowserRouter>
        <ArticleCard article={mockArticle} />
      </BrowserRouter>,
    );
    expect(screen.getByText("UAE")).toBeInTheDocument();
  });

  it("renders credibility score", () => {
    render(
      <BrowserRouter>
        <ArticleCard article={mockArticle} />
      </BrowserRouter>,
    );
    expect(screen.getByText("82")).toBeInTheDocument();
  });

  it("links to article detail page", () => {
    render(
      <BrowserRouter>
        <ArticleCard article={mockArticle} />
      </BrowserRouter>,
    );
    const link = screen.getByRole("link");
    expect(link).toHaveAttribute("href", "/article/abc-123");
  });
});
