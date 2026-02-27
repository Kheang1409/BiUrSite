"use client";

import { useState, useCallback, useRef, useEffect } from "react";
import { FileUtils } from "@/utils/helpers";

export interface ImageUploadState {
  bytes: number[] | null;
  previewUrl: string | null;
  mimeType: string | null;
  shouldRemove: boolean;
}

export interface UseImageUploadResult extends ImageUploadState {
  fileInputRef: React.RefObject<HTMLInputElement | null>;
  handleFileSelect: (file?: File) => Promise<void>;
  removeImage: () => void;
  undoRemove: () => void;
  reset: () => void;
  hasImage: (existingImageUrl?: string | null) => boolean;
  getEffectiveImageUrl: (existingImageUrl?: string | null) => string | null;
}

export function useImageUpload(): UseImageUploadResult {
  const [bytes, setBytes] = useState<number[] | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [mimeType, setMimeType] = useState<string | null>(null);
  const [shouldRemove, setShouldRemove] = useState(false);

  const fileInputRef = useRef<HTMLInputElement | null>(null);

  // Cleanup blob URLs on unmount
  useEffect(() => {
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, [previewUrl]);

  const handleFileSelect = useCallback(async (file?: File) => {
    if (!file) return;

    try {
      const fileBytes = await FileUtils.fileToByteArray(file);

      // Revoke old preview URL
      setPreviewUrl((prev) => {
        if (prev) URL.revokeObjectURL(prev);
        return URL.createObjectURL(file);
      });

      setBytes(fileBytes);
      setMimeType(file.type || "image/jpeg");
      setShouldRemove(false);
    } catch (error) {
      console.error("Failed to read image file:", error);
      throw error;
    }
  }, []);

  const removeImage = useCallback(() => {
    // Revoke preview URL
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
    }

    setPreviewUrl(null);
    setBytes(null);
    setMimeType(null);
    setShouldRemove(true);

    // Clear file input
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  }, [previewUrl]);

  const undoRemove = useCallback(() => {
    setShouldRemove(false);
  }, []);

  const reset = useCallback(() => {
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
    }

    setPreviewUrl(null);
    setBytes(null);
    setMimeType(null);
    setShouldRemove(false);

    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  }, [previewUrl]);

  const hasImage = useCallback(
    (existingImageUrl?: string | null): boolean => {
      if (shouldRemove) return false;
      return !!previewUrl || !!existingImageUrl;
    },
    [previewUrl, shouldRemove],
  );

  const getEffectiveImageUrl = useCallback(
    (existingImageUrl?: string | null): string | null => {
      if (shouldRemove) return null;
      return previewUrl || existingImageUrl || null;
    },
    [previewUrl, shouldRemove],
  );

  return {
    bytes,
    previewUrl,
    mimeType,
    shouldRemove,
    fileInputRef,
    handleFileSelect,
    removeImage,
    undoRemove,
    reset,
    hasImage,
    getEffectiveImageUrl,
  };
}
