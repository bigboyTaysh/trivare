import { describe, it, expect } from "vitest";
import { formatDate, formatDateTime } from "../lib/dateUtils";

describe("Date Utilities", () => {
  describe("formatDate", () => {
    it("should format a valid date string to dd-MM-yyyy format", () => {
      const dateString = "2024-07-15";
      const result = formatDate(dateString);
      expect(result).toBe("15-07-2024");
    });

    it("should format a Date object to dd-MM-yyyy format", () => {
      const date = new Date(2024, 6, 15); // July 15, 2024 (months are 0-indexed)
      const result = formatDate(date);
      expect(result).toBe("15-07-2024");
    });

    it("should return null for null input", () => {
      const result = formatDate(null as unknown as string | Date);
      expect(result).toBeNull();
    });

    it("should return null for undefined input", () => {
      const result = formatDate(undefined);
      expect(result).toBeNull();
    });

    it("should return null for empty string", () => {
      const result = formatDate("");
      expect(result).toBeNull();
    });

    it("should pad single digit day and month with zeros", () => {
      const date = new Date(2024, 0, 5); // January 5, 2024
      const result = formatDate(date);
      expect(result).toBe("05-01-2024");
    });

    it("should handle leap year dates correctly", () => {
      const date = new Date(2024, 1, 29); // February 29, 2024 (leap year)
      const result = formatDate(date);
      expect(result).toBe("29-02-2024");
    });

    it("should handle year boundaries correctly", () => {
      const date = new Date(2023, 11, 31); // December 31, 2023
      const result = formatDate(date);
      expect(result).toBe("31-12-2023");
    });

    it("should handle ISO date strings with time component", () => {
      const dateString = "2024-07-15T14:30:00.000Z";
      const result = formatDate(dateString);
      expect(result).toBe("15-07-2024");
    });
  });

  describe("formatDateTime", () => {
    it("should format a valid date string to dd-MM-yyyy, HH:mm:ss format", () => {
      const dateString = "2024-07-15T14:30:45";
      const result = formatDateTime(dateString);
      expect(result).toBe("15-07-2024, 14:30:45");
    });

    it("should format a Date object to dd-MM-yyyy, HH:mm:ss format", () => {
      const date = new Date(2024, 6, 15, 14, 30, 45); // July 15, 2024 14:30:45
      const result = formatDateTime(date);
      expect(result).toBe("15-07-2024, 14:30:45");
    });

    it("should return null for null input", () => {
      const result = formatDateTime(null as unknown as string | Date);
      expect(result).toBeNull();
    });

    it("should return null for undefined input", () => {
      const result = formatDateTime(undefined);
      expect(result).toBeNull();
    });

    it("should return null for empty string", () => {
      const result = formatDateTime("");
      expect(result).toBeNull();
    });

    it("should pad single digit hours, minutes, and seconds with zeros", () => {
      const date = new Date(2024, 6, 15, 9, 5, 3); // July 15, 2024 09:05:03
      const result = formatDateTime(date);
      expect(result).toBe("15-07-2024, 09:05:03");
    });

    it("should handle midnight correctly", () => {
      const date = new Date(2024, 6, 15, 0, 0, 0); // July 15, 2024 00:00:00
      const result = formatDateTime(date);
      expect(result).toBe("15-07-2024, 00:00:00");
    });

    it("should handle noon correctly", () => {
      const date = new Date(2024, 6, 15, 12, 0, 0); // July 15, 2024 12:00:00
      const result = formatDateTime(date);
      expect(result).toBe("15-07-2024, 12:00:00");
    });

    it("should handle ISO date strings with timezone", () => {
      // Note: Date parsing converts to local timezone, so we test the format rather than exact time
      const dateString = "2024-07-15T14:30:45.123Z";
      const result = formatDateTime(dateString);
      expect(result).toMatch(/^15-07-2024, \d{2}:\d{2}:\d{2}$/);
      // The exact time will depend on the local timezone
    });
  });

  describe("Edge Cases", () => {
    it("should handle invalid date strings gracefully", () => {
      const invalidDateString = "invalid-date";
      const result = formatDate(invalidDateString);
      // Invalid date strings result in "Invalid Date" which formats to NaN-NaN-NaN
      expect(result).toBe("NaN-NaN-NaN");
    });

    it("should handle very old dates", () => {
      const date = new Date(1900, 0, 1);
      const result = formatDate(date);
      expect(result).toBe("01-01-1900");
    });

    it("should handle future dates", () => {
      const date = new Date(2050, 11, 31);
      const result = formatDate(date);
      expect(result).toBe("31-12-2050");
    });
  });
});
