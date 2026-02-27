"use client";

import { useState, useCallback, useEffect, useRef } from "react";

export interface UseConfirmDialogResult<T = string> {
  isOpen: boolean;
  itemId: T | null;
  isConfirming: boolean;
  openDialog: (id: T) => void;
  closeDialog: () => void;
  confirm: () => Promise<void>;
}

export interface UseConfirmDialogOptions<T = string> {
  onConfirm: (id: T) => Promise<void>;
  onConfirmComplete?: (id: T) => void;
  onConfirmError?: (id: T, error: unknown) => void;
}

export function useConfirmDialog<T = string>({
  onConfirm,
  onConfirmComplete,
  onConfirmError,
}: UseConfirmDialogOptions<T>): UseConfirmDialogResult<T> {
  const [isOpen, setIsOpen] = useState(false);
  const [itemId, setItemId] = useState<T | null>(null);
  const [isConfirming, setIsConfirming] = useState(false);

  // Keep stable references for callbacks
  const onConfirmRef = useRef(onConfirm);
  const onConfirmCompleteRef = useRef(onConfirmComplete);
  const onConfirmErrorRef = useRef(onConfirmError);

  useEffect(() => {
    onConfirmRef.current = onConfirm;
    onConfirmCompleteRef.current = onConfirmComplete;
    onConfirmErrorRef.current = onConfirmError;
  });

  const openDialog = useCallback((id: T) => {
    setItemId(id);
    setIsOpen(true);
  }, []);

  const closeDialog = useCallback(() => {
    if (isConfirming) return; // Don't allow close while confirming
    setIsOpen(false);
    setItemId(null);
  }, [isConfirming]);

  const confirm = useCallback(async () => {
    if (itemId === null) return;

    const currentId = itemId;
    setIsConfirming(true);

    try {
      await onConfirmRef.current(currentId);
      setIsOpen(false);
      setItemId(null);
      onConfirmCompleteRef.current?.(currentId);
    } catch (error) {
      onConfirmErrorRef.current?.(currentId, error);
    } finally {
      setIsConfirming(false);
    }
  }, [itemId]);

  return {
    isOpen,
    itemId,
    isConfirming,
    openDialog,
    closeDialog,
    confirm,
  };
}
