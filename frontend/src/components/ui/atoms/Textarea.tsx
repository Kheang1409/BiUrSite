import React, { TextareaHTMLAttributes, forwardRef } from "react";

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  error?: string;
  maxLength?: number;
  showCharCount?: boolean;
  fullWidth?: boolean;
}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
  (
    {
      label,
      error,
      maxLength,
      showCharCount = false,
      fullWidth = true,
      className = "",
      value = "",
      ...props
    },
    ref
  ) => {
    const charCount = String(value).length;

    return (
      <div className={fullWidth ? "w-full" : ""}>
        {label && (
          <label className="block text-sm font-medium text-white mb-2">
            {label}
          </label>
        )}
        <textarea
          ref={ref}
          maxLength={maxLength}
          value={value}
          className={[
            "px-4 py-2.5 bg-white/10 border border-white/20 rounded-[10px] text-white placeholder-muted",
            "focus:outline-none focus:border-primary-1 focus:ring-2 focus:ring-primary-1/50",
            "transition-colors duration-200 resize-vertical",
            error && "border-danger-1 focus:ring-danger-1/50",
            fullWidth && "w-full",
            className,
          ]
            .filter(Boolean)
            .join(" ")}
          {...props}
        />
        <div className="flex justify-between items-start mt-2">
          {error && <p className="text-danger-1 text-xs">{error}</p>}
          {showCharCount && maxLength && (
            <p
              className={`text-xs ml-auto ${
                charCount > maxLength * 0.9 ? "text-danger-1" : "text-muted"
              }`}
            >
              {charCount}/{maxLength}
            </p>
          )}
        </div>
      </div>
    );
  }
);

Textarea.displayName = "Textarea";
