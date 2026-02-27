"use client";

import { useEffect, useState } from "react";

// Re-export specialized hooks
export { useAuth } from "./useAuth";
export { useGraphQLAuth } from "./useGraphQLAuth";
export { useLocalStorage } from "./useLocalStorage";
export { useNotificationHub } from "./useNotificationHub";
export { useInfiniteScroll } from "./useInfiniteScroll";
export { useImageUpload } from "./useImageUpload";
export { useConfirmDialog } from "./useConfirmDialog";
export { useDropdownMenu } from "./useDropdownMenu";

// Re-export types
export type {
  UsePaginationResult,
  UsePaginationOptions,
} from "./useInfiniteScroll";
export type { UseImageUploadResult, ImageUploadState } from "./useImageUpload";
export type {
  UseConfirmDialogResult,
  UseConfirmDialogOptions,
} from "./useConfirmDialog";
export type { UseDropdownMenuResult } from "./useDropdownMenu";

export function usePagination(initialPage = 1) {
  const [page, setPage] = useState(initialPage);

  const goToPage = (newPage: number) => {
    setPage(Math.max(1, newPage));
  };

  const nextPage = () => {
    setPage((p) => p + 1);
  };

  const prevPage = () => {
    setPage((p) => Math.max(1, p - 1));
  };

  const reset = () => {
    setPage(initialPage);
  };

  return {
    page,
    goToPage,
    nextPage,
    prevPage,
    reset,
  };
}

export function useAsyncState<T>(
  asyncFn: () => Promise<T>,
  dependencies: React.DependencyList = [],
) {
  const [state, setState] = useState<{
    data: T | null;
    loading: boolean;
    error: Error | null;
  }>({
    data: null,
    loading: false,
    error: null,
  });

  useEffect(() => {
    let mounted = true;

    const run = async () => {
      setState({ data: null, loading: true, error: null });
      try {
        const result = await asyncFn();
        if (mounted) {
          setState({ data: result, loading: false, error: null });
        }
      } catch (err) {
        if (mounted) {
          setState({
            data: null,
            loading: false,
            error: err instanceof Error ? err : new Error(String(err)),
          });
        }
      }
    };

    run();

    return () => {
      mounted = false;
    };
  }, dependencies);

  return state;
}

export function useForm<
  T extends Record<string, string | number | boolean | null | undefined>,
>(initialValues: T, onSubmit?: (values: T) => void | Promise<void>) {
  const [values, setValues] = useState(initialValues);
  const [errors, setErrors] = useState<Partial<Record<keyof T, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  ) => {
    const { name, value } = e.target;
    setValues((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      await onSubmit?.(values);
    } finally {
      setIsSubmitting(false);
    }
  };

  const reset = () => {
    setValues(initialValues);
    setErrors({});
  };

  const setFieldError = (field: keyof T, error: string) => {
    setErrors((prev) => ({
      ...prev,
      [field]: error,
    }));
  };

  return {
    values,
    setValues,
    errors,
    setFieldError,
    handleChange,
    handleSubmit,
    reset,
    isSubmitting,
  };
}

export function useIsMounted() {
  const [isMounted, setIsMounted] = useState(false);

  useEffect(() => {
    setIsMounted(true);
  }, []);

  return isMounted;
}

export function useDebounce<T>(value: T, delay: number = 500): T {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => clearTimeout(handler);
  }, [value, delay]);

  return debouncedValue;
}

export function useClickOutside(
  ref: React.RefObject<HTMLElement>,
  callback: () => void,
) {
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (ref.current && !ref.current.contains(event.target as Node)) {
        callback();
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [ref, callback]);
}
