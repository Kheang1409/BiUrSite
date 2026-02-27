"use client";

import { ReactNode, useEffect, useRef } from "react";
import { Card, Button } from "@/components/ui/atoms";

type ConfirmVariant = "danger" | "primary";

export function ConfirmDialog({
  isOpen,
  title,
  description,
  confirmText = "Delete",
  cancelText = "Cancel",
  confirmVariant = "danger",
  isConfirmLoading = false,
  onConfirm,
  onCancel,
}: {
  isOpen: boolean;
  title: string;
  description?: ReactNode;
  confirmText?: string;
  cancelText?: string;
  confirmVariant?: ConfirmVariant;
  isConfirmLoading?: boolean;
  onConfirm: () => void | Promise<void>;
  onCancel: () => void;
}) {
  const cancelButtonRef = useRef<HTMLButtonElement | null>(null);

  useEffect(() => {
    if (!isOpen) return;

    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onCancel();
    };

    document.addEventListener("keydown", onKeyDown);
    const prevOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    // Focus cancel by default (safer)
    queueMicrotask(() => cancelButtonRef.current?.focus());

    return () => {
      document.removeEventListener("keydown", onKeyDown);
      document.body.style.overflow = prevOverflow;
    };
  }, [isOpen, onCancel]);

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 z-[200] flex items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      aria-label={title}
    >
      <button
        type="button"
        className="absolute inset-0 bg-black/60 backdrop-blur-[2px]"
        aria-label="Close dialog"
        onClick={onCancel}
        disabled={isConfirmLoading}
      />

      <div className="relative w-full max-w-md">
        <Card className="p-0 overflow-hidden border border-white/10 bg-primary-3">
          <div className="px-4 py-3 border-b border-white/10">
            <h3 className="text-white font-bold text-lg">{title}</h3>
          </div>

          {description ? (
            <div className="px-4 py-3 text-sm text-white/80">{description}</div>
          ) : null}

          <div className="px-4 py-3 flex justify-end gap-2 border-t border-white/10">
            <Button
              ref={cancelButtonRef}
              variant="secondary"
              size="sm"
              onClick={onCancel}
              disabled={isConfirmLoading}
            >
              {cancelText}
            </Button>
            <Button
              variant={confirmVariant}
              size="sm"
              isLoading={isConfirmLoading}
              onClick={onConfirm}
            >
              {confirmText}
            </Button>
          </div>
        </Card>
      </div>
    </div>
  );
}
