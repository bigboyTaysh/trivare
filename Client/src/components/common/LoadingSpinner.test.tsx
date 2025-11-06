import { render, screen } from "@testing-library/react";
import { LoadingSpinner } from "./LoadingSpinner";
import { describe, it, expect } from "vitest";

describe("LoadingSpinner", () => {
  it("renders loading spinner with animation", () => {
    // Arrange & Act
    render(<LoadingSpinner />);

    // Assert
    const spinner = screen.getByTestId("loading-spinner");
    expect(spinner).toBeInTheDocument();
    expect(spinner).toHaveClass("animate-spin");
  });

  it("renders loading text", () => {
    // Arrange & Act
    render(<LoadingSpinner />);

    // Assert
    expect(screen.getByText("Loading...")).toBeInTheDocument();
  });

  it("renders with proper layout structure", () => {
    // Arrange & Act
    render(<LoadingSpinner />);

    // Assert
    const container = screen.getByTestId("loading-container");
    expect(container).toHaveClass("flex", "min-h-screen", "items-center", "justify-center");

    const content = screen.getByTestId("loading-content");
    expect(content).toHaveClass("flex", "flex-col", "items-center", "space-y-4");
  });
});
