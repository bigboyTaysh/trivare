/**
 * Formats a date string to dd-MM-yyyy format
 * @param dateString - ISO date string or Date object
 * @returns Formatted date string in dd-MM-yyyy format
 */
export const formatDate = (dateString?: string | Date): string | null => {
  if (!dateString) return null;

  const date = typeof dateString === "string" ? new Date(dateString) : dateString;
  const day = String(date.getDate()).padStart(2, "0");
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const year = date.getFullYear();

  return `${day}-${month}-${year}`;
};

/**
 * Formats a date string to dd-MM-yyyy, HH:mm:ss format
 * @param dateString - ISO date string or Date object
 * @returns Formatted date string in dd-MM-yyyy, HH:mm:ss format
 */
export const formatDateTime = (dateString?: string | Date): string | null => {
  if (!dateString) return null;

  const date = typeof dateString === "string" ? new Date(dateString) : dateString;
  const day = String(date.getDate()).padStart(2, "0");
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const year = date.getFullYear();
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  const seconds = String(date.getSeconds()).padStart(2, "0");

  return `${day}-${month}-${year}, ${hours}:${minutes}:${seconds}`;
};
