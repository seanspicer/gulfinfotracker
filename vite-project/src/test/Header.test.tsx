import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { BrowserRouter } from "react-router-dom";
import Header from "../components/Header";
import "../i18n/i18n";

function renderHeader() {
  return render(
    <BrowserRouter>
      <Header />
    </BrowserRouter>,
  );
}

describe("Header", () => {
  it("renders app title", () => {
    renderHeader();
    expect(screen.getByText("Gulf Info Tracker")).toBeInTheDocument();
  });

  it("renders navigation links", () => {
    renderHeader();
    expect(screen.getByText("Feed")).toBeInTheDocument();
    expect(screen.getByText("Sources Admin")).toBeInTheDocument();
  });

  it("toggles language on button click", () => {
    renderHeader();
    const langButton = screen.getByText("AR");
    fireEvent.click(langButton);
    expect(document.documentElement.dir).toBe("rtl");
    expect(document.documentElement.lang).toBe("ar");
  });
});
