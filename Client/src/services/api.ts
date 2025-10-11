import { API_BASE_URL } from "../config/api";

export async function fetchData<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    headers: {
      "Content-Type": "application/json",
      ...options?.headers,
    },
    ...options,
  });

  if (!response.ok) {
    throw new Error(`API error: ${response.statusText}`);
  }

  return response.json();
}

export const api = {
  getData: () => fetchData("/data"),
  getDataById: (id: number) => fetchData(`/data/${id}`),
  postData: (data: any) =>
    fetchData("/data", {
      method: "POST",
      body: JSON.stringify(data),
    }),
};
